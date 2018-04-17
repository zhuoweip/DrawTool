/* 
 * Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 * See LICENSE in the project root for license information.
 */
// Mono's implementation of using (...) {} boxes the used object, thus churning the box.
// Visual Studio avoids this. USE_CLASS_WRAPPER uses pooled wrapper objects to handle this problem in Unity
#define USE_CLASS_WRAPPER

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Multitouch.EventSystems.Utils
{
    /// <summary>
    /// Class for creating pools of arbitrary class objects
    /// Constructor allows one to specify an initialization Action
    /// when the class has to construct a new object (as opposed to reusing
    /// a previously released object) and a cleanup Action to be performed
    /// when an object is released back to the pool.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Pool<T> where T : class, new()
    {
        /// <summary>
        /// Constructs a pool with Action delegates for object Initialization and Cleanup
        /// </summary>
        /// <param name="onCreate">Action when an object is initially created</param>
        /// <param name="onRelease">Action when an Release is called for an object 
        /// (prior to being placed in the pool for re-use)</param>
        public Pool(Action<T> onCreate, Action<T> onRelease)
        {
            _onCreate = onCreate;
            _onRelease = onRelease;
        }

        /// <summary>
        /// Gets a previously Released() object, or constructs a new one 
        /// (and calls the onCreate delegate if a new one is created) 
        /// </summary>
        /// <returns></returns>
        public T Get()
        {
            return _freeObjects.Count > 0 ? _freeObjects.Pop() : CreateItem();
        }

        /// <summary>
        /// Releases the object back to the pool
        /// Calls onRelease prior to pooling.
        /// </summary>
        /// <param name="obj"></param>
        public void Release(T obj)
        {
#if DEBUG
            if (_freeObjects.Contains(obj))
            {
                Debug.LogErrorFormat("Releasing an already released object {0}", obj);
                return;
            }
#endif
            if (_onRelease != null)
            {
                _onRelease(obj);
            }
            _freeObjects.Push(obj);
        }

        private T CreateItem()
        {
            T obj = new T();
            if (_onCreate != null)
                _onCreate(obj);
            return obj;
        }
        private readonly Action<T> _onCreate;
        private readonly Action<T> _onRelease;
        private readonly Stack<T> _freeObjects = new Stack<T>();
    }

#if USE_CLASS_WRAPPER
    /// <summary>
    /// This is a wrapper for an object obtained from a pool suitable for use in a
    /// using(...) statement. At the end of the using statement, the object is released back to the pool
    /// This implementation is a class object (itself held on a private free stack) to prevent GC churn
    /// under Unity. Mono's implementation of using(...) boxes the object in order to call the Dispose() method.
    /// Visual Studio's version of using(...) emits the proper IL to call the method without boxing. Therefore
    /// the struct implementation is prefered in that case.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Pooled<T> : IDisposable where T : class, new()
    {
        public static Pooled<TU> GetPooled<TU>(Pool<TU> pool) where TU : class, new()
        {
            var wrapper = FreeWrappers.Count > 0 ? Pooled<TU>.FreeWrappers.Pop() : new Pooled<TU>();
            var obj = pool.Get();
            wrapper.Set(obj, pool);
            return wrapper;
        }

        public T Item { get; private set; }
        public Pool<T> Pool { get; private set; } 
        private void Set(T item, Pool<T> pool)
        {
            Item = item;
            Pool = pool;
        }

        public void Dispose()
        {
            if (Pool == null || Item == null)
                return;

            Pool.Release(Item);
            Item = null;
            Pool = null;
            FreeWrappers.Push(this);
        }

        private static readonly Stack<Pooled<T>> FreeWrappers = new Stack<Pooled<T>>();
    }
#else
    /// <summary>
    /// This is a wrapper for an object obtained from a pool suitable for use in a
    /// using(...) statement. At the end of the using statement, the object is released back to the pool
    /// This implementation is a struct object to prevent GC churn in Visual Studio.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct Pooled<T> : IDisposable where T : class, new()
    {
        public static Pooled<TU> GetPooled<TU>(Pool<TU> pool) where TU : class, new()
        {
            var wrapper = new Pooled<TU>
            {
                Pool = pool,
                Item = pool.Get()
            };
            return wrapper;
        }

        public T Item { get; private set; }
        public Pool<T> Pool { get; private set; }

        public void Dispose()
        {
            if (Pool == null || Item == null)
                return;

            Pool.Release(Item);
            Item = null;
            Pool = null;
        }
    }
#endif
}
