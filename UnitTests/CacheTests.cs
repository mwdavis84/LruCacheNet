// Copyright (c) Mark Davis. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using System;
using System.Collections;
using System.Collections.Generic;
using LruCacheNet;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    /// <summary>
    /// Unit tests for the <see cref="LruCache{T}"/> class
    /// </summary>
    [TestClass]
    public class LruCacheTests
    {
        [TestMethod, TestCategory("Cache")]
        public void CreateTooSmall()
        {
            bool thrown = false;
            try
            {
                var cache = new LruCache<string, TestData>(1);
            }
            catch (ArgumentException)
            {
                thrown = true;
            }
            Assert.IsTrue(thrown, "Minimum size ArgumentException not thrown");
        }

        [TestMethod, TestCategory("Cache")]
        public void CreateDefaultSize()
        {
            var cache = new LruCache<string, TestData>();
            Assert.AreEqual(1000, cache.Capacity);
        }

        [TestMethod, TestCategory("Cache")]
        public void CreateCustomSize()
        {
            var cache = new LruCache<string, TestData>(10);
            Assert.AreEqual(10, cache.Capacity);
        }

        [TestMethod, TestCategory("Cache")]
        public void InsertEmpty()
        {
            var cache = new LruCache<string, TestData>(10);
            var data = new TestData
            {
                TestValue1 = "123"
            };
            cache.AddOrUpdate("1", data);
            Assert.AreEqual(1, cache.Count, "Item not added to cache");

            var retrieved = cache.Get("1");
            Assert.IsNotNull(retrieved, "Item not found in cache");
            Assert.AreEqual(data.TestValue1, retrieved.TestValue1, "Items in cache didn't match");
        }

        [TestMethod, TestCategory("Cache")]
        public void InsertNullValue()
        {
            var cache = new LruCache<string, TestData>(10);
            bool thrown = false;
            try
            {
                cache.AddOrUpdate("1", null);
            }
            catch (ArgumentException)
            {
                thrown = true;
            }
            Assert.AreEqual(0, cache.Count, "Null item shouldn't have been added to cache");
            Assert.IsTrue(thrown, "Exception not thrown");
        }

        [TestMethod, TestCategory("Cache")]
        public void InsertNullKey()
        {
            var cache = new LruCache<string, TestData>(10);
            bool thrown = false;
            try
            {
                cache.AddOrUpdate(null, new TestData());
            }
            catch (ArgumentException)
            {
                thrown = true;
            }
            Assert.AreEqual(0, cache.Count, "Null item shouldn't have been added to cache");
            Assert.IsTrue(thrown, "Exception not thrown");
        }

        [TestMethod, TestCategory("Cache")]
        public void GetNotThere()
        {
            var cache = new LruCache<string, TestData>(10);
            bool thrown = false;
            try
            {
                cache.Get("123");
            }
            catch (KeyNotFoundException)
            {
                thrown = true;
            }
            Assert.IsTrue(thrown, "Item shouldn't have been found in cache");
        }

        [TestMethod, TestCategory("Cache")]
        public void InsertOverCapacity()
        {
            var cache = new LruCache<string, TestData>(10);
            for (int index = 0; index <= 10; ++index)
            {
                var data = new TestData
                {
                    TestValue1 = index.ToString()
                };
                cache.AddOrUpdate(index.ToString(), data);
            }

            bool thrown = false;
            try
            {
                cache.Get("0");
            }
            catch (KeyNotFoundException)
            {
                thrown = true;
            }
            Assert.IsTrue(thrown, "Item should have been removed from cache");
        }

        [TestMethod, TestCategory("Cache")]
        public void InsertReorder()
        {
            var cache = new LruCache<string, TestData>(10);
            for (int index = 0; index < 10; ++index)
            {
                var data = new TestData
                {
                    TestValue1 = index.ToString()
                };
                cache.AddOrUpdate(index.ToString(), data);
            }
            Assert.AreEqual(10, cache.Count, "Cache size incorrect");

            cache.AddOrUpdate("0", new TestData());
            Assert.AreEqual(10, cache.Count, "Item shouldn't have duplicated in cache");

            var next = new TestData
            {
                TestValue1 = "11"
            };
            cache.AddOrUpdate(next.TestValue1, next);
            var firstData = cache.Get("0");
            Assert.IsNotNull(firstData, "Item shouldn't have been removed from cache");
        }

        [TestMethod, TestCategory("Cache")]
        public void UpdateItem()
        {
            var cache = new LruCache<string, TestData>(10);
            cache.SetUpdateMethod((a, b) =>
            {
                a.TestValue1 = b.TestValue1;
                a.TestValue2 = b.TestValue2;
                return true;
            });
            cache.SetCopyMethod((a) =>
            {
                return new TestData
                {
                    TestValue1 = a.TestValue1,
                    TestValue2 = a.TestValue2,
                };
            });

            var data = new TestData
            {
                TestValue1 = "1",
                TestValue2 = "2"
            };
            cache.AddOrUpdate("1", data);

            var newData = new TestData
            {
                TestValue1 = "A",
                TestValue2 = "B",
            };
            cache.AddOrUpdate("1", newData);

            var cachedData = cache.Get("1");
            Assert.AreNotEqual(newData.TestValue1, data.TestValue1, "TestValue1 shouldn't match");
            Assert.AreNotEqual(newData.TestValue2, data.TestValue2, "TestValue2 shouldn't match");
            Assert.AreEqual(newData.TestValue1, cachedData.TestValue1, "TestValue1 didn't update");
            Assert.AreEqual(newData.TestValue2, cachedData.TestValue2, "TestValue2 didn't update");
        }

        [TestMethod, TestCategory("Cache")]
        [DataRow("0", DisplayName = "Remove Tail")]
        [DataRow("9", DisplayName = "Remove Head")]
        [DataRow("5", DisplayName = "Remove Middle")]
        public void RemoveItem(string keyToRemove)
        {
            var cache = new LruCache<string, TestData>(10);
            for (int index = 0; index < 10; ++index)
            {
                var test = new TestData
                {
                    TestValue1 = index.ToString()
                };
                cache.AddOrUpdate(test.TestValue1, test);
            }

            Assert.AreEqual(10, cache.Count, "Cache should contain 10 items");
            bool removedData = cache.Remove(keyToRemove);
            Assert.IsTrue(removedData, "Removed data should not be null");
            Assert.AreEqual(9, cache.Count, "Cache should be empty");

            bool removed = cache.Remove(keyToRemove);
            Assert.IsFalse(removed, "Item should not have been removed");
        }

        [TestMethod, TestCategory("Cache")]
        public void RemoveItemNotThere()
        {
            var cache = new LruCache<string, TestData>(10);
            bool removed = cache.Remove("0");
            Assert.IsFalse(removed, "Item should not have been removed");
        }

        [TestMethod, TestCategory("Cache")]
        public void ClearItems()
        {
            var cache = new LruCache<string, TestData>(10);
            for (int index = 0; index < 10; ++index)
            {
                cache.AddOrUpdate(index.ToString(), new TestData());
            }

            Assert.AreEqual(10, cache.Count, "Cache size incorrect before clearing");
            for (int index = 0; index < 10; ++index)
            {
                var item = cache.Get(index.ToString());
                Assert.IsNotNull(item, "Removed item should exist in cache");
            }

            cache.Clear();
            Assert.AreEqual(0, cache.Count, "Cache size incorrect after clearing");
            for (int index = 0; index < 10; ++index)
            {
                bool removed = cache.Remove(index.ToString());
                Assert.IsFalse(removed, "Removed item shouldn't exist in cache");
            }
        }

        [TestMethod, TestCategory("Cache")]
        [DataRow(true, DisplayName = "Item Exists")]
        [DataRow(false, DisplayName = "Item Not Exists")]
        public void ContainsTest(bool shouldExist)
        {
            var cache = new LruCache<string, TestData>(10);
            if (shouldExist)
            {
                cache.AddOrUpdate("1", new TestData());
            }
            bool exists = cache.ContainsKey("1");
            Assert.AreEqual(shouldExist, exists);
        }

        [TestMethod, TestCategory("Cache")]
        public void PeekTest()
        {
            var cache = new LruCache<string, TestData>(10);
            for (int index = 0; index < 10; ++index)
            {
                cache.AddOrUpdate(index.ToString(), new TestData());
            }
            Assert.AreEqual(10, cache.Count, "Cache size incorrect");

            var cached = cache.Peek("0");
            Assert.IsNotNull(cached, "Item should exist in cache");

            cache.AddOrUpdate("11", new TestData());
            bool thrown = false;
            try
            {
                cache.Get("0");
            }
            catch (KeyNotFoundException)
            {
                thrown = true;
            }
            Assert.IsTrue(thrown, "Item should have been removed from cache");
        }

        [TestMethod, TestCategory("Cache")]
        public void PeekNotExists()
        {
            var cache = new LruCache<string, TestData>(10);
            for (int index = 0; index < 10; ++index)
            {
                cache.AddOrUpdate(index.ToString(), new TestData());
            }
            Assert.AreEqual(10, cache.Count, "Cache size incorrect");

            bool thrown = false;
            try
            {
                cache.Peek("11");
            }
            catch (KeyNotFoundException)
            {
                thrown = true;
            }
            Assert.IsTrue(thrown, "Item should not exist in cache");
        }

        [TestMethod, TestCategory("Cache")]
        public void RefreshTest()
        {
            var cache = new LruCache<string, TestData>(10);
            for (int index = 0; index < 10; ++index)
            {
                cache.AddOrUpdate(index.ToString(), new TestData());
            }
            Assert.AreEqual(10, cache.Count, "Cache size incorrect");

            bool cached = cache.Refresh("0");
            Assert.IsTrue(cached, "Item should have refreshed in cache");

            cache.AddOrUpdate("11", new TestData());
            var firstData = cache.Get("0");
            Assert.IsNotNull(firstData, "Item should't have been removed from cache");
        }

        [TestMethod, TestCategory("Cache")]
        public void RefreshNotExists()
        {
            var cache = new LruCache<string, TestData>(10);
            cache.AddOrUpdate("0", new TestData());
            bool check = cache.Refresh("1");
            Assert.IsFalse(check, "Item should not have refreshed in cache");
        }

        [TestMethod]
        public void InsertsAndGets()
        {
            var cache = new LruCache<int, int>(2);
            cache.AddOrUpdate(1, 1);
            cache.AddOrUpdate(2, 2);

            int retrieved = cache.Get(1);
            Assert.AreEqual(1, retrieved);

            cache.AddOrUpdate(3, 3);
            bool thrown = false;
            try
            {
                cache.Get(2);
            }
            catch (KeyNotFoundException)
            {
                thrown = true;
            }
            Assert.IsTrue(thrown);

            cache.AddOrUpdate(4, 4);
            thrown = false;
            try
            {
                cache.Get(1);
            }
            catch (KeyNotFoundException)
            {
                thrown = true;
            }
            Assert.IsTrue(thrown);

            retrieved = cache.Get(3);
            Assert.AreEqual(3, retrieved);
            retrieved = cache.Get(4);
            Assert.AreEqual(4, retrieved);
        }

        [TestMethod]
        public void TryGetFound()
        {
            var cache = new LruCache<int, int>(10);
            cache.AddOrUpdate(1, 1);

            bool result = cache.TryGetValue(1, out int data);
            Assert.IsTrue(result, "Item should have been found");
            Assert.AreEqual(1, data, "Data should match");
        }

        [TestMethod]
        public void TryGetNotFound()
        {
            var cache = new LruCache<int, string>(10);
            cache.AddOrUpdate(1, "1");

            bool result = cache.TryGetValue(2, out string data);
            Assert.IsFalse(result, "Item should not have been found");
            Assert.IsNull(data, "Data should be null");
        }

        [TestMethod]
        public void TryPeekFound()
        {
            var cache = new LruCache<int, int>(10);
            cache.AddOrUpdate(1, 1);

            bool result = cache.TryPeek(1, out int data);
            Assert.IsTrue(result, "Item should have been found");
            Assert.AreEqual(1, data, "Data should match");
        }

        [TestMethod]
        public void TryPeekNotFound()
        {
            var cache = new LruCache<int, string>(10);
            cache.AddOrUpdate(1, "1");

            bool result = cache.TryPeek(2, out string data);
            Assert.IsFalse(result, "Item should not have been found");
            Assert.IsNull(data, "Data should be null");
        }

        [TestMethod]
        public void ToList()
        {
            var cache = new LruCache<int, int>(10);
            for (int index = 0; index < 10; ++index)
            {
                cache.AddOrUpdate(index, index);
            }

            List<int> items = cache.ToList();
            Assert.AreEqual(cache.Count, items.Count, "List size should match cache size");
            for (int index = 0; index < 10; ++index)
            {
                Assert.AreEqual(9 - index, items[index], "List not in expected order");
            }
        }

        [TestMethod]
        public void CacheEnumerator()
        {
            var cache = new LruCache<int, int>(10);
            for (int index = 0; index < 10; ++index)
            {
                cache.AddOrUpdate(index, index);
            }

            int testIndex = 9;
            foreach (var item in cache)
            {
                Assert.AreEqual(testIndex, item.Key, "Key does not match");
                Assert.AreEqual(testIndex, item.Value, "Value does not match");
                --testIndex;
            }
        }

        [TestMethod]
        public void CacheGenericEnumerator()
        {
            var cache = new LruCache<int, int>(10);
            for (int index = 0; index < 10; ++index)
            {
                cache.AddOrUpdate(index, index);
            }

            int testIndex = 9;
            IEnumerable plainDict = (IEnumerable)cache;
            var enumerator = plainDict.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var item = (KeyValuePair<int, int>)enumerator.Current;
                Assert.AreEqual(testIndex, item.Key, "Key does not match");
                Assert.AreEqual(testIndex, item.Value, "Value does not match");
                --testIndex;
            }
        }

        [TestMethod]
        public void CacheGetIndexer()
        {
            var cache = new LruCache<int, int>(10);
            for (int index = 0; index < 10; ++index)
            {
                cache.AddOrUpdate(index, index);
            }

            for (int index = 0; index < 10; ++index)
            {
                int testIndex = cache[index];
                int testGet = cache.Get(index);
                Assert.AreEqual(index, testIndex, "Index get isn't correct value");
                Assert.AreEqual(testIndex, testGet, "Index get doesn't match method get");
            }
        }

        [TestMethod]
        public void CacheSetIndexer()
        {
            var cache = new LruCache<int, int>(10);
            for (int index = 0; index < 10; ++index)
            {
                cache[index] = index;
                int test = cache.Get(index);
                Assert.AreEqual(index, test, "Value not correctly inserted");
            }

            for (int index = 0; index < 10; ++index)
            {
                cache[index] = 9 - index;
                int test = cache.Get(index);
                Assert.AreEqual(9 - index, test, "Updated value not correctly inserted");
            }
        }

        [TestMethod]
        public void CacheAdd()
        {
            var cache = new LruCache<int, int>(10);
            for (int index = 0; index < 10; ++index)
            {
                cache.Add(index, index);
                int test = cache.Get(index);
                Assert.AreEqual(index, test, "Item not correctly added");
            }

            bool thrown = false;
            try
            {
                cache.Add(0, 0);
            }
            catch (ArgumentException)
            {
                thrown = true;
            }
            Assert.IsTrue(thrown, "Item should not have been added");
        }

        [TestMethod]
        public void CacheAddKeyValuePair()
        {
            var cache = new LruCache<int, int>(10);
            for (int index = 0; index < 10; ++index)
            {
                cache.Add(new KeyValuePair<int, int>(index, index));
                int test = cache.Get(index);
                Assert.AreEqual(index, test, "Item not correctly added");
            }

            bool thrown = false;
            try
            {
                cache.Add(new KeyValuePair<int, int>(0, 0));
            }
            catch (ArgumentException)
            {
                thrown = true;
            }
            Assert.IsTrue(thrown, "Item should not have been added");
        }

        [TestMethod]
        public void ContainsKeyValuePair()
        {
            var cache = new LruCache<int, int>(10);
            for (int index = 0; index < 10; ++index)
            {
                cache.Add(new KeyValuePair<int, int>(index, index));
            }

            bool found = cache.Contains(new KeyValuePair<int, int>(0, 0));
            Assert.IsTrue(found, "Item should have been found");
        }

        [TestMethod]
        public void ContainsKeyNotValue()
        {
            var cache = new LruCache<int, int>(10);
            for (int index = 0; index < 10; ++index)
            {
                cache.Add(new KeyValuePair<int, int>(index, index));
            }

            bool found = cache.Contains(new KeyValuePair<int, int>(0, 1));
            Assert.IsFalse(found, "Item not should have been found");
        }

        [TestMethod]
        public void ContainsKeyValuePairNotFound()
        {
            var cache = new LruCache<int, int>(10);
            for (int index = 0; index < 10; ++index)
            {
                cache.Add(new KeyValuePair<int, int>(index, index));
            }

            bool found = cache.Contains(new KeyValuePair<int, int>(11, 12));
            Assert.IsFalse(found, "Item not should have been found");
        }

        [TestMethod]
        public void RemoveKeyValuePair()
        {
            var cache = new LruCache<int, int>(10);
            for (int index = 0; index < 10; ++index)
            {
                cache.Add(new KeyValuePair<int, int>(index, index));
            }

            bool found = cache.Remove(new KeyValuePair<int, int>(0, 0));
            Assert.IsTrue(found, "Item should have been found");
        }

        [TestMethod]
        public void RemoveKeyNotValue()
        {
            var cache = new LruCache<int, int>(10);
            for (int index = 0; index < 10; ++index)
            {
                cache.Add(new KeyValuePair<int, int>(index, index));
            }

            bool found = cache.Remove(new KeyValuePair<int, int>(0, 1));
            Assert.IsFalse(found, "Item not should have been found");
        }

        [TestMethod]
        public void RemoveKeyValuePairNotFound()
        {
            var cache = new LruCache<int, int>(10);
            for (int index = 0; index < 10; ++index)
            {
                cache.Add(new KeyValuePair<int, int>(index, index));
            }

            bool found = cache.Remove(new KeyValuePair<int, int>(11, 12));
            Assert.IsFalse(found, "Item not should have been found");
        }

        [TestMethod]
        public void CacheCopyTo()
        {
            var cache = new LruCache<int, int>(10);
            for (int index = 0; index < 10; ++index)
            {
                cache.Add(new KeyValuePair<int, int>(index, index));
            }

            KeyValuePair<int, int>[] array = new KeyValuePair<int, int>[10];
            cache.CopyTo(array, 0);

            for (int index = 0; index < array.Length; ++index)
            {
                Assert.AreEqual(9 - index, array[index].Key, "Keys don't match");
                Assert.AreEqual(9 - index, array[index].Value, "Values don't match");
            }
        }

        [TestMethod]
        public void CacheCopyToWithIndex()
        {
            var cache = new LruCache<int, int>(10);
            for (int index = 0; index < 10; ++index)
            {
                cache.Add(new KeyValuePair<int, int>(index, index));
            }

            KeyValuePair<int, int>[] array = new KeyValuePair<int, int>[20];
            cache.CopyTo(array, 10);

            for (int index = 0; index < cache.Count; ++index)
            {
                Assert.AreEqual(9 - index, array[index + 10].Key, "Keys don't match");
                Assert.AreEqual(9 - index, array[index + 10].Value, "Values don't match");
            }
        }

        [TestMethod]
        public void CacheCopyToTooSmall()
        {
            var cache = new LruCache<int, int>(10);
            for (int index = 0; index < 10; ++index)
            {
                cache.Add(new KeyValuePair<int, int>(index, index));
            }

            KeyValuePair<int, int>[] array = new KeyValuePair<int, int>[5];

            bool thrown = false;
            try
            {
                cache.CopyTo(array, 0);
            }
            catch (IndexOutOfRangeException)
            {
                thrown = true;
            }
            Assert.IsTrue(thrown, "Exception should have been thrown");
        }

        [TestMethod]
        public void CacheReadOnly()
        {
            var cache = new LruCache<int, int>(10);
            Assert.IsFalse(cache.IsReadOnly, "Cache should not be read only");
        }

        [TestMethod]
        public void CacheKeys()
        {
            var cache = new LruCache<int, int>(10);
            for (int index = 0; index < 10; ++index)
            {
                cache.Add(new KeyValuePair<int, int>(index, index));
            }

            bool[] found = new bool[10];
            foreach (int key in cache.Keys)
            {
                found[key] = true;
            }

            for (int index = 0; index < found.Length; ++index)
            {
                Assert.IsTrue(found[index], $"Index {index} wasn't found");
            }
        }

        [TestMethod]
        public void CacheValues()
        {
            var cache = new LruCache<int, int>(10);
            for (int index = 0; index < 10; ++index)
            {
                cache.Add(new KeyValuePair<int, int>(index, index));
            }

            bool[] found = new bool[10];
            foreach (int key in cache.Values)
            {
                found[key] = true;
            }

            for (int index = 0; index < found.Length; ++index)
            {
                Assert.IsTrue(found[index], $"Index {index} wasn't found");
            }
        }
    }
}
