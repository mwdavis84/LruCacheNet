// Copyright (c) Mark Davis. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using System;
using System.Collections;
using System.Collections.Generic;

namespace LruCacheNet
{
    /// <summary>
    /// An LRU cache that caches data in memory 
    /// </summary>
    /// <typeparam name="TKey">Type of key to use</typeparam>
    /// <typeparam name="TValue">Type of data to store in the cache</typeparam>
    public class LruCache<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private const int DefaultCacheSize = 1000;
        private const int MinimumCacheSize = 2;

        private Dictionary<TKey, Node<TKey, TValue>> _data;
        private Node<TKey, TValue> _head;
        private Node<TKey, TValue> _tail;
        private int _cacheSize;
        private object _lock;
        private UpdateDataMethod _updateMethod;
        private CreateCopyMethod _createMethod;

        /// <summary>
        /// Initializes a new instance of the <see cref="LruCache{TKey,TValue}"/> class with the default size of 1000 items
        /// </summary>
        public LruCache() : this(DefaultCacheSize)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LruCache{TKey,TValue}"/> class with a specific cache size
        /// </summary>
        /// <param name="capacity">Maximum number of items to hold in the cache</param>
        /// <exception cref="ArgumentException">Thrown if the capacity is less than the minimum</exception>
        public LruCache(int capacity)
        {
            // Why would you have a cache with so few items?
            if (capacity < MinimumCacheSize)
            {
                throw new ArgumentException("Cache size must be at least 2", nameof(capacity));
            }

            _cacheSize = capacity;
            _data = new Dictionary<TKey, Node<TKey, TValue>>();
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
        public delegate bool UpdateDataMethod(TValue cachedData, TValue newData);

        /// <summary>
        /// A method to create a deep copy of an item to place in the cache
        /// </summary>
        /// <param name="data">Data to copy</param>
        /// <returns>New copy of data</returns>
        public delegate TValue CreateCopyMethod(TValue data);

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
        /// Gets the keys for the items stored in the cache
        /// </summary>
        public ICollection<TKey> Keys
        {
            get
            {
                lock (_lock)
                {
                    return _data.Keys;
                }
            }
        }

        /// <summary>
        /// Gets an unordered set of the values stored in the cache
        /// </summary>
        public ICollection<TValue> Values
        {
            get
            {
                lock (_lock)
                {
                    return ToList();
                }
            }
        }

        /// <summary>
        /// Gets a value indicating if the collection is read-only
        /// </summary>
        public bool IsReadOnly => false;

        public TValue this[TKey key]
        {
            get => Get(key);
            set => AddOrUpdate(key, value);
        }

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
        /// <exception cref="ArgumentException">Thrown if the key or data is null</exception>
        public void AddOrUpdate(TKey key, TValue data)
        {
            if (key == null)
            {
                throw new ArgumentException("Key cannot be null", nameof(key));
            }
            if (data == null)
            {
                throw new ArgumentException("Data cannot be null", nameof(data));
            }

            lock (_lock)
            {
                if (_data.TryGetValue(key, out Node<TKey, TValue> node))
                {
                    // Data is already in the cache
                    // Move node to the head, and link up the node's previous next/previous together
                    MoveNodeUp(node);

                    if (_updateMethod != null)
                    {
                        _updateMethod.Invoke(node.Value, data);
                    }
                    else
                    {
                        node.Value = data;
                    }
                }
                else
                {
                    AddItem(key, data);
                }
            }
        }

        /// <summary>
        /// Marks an item as used and moves it to the front of the list
        /// </summary>
        /// <param name="key">Key for the item to refresh</param>
        /// <returns>True if item was in the cache and refreshed; otherwise false</returns>
        public bool Refresh(TKey key)
        {
            lock (_lock)
            {
                if (_data.TryGetValue(key, out Node<TKey, TValue> node))
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
        /// Retrieves an item from the cache without updating its position
        /// </summary>
        /// <param name="key">Key for which to search the queue</param>
        /// <returns>Item for key if found, otherwise null</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the key is not found in the cache</exception>
        public TValue Peek(TKey key)
        {
            lock (_lock)
            {
                _data.TryGetValue(key, out Node<TKey, TValue> data);
                if (data != null)
                {
                    return data.Value;
                }
                else
                {
                    throw new KeyNotFoundException();
                }
            }
        }

        /// <summary>
        /// Attempts to retrieve an item from the cache without updating its position
        /// </summary>
        /// <param name="key">Key for which to search the queue</param>
        /// <param name="data">Output data if found in the cache, otherwise default(T)</param>
        /// <returns>Item for key if found, otherwise null</returns>
        public bool TryPeek(TKey key, out TValue data)
        {
            lock (_lock)
            {
                if (_data.TryGetValue(key, out Node<TKey, TValue> node))
                {
                    data = node.Value;
                    return true;
                }
                else
                {
                    data = default(TValue);
                    return false;
                }
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
        /// Gets an ordered list of the items stored in the cache
        /// </summary>
        /// <returns>List of items in the cache, from most recently used to lead</returns>
        public List<TValue> ToList()
        {
            lock (_lock)
            {
                var list = new List<TValue>(Count);
                var current = _head;
                while (current != null)
                {
                    list.Add(current.Value);
                    current = current.Next;
                }
                return list;
            }
        }

        #region IDictionary Methods

        /// <summary>
        /// Gets data from the cache
        /// If the key exists in ;the cache it will also be moved to the front of the cache
        /// </summary>
        /// <param name="key">Key to find in the cache</param>
        /// <returns>Cached data, null if not found</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the key is not found in the cache</exception>
        public TValue Get(TKey key)
        {
            lock (_lock)
            {
                if (_data.TryGetValue(key, out Node<TKey, TValue> node))
                {
                    MoveNodeUp(node);
                    return node.Value;
                }
                else
                {
                    throw new KeyNotFoundException();
                }
            }
        }

        /// <summary>
        /// Attempts to retrieve a value from the cache
        /// </summary>
        /// <param name="key">Key to retrieve</param>
        /// <param name="data">Data from the cache, default value if not found</param>
        /// <returns>True if key is found in the cache, otherwise false</returns>
        public bool TryGetValue(TKey key, out TValue data)
        {
            lock (_lock)
            {
                if (_data.TryGetValue(key, out Node<TKey, TValue> node))
                {
                    data = node.Value;
                    MoveNodeUp(node);
                    return true;
                }
                else
                {
                    data = default(TValue);
                    return false;
                }
            }
        }

        /// <summary>
        /// Removes an item from the cache
        /// </summary>
        /// <param name="key">Key for the item to remove</param>
        /// <returns>True if the item was found in the cache and removed, otherwise false if not found</returns>
        public bool Remove(TKey key)
        {
            lock (_lock)
            {
                if (_data.TryGetValue(key, out Node<TKey, TValue> node))
                {
                    RemoveNodeFromList(node);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Checks if an item is the cache. Does not update its position.
        /// </summary>
        /// <param name="key">Key for which to search in the cache.</param>
        /// <returns>True if item is found in the cache, otherwise false</returns>
        public bool ContainsKey(TKey key)
        {
            lock (_lock)
            {
                return _data.ContainsKey(key);
            }
        }

        /// <summary>
        /// Adds an item to the cache, throws if it already exists
        /// </summary>
        /// <param name="key">Key for the item to add</param>
        /// <param name="value">Item to add to the cache</param>
        /// <exception cref="ArgumentException">Thrown if the key already exists in the cache</exception>
        public void Add(TKey key, TValue value)
        {
            lock (_lock)
            {
                if (!_data.ContainsKey(key))
                {
                    AddItem(key, value);
                }
                else
                {
                    throw new ArgumentException("Key already exists in cache");
                }
            }
        }

        /// <summary>
        /// Adds an item to the list, fails if the item's key already exists
        /// </summary>
        /// <param name="item">Item to add to the cache</param>
        /// <exception cref="ArgumentException">Thrown if the item's key already exists in the cache</exception>
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            lock (_lock)
            {
                Add(item.Key, item.Value);
            }
        }

        /// <summary>
        /// Checks if a key/value pair already exists in the cache
        /// </summary>
        /// <param name="item">Item to check for in the cache</param>
        /// <returns>True if the item exists in the cache, otherwise false</returns>
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            lock (_lock)
            {
                if (_data.TryGetValue(item.Key, out Node<TKey, TValue> node))
                {
                    if (node.Value.Equals(item.Value))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Copies the values from the cache into an array at a specified index
        /// </summary>
        /// <param name="array">Array in which to copy the items</param>
        /// <param name="arrayIndex">Index at which to start copying the items</param>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            lock (_lock)
            {
                if (array.Length >= arrayIndex + Count)
                {
                    int index = 0;
                    var node = _head;
                    while (node != null)
                    {
                        array[arrayIndex + index] = new KeyValuePair<TKey, TValue>(node.Key, node.Value);
                        ++index;
                        node = node.Next;
                    }
                }
                else
                {
                    throw new IndexOutOfRangeException("Cache will not fit in array");
                }
            }
        }

        /// <summary>
        /// Removes a key/value pair from the cache
        /// </summary>
        /// <param name="item">Item to remove from the cache</param>
        /// <returns>True if item was removed, otherwise false</returns>
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            lock (_lock)
            {
                if (_data.TryGetValue(item.Key, out Node<TKey, TValue> node))
                {
                    if (node.Value.Equals(item.Value))
                    {
                        RemoveNodeFromList(node);
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Gets the enumator for the cache
        /// </summary>
        /// <returns>Enumerator for the cache, ordered from most recently used to least</returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            lock (_lock)
            {
                return new CacheEnumerator<TKey, TValue>(_head);
            }
        }

        /// <summary>
        /// Gets the enumator for the cache
        /// </summary>
        /// <returns>Enumerator for the cache, ordered from most recently used to least</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (_lock)
            {
                return GetEnumerator();
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Removes a node from the list
        /// </summary>
        /// <param name="node">Node to remove</param>
        private void RemoveNodeFromList(Node<TKey, TValue> node)
        {
            _data.Remove(node.Key);
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
        private void MoveNodeUp(Node<TKey, TValue> node)
        {
            if (node == _head)
            {
                return;
            }

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
            _head.Previous = node;
            node.Previous = null;
            _head = node;
        }

        private void AddItem(TKey key, TValue value)
        {
            lock (_lock)
            {
                // Data isn't in the cache yet, so create a new node and add it
                TValue dataToInsert = _createMethod == null ? value : _createMethod(value);
                var node = new Node<TKey, TValue>(key, dataToInsert);
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
                        Node<TKey, TValue> nodeToRemove = _tail;
                        RemoveNodeFromList(nodeToRemove);
                    }
                }
            }
        }

        #endregion
    }
}
