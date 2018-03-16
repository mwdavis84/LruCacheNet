// Copyright (c) Mark Davis. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using System;

namespace LruCacheNet
{
    /// <summary>
    /// Node for storing data in the doubly linked list
    /// </summary>
    /// <typeparam name="K">Type of key to use to identify the data</typeparam>
    /// <typeparam name="T">Type of data to store in the cache</typeparam>
    public sealed class Node<K, T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Node{K,T}"/> class
        /// </summary>
        /// <param name="key">Key for the node</param>
        /// <param name="data">Data for the node</param>
#if DEBUG
        public Node(K key, T data)
#else
        internal Node(K key, T data)
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

            Data = data;
            Key = key;
            Next = null;
            Previous = null;
        }

        /// <summary>
        /// Gets or sets the data stored in the node
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// Gets or sets the key for the data in the cache
        /// </summary>
        public K Key { get; set; }

        /// <summary>
        /// Gets or sets the next node in the list
        /// </summary>
        public Node<K, T> Next { get; set; }

        /// <summary>
        /// Gets or sets the previous node in the list
        /// </summary>
        public Node<K, T> Previous { get; set; }

        public override string ToString()
        {
            return $"Key:{Key} Data:{Data.ToString()} Previous:{GetNodeSummary(Previous)} Next:{GetNodeSummary(Next)}";
        }

        private string GetNodeSummary(Node<K, T> node)
        {
            return node != null ? "Set" : "Null";
        }
    }
}
