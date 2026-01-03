using System;
using System.Threading;
using System.Threading.Tasks;
using LruCacheNet;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class DeadlockTests
    {
        [TestMethod, TestCategory("Concurrency")]
        [Timeout(2000)] // Fail if deadlock (hangs longer than 2s)
        public void CallbackDeadlock_Update()
        {
            var cache = new LruCache<int, int>(10);
            
            // Set an update method that tries to access the cache again
            cache.SetUpdateMethod((oldVal, newVal) =>
            {
                // Accessing cache property or method inside lock
                // This is re-entrant on the SAME thread, so standard 'lock' allows it.
                // However, let's simulate a more complex scenario where we wait for another task
                // that needs the lock.
                
                // Simple re-entrancy works in C# lock. We need Thread A holding Lock, waiting for Thread B.
                // Thread B needs Lock.
                
                // Scenario: Update triggers an event on another thread? 
                // Alternatively, just prove that 'UpdateMethod' runs inside the lock.
                
                // This call should succeed due to re-entrancy
                int count = cache.Count; 
                return true;
            });

            cache.AddOrUpdate(1, 1);
            cache.AddOrUpdate(1, 2); // Triggers update
        }

        [TestMethod, TestCategory("Concurrency")]
        [Timeout(5000)]
        public void EventDeadlock_Wait()
        {
            // Scenario: CollectionChanged handler waits for a signal that is blocked by the cache lock
            var cache = new LruCache<int, int>(10);
            var signal = new ManualResetEventSlim(false);

            cache.CollectionChanged += (s, e) =>
            {
                // This runs inside the cache lock.
                // If we wait for something here, we hold the lock.
                // If the thing we wait for needs the lock, we deadlock.
                
                // Let's spawn a task that needs the lock, and wait for it.
                var t = Task.Run(() =>
                {
                    // Attempt to acquire lock
                    cache.Get(1);
                });
                
                // Wait for that task to finish
                // t cannot finish because it needs the lock we are holding.
                // We cannot finish because we are waiting for t.
                // --> Deadlock.
                 if (!t.Wait(100))
                 {
                     throw new TimeoutException("Deadlock detected!");
                 }
            };

            try 
            {
               cache.AddOrUpdate(1, 1);
            }
            catch (AggregateException ae)
            {
                if (ae.InnerException is TimeoutException)
                {
                    // Success validation of deadlock potential
                    return;
                }
                throw;
            }
             catch (TimeoutException)
            {
                // Success validation of deadlock potential
                return;
            }
            Assert.Fail("Should have deadlocked or timed out");
        }
    }
}
