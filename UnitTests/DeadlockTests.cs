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
                // This call should succeed due to re-entrancy and no deadlock
                int count = cache.Count; 
                return true;
            });

            cache.AddOrUpdate(1, 1);
            cache.AddOrUpdate(1, 2); // Triggers update
            // If we reach here, no deadlock.
        }

        [TestMethod, TestCategory("Concurrency")]
        [Timeout(5000)]
        public void EventDeadlock_Wait()
        {
            // Scenario: CollectionChanged handler waits for a signal that is blocked by the cache lock
            var cache = new LruCache<int, int>(10);

            cache.CollectionChanged += (s, e) =>
            {
                // This runs OUTSIDE the cache lock now.
                
                // Let's spawn a task that needs the lock, and wait for it.
                var t = Task.Run(() =>
                {
                    // Attempt to acquire lock. Use Count to avoid infinite recursion of events.
                    var c = cache.Count;
                });
                
                // Wait for that task to finish. 
                // Since we don't hold the lock here anymore, t can acquire it, finish, and we can proceed.
                 if (!t.Wait(1000))
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
                      Assert.Fail("Deadlock detected (Timeout)");
                 }
                 throw;
            }
             catch (TimeoutException)
            {
                Assert.Fail("Deadlock detected (Timeout)");
            }
            // Success if we get here
        }
    }
}
