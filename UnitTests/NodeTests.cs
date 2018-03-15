//  
// Copyright (c) Mark Davis. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
//  

using LruCacheNet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UnitTests
{
    [TestClass]
    public class NodeTests
    {
        [TestMethod, TestCategory("Node")]
        public void NullNodeKey()
        {
            bool thrown = false;
            try
            {
                var node = new Node<string, TestData>(null, new TestData());
            }
            catch (ArgumentException)
            {
                thrown = true;
            }
            Assert.IsTrue(thrown, "ArgumentException not thrown");
        }

        [TestMethod, TestCategory("Node")]
        public void NullNodeValue()
        {
            bool thrown = false;
            try
            {
                var node = new Node<string, TestData>("0", null);
            }
            catch (ArgumentException)
            {
                thrown = true;
            }
            Assert.IsTrue(thrown, "ArgumentException not thrown");
        }

        [TestMethod, TestCategory("Node")]
        [DataRow(true, false, "Key:0 Data:0 Previous:Set Next:Null", DisplayName = "True|False")]
        [DataRow(false, true, "Key:0 Data:0 Previous:Null Next:Set", DisplayName = "False|True")]
        [DataRow(false, false, "Key:0 Data:0 Previous:Null Next:Null", DisplayName = "False|False")]
        [DataRow(true, true, "Key:0 Data:0 Previous:Set Next:Set", DisplayName = "True|True")]
        public void NodeToString(bool setPrevious, bool setNext, string expected)
        {
            var node = new Node<string, string>("0", "0");
            if (setPrevious)
            {
                node.Previous = new Node<string, string>("1", "1");
            }
            if (setNext)
            {
                node.Next = new Node<string, string>("2", "2");
            }
            string str = node.ToString();
            Assert.AreEqual(expected, str);
        }
    }
}
