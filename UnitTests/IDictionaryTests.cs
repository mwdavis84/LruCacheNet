// Copyright (c) Mark Davis. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using LruCacheNet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace UnitTests
{
    [TestClass]
    public class IDictionaryTests
    {
        /// <summary>
        /// Tests that a handful of IDictionary interface methods work
        /// </summary>
        [TestMethod, TestCategory("IDictionary")]
        public void DictionaryTests()
        {
            IDictionary<int, int> data = new LruCache<int, int>(10);
            data[0] = 1;
            Assert.AreEqual(1, data.Count);
            Assert.AreEqual(1, data[0]);
            Assert.AreEqual(1, data.Keys.Count);
            Assert.AreEqual(1, data.Values.Count);
        }
    }
}
