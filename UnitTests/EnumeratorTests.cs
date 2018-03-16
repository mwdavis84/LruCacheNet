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
    /// Tests the <see cref="CacheEnumerator{TKey, TValue}"/> class
    /// </summary>
    [TestClass]
    public class EnumeratorTests
    {
        /// <summary>
        /// Tests creating an enumerator with a null node
        /// </summary>
        [TestMethod, TestCategory("Enumeartor")]
        public void EnumeratorNullList()
        {
            bool thrown = false;
            try
            {
                var enumerator = new CacheEnumerator<int, int>(null);
            }
            catch (ArgumentException)
            {
                thrown = true;
            }
            Assert.IsTrue(thrown, "Exception should be thrown");
        }

        /// <summary>
        /// Tests regular enumeration through the list
        /// </summary>
        [TestMethod, TestCategory("Enumeartor")]
        public void EnumeratorList()
        {
            var head = CreateTestList();
            var enumerator = new CacheEnumerator<int, int>(head);
            int index = 0;
            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;
                Assert.IsNotNull(current, "Node shouldn't be null");
                Assert.AreEqual(index, current.Key, "Incorrect key");
                Assert.AreEqual(index, current.Value, "Incorrect value");
                ++index;
            }
        }

        /// <summary>
        /// Tests resetting an enumerator
        /// </summary>
        [TestMethod, TestCategory("Enumeartor")]
        public void EnumeratorResetList()
        {
            var head = CreateTestList();
            var enumerator = new CacheEnumerator<int, int>(head);
            int index = 0;
            while (index <= 5)
            {
                enumerator.MoveNext();
                var current = enumerator.Current;
                Assert.IsNotNull(current, "Node shouldn't be null");
                Assert.AreEqual(index, current.Key, "Incorrect key");
                Assert.AreEqual(index, current.Value, "Incorrect value");
                ++index;
            }

            enumerator.Reset();
            enumerator.MoveNext();
            var start = enumerator.Current;
            Assert.IsNotNull(start, "Node shouldn't be null");
            Assert.AreEqual(0, start.Key, "Incorrect key");
            Assert.AreEqual(0, start.Value, "Incorrect value");
        }

        /// <summary>
        /// Tests an enumerator that's been disposed
        /// </summary>
        [TestMethod, TestCategory("Enumeartor")]
        public void EnumeratorDisposeTest()
        {
            var head = CreateTestList();
            var enumerator = new CacheEnumerator<int, int>(head);
            enumerator.Dispose();

            bool thrown = false;
            try
            {
                enumerator.MoveNext();
            }
            catch (ObjectDisposedException)
            {
                thrown = true;
            }
            Assert.IsTrue(thrown, "Exception should have thrown on MoveNext");

            thrown = false;
            try
            {
                var current = enumerator.Current;
            }
            catch (ObjectDisposedException)
            {
                thrown = true;
            }
            Assert.IsTrue(thrown, "Exception should have thrown on Current");
        }

        /// <summary>
        /// Tests the current property in the enumerator
        /// </summary>
        [TestMethod, TestCategory("Enumeartor")]
        public void EnumeratorGetCurrent()
        {
            var head = CreateTestList();
            var enumerator = new CacheEnumerator<int, int>(head);
            var plainEnumerator = (IEnumerator)enumerator;

            enumerator.MoveNext();
            var current = enumerator.Current;
            var plainCurrent = plainEnumerator.Current;
            Assert.AreEqual(current, plainCurrent);
        }

        /// <summary>
        /// Tests when an item is added to the collection
        /// </summary>
        [TestMethod, TestCategory("Enumeartor")]
        public void EnumeratorChangedAdded()
        {
            var cache = new LruCache<int, int>(10);
            for (int index = 0; index < 10; ++index)
            {
                cache.Add(index, index);
            }

            var enumerator = cache.GetEnumerator();
            bool success = enumerator.MoveNext();
            Assert.IsTrue(success, "Enumerator should move");

            cache.Add(11, 11);

            CheckForInvalidEnumerator(enumerator);
        }

        /// <summary>
        /// Tests when an item in the collection updates without an update method
        /// </summary>
        [TestMethod, TestCategory("Enumeartor")]
        public void EnumeratorChangedUpdateValue()
        {
            var cache = new LruCache<int, int>(10);
            for (int index = 0; index < 10; ++index)
            {
                cache.Add(index, index);
            }

            var enumerator = cache.GetEnumerator();
            bool success = enumerator.MoveNext();
            Assert.IsTrue(success, "Enumerator should move");

            cache.AddOrUpdate(5, 4);

            CheckForInvalidEnumerator(enumerator);
        }

        /// <summary>
        /// Tests when an item is updated in the underlying collection with an update method
        /// </summary>
        [TestMethod, TestCategory("Enumeartor")]
        public void EnumeratorChangedUpdateMethod()
        {
            var cache = new LruCache<int, int>(10);
            cache.SetUpdateMethod((a, b) =>
            {
                if (a != b)
                {
                    a = b;
                    return true;
                }
                else
                {
                    return false;
                }
            });
            for (int index = 0; index < 10; ++index)
            {
                cache.Add(index, index);
            }

            var enumerator = cache.GetEnumerator();
            bool success = enumerator.MoveNext();
            Assert.IsTrue(success, "Enumerator should move");

            cache.AddOrUpdate(5, 4);

            CheckForInvalidEnumerator(enumerator);
        }

        /// <summary>
        /// Tests when an item update is called, but the item is not updated
        /// </summary>
        [TestMethod, TestCategory("Enumeartor")]
        public void EnumeratorChangedUpdateMethodNoChange()
        {
            var cache = new LruCache<int, int>(10);
            cache.SetUpdateMethod((a, b) =>
            {
                if (a != b)
                {
                    a = b;
                    return true;
                }
                else
                {
                    return false;
                }
            });
            for (int index = 0; index < 10; ++index)
            {
                cache.Add(index, index);
            }

            var enumerator = cache.GetEnumerator();
            bool success = enumerator.MoveNext();
            Assert.IsTrue(success, "Enumerator should move");

            cache.AddOrUpdate(9, 9);
            success = enumerator.MoveNext();
            Assert.IsTrue(success, "Enumerator should move");
        }

        /// <summary>
        /// Tests when an item is removed from the collection
        /// </summary>
        [TestMethod, TestCategory("Enumeartor")]
        public void EnumeratorChangedRemoved()
        {
            var cache = new LruCache<int, int>(10);
            for (int index = 0; index < 10; ++index)
            {
                cache.Add(index, index);
            }

            var enumerator = cache.GetEnumerator();
            bool success = enumerator.MoveNext();
            Assert.IsTrue(success, "Enumerator should move");

            cache.Remove(0);

            CheckForInvalidEnumerator(enumerator);
        }

        /// <summary>
        /// Tests when an item is retrieved from the collection and its position changes
        /// </summary>
        [TestMethod, TestCategory("Enumeartor")]
        public void EnumeratorChangedGet()
        {
            var cache = new LruCache<int, int>(10);
            for (int index = 0; index < 10; ++index)
            {
                cache.Add(index, index);
            }

            var enumerator = cache.GetEnumerator();
            bool success = enumerator.MoveNext();
            Assert.IsTrue(success, "Enumerator should move");

            cache.Get(0);

            CheckForInvalidEnumerator(enumerator);
        }

        /// <summary>
        /// Tests when an item is refreshed in the collection and its position changes
        /// </summary>
        [TestMethod, TestCategory("Enumeartor")]
        public void EnumeratorChangedRefresh()
        {
            var cache = new LruCache<int, int>(10);
            for (int index = 0; index < 10; ++index)
            {
                cache.Add(index, index);
            }

            var enumerator = cache.GetEnumerator();
            bool success = enumerator.MoveNext();
            Assert.IsTrue(success, "Enumerator should move");

            cache.Refresh(0);

            CheckForInvalidEnumerator(enumerator);
        }

        /// <summary>
        /// Tests when an item is refreshed in the collection and its position does not change
        /// </summary>
        [TestMethod, TestCategory("Enumeartor")]
        public void EnumeratorChangedRefreshNoMove()
        {
            var cache = new LruCache<int, int>(10);
            for (int index = 0; index < 10; ++index)
            {
                cache.Add(index, index);
            }

            var enumerator = cache.GetEnumerator();
            bool success = enumerator.MoveNext();
            Assert.IsTrue(success, "Enumerator should move");

            cache.Refresh(9);
            success = enumerator.MoveNext();
            Assert.IsTrue(success, "Enumerator should move");
        }

        /// <summary>
        /// Tests when the collection is cleared
        /// </summary>
        [TestMethod, TestCategory("Enumeartor")]
        public void EnumeratorChangedClear()
        {
            var cache = new LruCache<int, int>(10);
            for (int index = 0; index < 10; ++index)
            {
                cache.Add(index, index);
            }

            var enumerator = cache.GetEnumerator();
            bool success = enumerator.MoveNext();
            Assert.IsTrue(success, "Enumerator should move");

            cache.Clear();

            CheckForInvalidEnumerator(enumerator);
        }

        /// <summary>
        /// Checks to ensure that all expected enumerator methods fail after the collection has updated
        /// </summary>
        /// <param name="enumerator">Enumerator to check</param>
        private void CheckForInvalidEnumerator(IEnumerator<KeyValuePair<int, int>> enumerator)
        {
            bool thrown = false;
            try
            {
                var current = enumerator.Current;
            }
            catch (InvalidOperationException)
            {
                thrown = true;
            }
            Assert.IsTrue(thrown, "Exception should be thrown accessing Current");

            thrown = false;
            try
            {
                enumerator.MoveNext();
            }
            catch (InvalidOperationException)
            {
                thrown = true;
            }
            Assert.IsTrue(thrown, "Exception should be thrown calling MoveNext");

            thrown = false;
            try
            {
                enumerator.Reset();
            }
            catch (InvalidOperationException)
            {
                thrown = true;
            }
            Assert.IsTrue(thrown, "Exception should be thrown calling Reset");
        }

        /// <summary>
        /// Creats a test linked list of nodes
        /// </summary>
        /// <returns>Head of the list</returns>
        private Node<int, int> CreateTestList()
        {
            Node<int, int> head = null;
            Node<int, int> current = null;
            for (int index = 0; index < 10; ++index)
            {
                var node = new Node<int, int>(index, index);
                if (head == null)
                {
                    head = node;
                    current = node;
                }
                else
                {
                    current.Next = node;
                    current = node;
                }
            }
            return head;
        }
    }
}
