//  
// Copyright (c) Mark Davis. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
//  

using System;
using System.Collections.Generic;

namespace LruCacheNet
{
    /// <summary>
    /// An LRU cache that caches data in memory 
    /// </summary>
    /// <typeparam name="T">Type of data to store in the cache. Must be an object</typeparam>
    public class LruCache<T> where T : class
    {
        private const int DefaultCacheSize = 1000;
        private const int MinimumCacheSize = 3;

        private Dictionary<string, Node<T>> _data;
        private Node<T> _head;
        private Node<T> _tail;
        private int _cacheSize;        
        private object _lock;
        private UpdateDataMethod _updateMethod;
        private CreateCopyMethod _createMethod;

        /// <summary>
        /// Initializes a new instance of the <see cref="LruCache{T}"/> class with the default size of 1000 items
        /// </summary>
        public LruCache() : this(DefaultCacheSize)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LruCache{T}"/> class with a specific cache size
        /// </summary>
        /// <param name="capacity">Maximum number of items to hold in the cache</param>
        public LruCache(int capacity)
        {
            // Why would you have a cache with so few items?
            if (capacity < MinimumCacheSize)
            {
                throw new ArgumentException("Cache size must be at least 3", nameof(capacity));
            }

            _cacheSize = capacity;
            _data = new Dictionary<string, Node<T>>();
            _head = null;
            _tail = null;
            _lock = new object();
        }

        /// <summary>
        /// A method to update a data item in the cache
        /// </summary>
        /// <param name="cachedData">Data currently in the cache</param>
        /// <param name="newData">New data to use to update the data</param>
        /// <returns>True if the value was updated, false if it was unchanged</returns>
        public delegate bool UpdateDataMethod(T cachedData, T newData);

        /// <summary>
        /// A method to create a deep copy of an item to place in the cache
        /// </summary>
        /// <param name="data">Data to copy</param>
        /// <returns>New copy of data</returns>
        public delegate T CreateCopyMethod(T data);

        /// <summary>
        /// Gets the number of items currently int the cache
        /// </summary>
        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _data.Count;
                }
            }
        }

        /// <summary>
        /// Gets the size of the cache
        /// </summary>
        public int Capacity => _cacheSize;

        /// <summary>
        /// Sets the method to call when data already in the cache is updated
        /// </summary>
        /// <param name="method">Update method implementation</param>
        public void SetUpdateMethod(UpdateDataMethod method)
        {
            _updateMethod = method;
        }

        /// <summary>
        /// Sets the method to call when placing a new item in the cache
        /// This method will create a copy of the item rather than adding the item passed in
        /// </summary>
        /// <param name="method">Copy method to call</param>
        public void SetCopyMethod(CreateCopyMethod method)
        {
            _createMethod = method;
        }

        /// <summary>
        /// Adds an item to the cache or updates its values if already in the cache
        /// If the item already existed in the cache it will also be moved to the front
        /// </summary>
        /// <param name="key">Key to store in the cache</param>
        /// <param name="data">Data to cache</param>
        public void AddOrUpdate(string key, T data)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key cannot be null", nameof(key));
            }
            if (data == null)
            {
                throw new ArgumentException("Data cannot be null", nameof(data));
            }

            lock (_lock)
            {
                if (_data.TryGetValue(key, out Node<T> node))
                {
                    // Data is already in the cache
                    // Move node to the head, and link up the node's previous next/previous together
                    MoveNodeUp(node);
                    _updateMethod?.Invoke(node.Data, data);
                }
                else
                {
                    // Data isn't in the cache yet, so create a new node and add it
                    T dataToInsert = _createMethod == null ? data : _createMethod(data);
                    node = new Node<T>(key, dataToInsert);
                    _data[key] = node;

                    if (_head == null)
                    {
                        // Empty cache - set this to head and tail
                        _head = node;
                        _tail = node;
                    }
                    else
                    {
                        // First put this new node at the front of the list
                        _head.Previous = node;
                        node.Next = _head;
                        _head = node;

                        if (Count > _cacheSize)
                        {
                            // List is over capacity so remove the tail
                            Node<T> nodeToRemove = _tail;
                            RemoveNodeFromList(nodeToRemove.Key, nodeToRemove);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets data from the cache
        /// If the key exists in the cache it will also be moved to the front of the cache
        /// </summary>
        /// <param name="key">Key to find in the cache</param>
        /// <returns>Cached data, null if not found</returns>
        public T Get(string key)
        {
            lock (_lock)
            {
                if (_data.TryGetValue(key, out Node<T> node))
                {
                    MoveNodeUp(node);
                    return node.Data;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Marks an item as used and moves it to the front of the list
        /// </summary>
        /// <param name="key">Key for the item to refresh</param>
        /// <returns>True if item was in the cache and refreshed; otherwise false</returns>
        public bool Refresh(string key)
        {
            lock (_lock)
            {
                if (_data.TryGetValue(key, out Node<T> node))
                {
                    MoveNodeUp(node);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Removes an item from the cache
        /// </summary>
        /// <param name="key">Key for the item to remove</param>
        /// <returns>Data that was stored in the cache, null if none</returns>
        public T Remove(string key)
        {
            lock (_lock)
            {
                if (_data.TryGetValue(key, out Node<T> node))
                {
                    RemoveNodeFromList(key, node);
                    return node.Data;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Checks if an item is the cache. Does not update its position.
        /// </summary>
        /// <param name="key">Key for which to search in the cache.</param>
        /// <returns>True if item is found in the cache, otherwise false</returns>
        public bool ContainsKey(string key)
        {
            lock (_lock)
            {
                return _data.ContainsKey(key);
            }
        }

        /// <summary>
        /// Retrieves an item from the cache without updating its position
        /// </summary>
        /// <param name="key">Key for which to search the queue</param>
        /// <returns>Item for key if found, otherwise null</returns>
        public T Peek(string key)
        {
            lock (_lock)
            {
                _data.TryGetValue(key, out Node<T> data);
                return data?.Data;
            }
        }

        /// <summary>
        /// Clears all items from the cache
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _data.Clear();
                _head = null;
                _tail = null;
            }
        }

        /// <summary>
        /// Removes a node from the list
        /// </summary>
        /// <param name="key">Key for the node</param>
        /// <param name="node">Node to remove</param>
        private void RemoveNodeFromList(string key, Node<T> node)
        {
            _data.Remove(key);
            if (node.Previous != null)
            {
                node.Previous.Next = node.Next;
            }
            if (node.Next != null)
            {
                node.Next.Previous = node.Previous;
            }
            if (node == _head)
            {
                _head = node.Next;
            }
            if (node == _tail)
            {
                _tail = node.Previous;
            }
            node.Previous = null;
            node.Next = null;
        }

        /// <summary>
        /// Moves a node to the front of the cache
        /// </summary>
        /// <param name="node">Node to move to the front</param>
        private void MoveNodeUp(Node<T> node)
        {
            if (node.Previous != null)
            {
                if (node == _tail)
                {
                    _tail = node.Previous;
                }
                node.Previous.Next = node.Next;
            }
            if (node.Next != null)
            {
                node.Next.Previous = node.Previous;
            }
            node.Next = _head;
            node.Previous = null;
            _head = node;
        }
    }
}
