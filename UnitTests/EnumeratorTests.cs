// Copyright (c) Mark Davis. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using LruCacheNet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;

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
        [TestMethod]
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
        [TestMethod]
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
        [TestMethod]
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
        [TestMethod]
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
        [TestMethod]
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
