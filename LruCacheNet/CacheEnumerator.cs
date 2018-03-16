// Copyright (c) Mark Davis. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using System;
using System.Collections;
using System.Collections.Generic;

namespace LruCacheNet
{
    /// <summary>
    /// Enumator for iterating through a list of <see cref="Node{TKey, TValue}"/>
    /// </summary>
    /// <typeparam name="TKey">Key for the items in the nodes</typeparam>
    /// <typeparam name="TValue">Value for the items in the nodes</typeparam>
    public class CacheEnumerator<TKey, TValue> : IEnumerator<KeyValuePair<TKey, TValue>>
    {
        private Node<TKey, TValue> _head;
        private Node<TKey, TValue> _current;
        private LruCache<TKey, TValue> _cache;
        private bool _collectionChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheEnumerator{TKey, TValue}"/> class
        /// </summary>
        /// <param name="cache">Cache to which this enumerator belongs</param>
        /// <param name="head">First item in the list</param>
        public CacheEnumerator(LruCache<TKey, TValue> cache, Node<TKey, TValue> head)
        {
            if (head == null)
            {
                throw new ArgumentException("Head can't be null");
            }
            if (cache != null)
            {
                _cache = cache;
                cache.CollectionChanged += Cache_CollectionChanged;
            }

            _head = head;
            _current = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheEnumerator{TKey, TValue}"/> class
        /// </summary>
        /// <param name="head">First item in the list</param>
        public CacheEnumerator(Node<TKey, TValue> head) : this(null, head)
        {
        }

        /// <summary>
        /// Gets the current item in the enumerator
        /// </summary>
        public KeyValuePair<TKey, TValue> Current
        {
            get
            {
                if (_head == null)
                {
                    throw new ObjectDisposedException(nameof(CacheEnumerator<TKey, TValue>));
                }
                if (_collectionChanged)
                {
                    throw new InvalidOperationException("Collection has changed");
                }

                return new KeyValuePair<TKey, TValue>(_current.Key, _current.Value);
            }
        }

        /// <summary>
        /// Gets the current item in the enumerator
        /// </summary>
        object IEnumerator.Current => Current;

        /// <summary>
        /// Clears the items in the enumerator
        /// </summary>
        public void Dispose()
        {
            _head = null;
            _current = null;
            if (_cache != null)
            {
                _cache.CollectionChanged -= Cache_CollectionChanged;
            }
            _cache = null;
        }

        /// <summary>
        /// Tries to move to the next item in the enumerator
        /// </summary>
        /// <returns>True if successfully moved to the next item, false if there were no more items</returns>
        public bool MoveNext()
        {
            if (_head == null)
            {
                throw new ObjectDisposedException(nameof(CacheEnumerator<TKey, TValue>));
            }
            if (_collectionChanged)
            {
                throw new InvalidOperationException("Collection has changed");
            }

            if (_current == null)
            {
                _current = _head;
                return true;
            }

            if (_current.Next != null)
            {
                _current = _current.Next;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Moves the enumerator back to the head item in the list
        /// </summary>
        public void Reset()
        {
            if (_collectionChanged)
            {
                throw new InvalidOperationException("Collection has changed");
            }

            _current = null;
        }

        /// <summary>
        /// Fired when the collection containing this enumerator changes
        /// </summary>
        /// <param name="sender">Cache that changed</param>
        /// <param name="e">Event args</param>
        private void Cache_CollectionChanged(object sender, EventArgs e)
        {
            _collectionChanged = true;
        }
    }
}
