// Copyright (c) Mark Davis. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using System;

namespace LruCacheNet
{
    /// <summary>
    /// Node for storing data in the doubly linked list
    /// </summary>
    /// <typeparam name="TKey">Type of key to use to identify the data</typeparam>
    /// <typeparam name="TValue">Type of data to store in the cache</typeparam>
    public sealed class Node<TKey, TValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Node{TKey,TValue}"/> class
        /// </summary>
        /// <param name="key">Key for the node</param>
        /// <param name="data">Data for the node</param>
#if DEBUG
        public Node(TKey key, TValue data)
#else
        internal Node(TKey key, TValue data)
#endif
        {
            if (key == null)
            {
                throw new ArgumentException("Key cannot be null", nameof(key));
            }
            if (data == null)
            {
                throw new ArgumentException("Data cannot be null", nameof(data));
            }

            Value = data;
            Key = key;
            Next = null;
            Previous = null;
        }

        /// <summary>
        /// Gets or sets the data stored in the node
        /// </summary>
        public TValue Value { get; set; }

        /// <summary>
        /// Gets or sets the key for the data in the cache
        /// </summary>
        public TKey Key { get; set; }

        /// <summary>
        /// Gets or sets the next node in the list
        /// </summary>
        public Node<TKey, TValue> Next { get; set; }

        /// <summary>
        /// Gets or sets the previous node in the list
        /// </summary>
        public Node<TKey, TValue> Previous { get; set; }

        public override string ToString()
        {
            return $"Key:{Key} Data:{Value.ToString()} Previous:{GetNodeSummary(Previous)} Next:{GetNodeSummary(Next)}";
        }

        private string GetNodeSummary(Node<TKey, TValue> node)
        {
            return node != null ? "Set" : "Null";
        }
    }
}
