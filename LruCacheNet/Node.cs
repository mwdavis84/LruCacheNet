using System;

namespace LruCacheNet
{
    /// <summary>
    /// Doubly LinkedList node for generic data
    /// </summary>
    public sealed class Node<T>
    {
        /// <summary>
        /// Creates a new Linked List Node
        /// </summary>
        /// <param name="key">Key for the node</param>
        /// <param name="data">Data for the node</param>
#if DEBUG
        public Node(string key, T data)
#else
        internal Node(string key, T data)
#endif
        {
            if (string.IsNullOrEmpty(key))
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
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the next node in the list
        /// </summary>
        public Node<T> Next { get; set; }

        /// <summary>
        /// Gets or sets the previous node in the list
        /// </summary>
        public Node<T> Previous { get; set; }

        public override string ToString()
        {
            return $"Key:{Key} Data:{Data.ToString()} Previous:{GetNodeSummary(Previous)} Next:{GetNodeSummary(Next)}";
        }

        private string GetNodeSummary(Node<T> node)
        {
            return node != null ? "Set" : "Null";
        }
    }
}
