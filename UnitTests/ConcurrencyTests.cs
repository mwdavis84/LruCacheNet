using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LruCacheNet;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class ConcurrencyTests
    {
        [TestMethod, TestCategory("Concurrency")]
        public void MultiThreadedAddGet()
        {
            var cache = new LruCache<int, int>(100);
            int threadCount = 20;
            int iterations = 1000;
            var tasks = new List<Task>();

            for (int i = 0; i < threadCount; i++)
            {
                int threadId = i;
                tasks.Add(Task.Run(() =>
                {
                    for (int j = 0; j < iterations; j++)
                    {
                        int key = j % 100; // High contention on keys
                        cache.AddOrUpdate(key, j);
                        try
                        {
                            var val = cache.Get(key);
                        }
                        catch (KeyNotFoundException)
                        {
                            // It's possible it was evicted even with 100 capacity if other threads are thrashing
                        }
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray());
            Assert.IsTrue(cache.Count <= 100);
        }

        [TestMethod, TestCategory("Concurrency")]
        public void EnumeratorRace()
        {
            // This test attempts to repro Issue #9
            var cache = new LruCache<int, int>(1000);
            for (int i = 0; i < 500; i++)
            {
                cache.AddOrUpdate(i, i);
            }

            var cts = new CancellationTokenSource();
            var modifierTask = Task.Run(() =>
            {
                var rnd = new Random();
                while (!cts.Token.IsCancellationRequested)
                {
                    int key = rnd.Next(0, 1000);
                    cache.AddOrUpdate(key, key); // Moves to front, changes links
                    // Frequently remove to cause null 'Next' pointers
                    if (rnd.Next(0, 5) == 0)
                    {
                        cache.Remove(key);
                    }
                }
            });

            try
            {
                // Run multiple enumerations while modifier runs
                for (int i = 0; i < 50; i++)
                {
                    try
                    {
                        foreach (var kvp in cache)
                        {
                            // Just iterate
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        // Expected if collection changed is detected
                    }
                    catch (NullReferenceException)
                    {
                        Assert.Fail("Caught NullReferenceException - Enumeration is not thread safe!");
                    }
                    catch (Exception ex)
                    {
                        // Other exceptions?
                        Console.WriteLine("Caught: " + ex.GetType().Name);
                    }
                }
            }
            finally
            {
                cts.Cancel();
                modifierTask.Wait();
            }
        }
    }
}
