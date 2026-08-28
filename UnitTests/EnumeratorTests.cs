// Copyright (c) Mark Davis. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using LruCacheNet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;

namespace UnitTests
{
    /// <summary>
    /// Tests the <see cref="IEnumerator{T}"/> returned by LruCache (Snapshot behavior)
    /// </summary>
    [TestClass]
    public class EnumeratorTests
    {
        /// <summary>
        /// Tests regular enumeration through the list
        /// </summary>
        [TestMethod, TestCategory("Enumeartor")]
        public void EnumeratorList()
        {
            var cache = new LruCache<int, int>(10);
            for (int i = 0; i < 10; i++) cache.Add(i, i);

            var enumerator = cache.GetEnumerator();
            int index = 0;
            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;
                // LruCache iterates MRU to LRU (Last added is first)
                Assert.AreEqual(9 - index, current.Key, "Incorrect key");
                Assert.AreEqual(9 - index, current.Value, "Incorrect value");
                ++index;
            }
        }

        /// <summary>
        /// Tests resetting an enumerator
        /// </summary>
        [TestMethod, TestCategory("Enumeartor")]
        public void EnumeratorResetList()
        {
            var cache = new LruCache<int, int>(10);
            for (int i = 0; i < 10; i++) cache.Add(i, i);

            var enumerator = cache.GetEnumerator();
            int index = 0;
            while (index <= 5)
            {
                enumerator.MoveNext();
                var current = enumerator.Current;
                Assert.AreEqual(9 - index, current.Key);
                ++index;
            }

            enumerator.Reset();
            enumerator.MoveNext();
            var start = enumerator.Current;
            Assert.AreEqual(9, start.Key, "Incorrect key after reset");
        }

        /// <summary>
        /// Tests an enumerator that's been disposed
        /// </summary>
        [TestMethod, TestCategory("Enumeartor")]
        public void EnumeratorDisposeTest()
        {
             var cache = new LruCache<int, int>(10);
            for (int i = 0; i < 10; i++) cache.Add(i, i);

            var enumerator = cache.GetEnumerator();
            enumerator.Dispose();
            
            // List<T>.Enumerator behavior on Dispose is implementation detail, usually just clears reference.
            // We mainly want to ensure no crash.
             try { enumerator.MoveNext(); } catch { }
        }

        [TestMethod, TestCategory("Enumeartor")]
        public void EnumeratorGetCurrent()
        {
             var cache = new LruCache<int, int>(10);
            for (int i = 0; i < 10; i++) cache.Add(i, i);

            var enumerator = cache.GetEnumerator();
            var plainEnumerator = (IEnumerator)enumerator;

            enumerator.MoveNext();
            var current = enumerator.Current;
            var plainCurrent = plainEnumerator.Current;
            Assert.AreEqual(current, plainCurrent);
        }

        [TestMethod, TestCategory("Enumeartor")]
        public void EnumeratorChangedAdded()
        {
            var cache = new LruCache<int, int>(10);
            for (int index = 0; index < 5; ++index) cache.Add(index, index);

            var enumerator = cache.GetEnumerator();
            enumerator.MoveNext(); // Start

            cache.Add(99, 99); // Modify

            CheckSnapshotStable(enumerator); 
        }

        [TestMethod, TestCategory("Enumeartor")]
        public void EnumeratorChangedUpdateValue()
        {
            var cache = new LruCache<int, int>(10);
            for (int index = 0; index < 5; ++index) cache.Add(index, index);

            var enumerator = cache.GetEnumerator();
            enumerator.MoveNext();

            cache.AddOrUpdate(0, 999);

            CheckSnapshotStable(enumerator);
        }

        [TestMethod, TestCategory("Enumeartor")]
        public void EnumeratorChangedUpdateMethod()
        {
            var cache = new LruCache<int, int>(10);
            cache.SetUpdateMethod((a, b) => true);
            for (int index = 0; index < 5; ++index) cache.Add(index, index);

            var enumerator = cache.GetEnumerator();
            enumerator.MoveNext();

            cache.AddOrUpdate(0, 999);

            CheckSnapshotStable(enumerator);
        }

        [TestMethod, TestCategory("Enumeartor")]
        public void EnumeratorChangedUpdateMethodNoChange()
        {
            var cache = new LruCache<int, int>(10);
            cache.SetUpdateMethod((a, b) => false);
            for (int index = 0; index < 5; ++index) cache.Add(index, index);

            var enumerator = cache.GetEnumerator();
            enumerator.MoveNext();

            cache.AddOrUpdate(0, 999);

            CheckSnapshotStable(enumerator);
        }

        [TestMethod, TestCategory("Enumeartor")]
        public void EnumeratorChangedRemoved()
        {
            var cache = new LruCache<int, int>(10);
            for (int index = 0; index < 5; ++index) cache.Add(index, index);

            var enumerator = cache.GetEnumerator();
            enumerator.MoveNext();

            cache.Remove(0);

            CheckSnapshotStable(enumerator);
        }

        [TestMethod, TestCategory("Enumeartor")]
        public void EnumeratorChangedGet()
        {
            var cache = new LruCache<int, int>(10);
            for (int index = 0; index < 5; ++index) cache.Add(index, index);

            var enumerator = cache.GetEnumerator();
            enumerator.MoveNext();

            cache.Get(4); // Reorder

            CheckSnapshotStable(enumerator);
        }

        [TestMethod, TestCategory("Enumeartor")]
        public void EnumeratorChangedRefresh()
        {
            var cache = new LruCache<int, int>(10);
            for (int index = 0; index < 5; ++index) cache.Add(index, index);

            var enumerator = cache.GetEnumerator();
            enumerator.MoveNext();

            cache.Refresh(0);

            CheckSnapshotStable(enumerator);
        }

        [TestMethod, TestCategory("Enumeartor")]
        public void EnumeratorChangedRefreshNoMove()
        {
            var cache = new LruCache<int, int>(10);
            for (int index = 0; index < 5; ++index) cache.Add(index, index);

            var enumerator = cache.GetEnumerator();
            enumerator.MoveNext();

            cache.Refresh(4); // Tail?

            CheckSnapshotStable(enumerator);
        }

        [TestMethod, TestCategory("Enumeartor")]
        public void EnumeratorChangedClear()
        {
            var cache = new LruCache<int, int>(10);
            for (int index = 0; index < 5; ++index) cache.Add(index, index);

            var enumerator = cache.GetEnumerator();
            enumerator.MoveNext();

            cache.Clear();

            CheckSnapshotStable(enumerator);
        }

        private void CheckSnapshotStable(IEnumerator<KeyValuePair<int, int>> enumerator)
        {
            // Should NOT throw
            var current = enumerator.Current;
            
            // Should be able to finish iterating
            while (enumerator.MoveNext()) { }
            
            // Should be table to Reset (List enumerator supports reset)
            enumerator.Reset();
        }
    }
}
