using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace serginian.Pooling
{
    /// <summary>
    /// Generic object pool for Unity GameObjects with PooledMonoBehaviour components.
    /// Provides automatic lifecycle management with Get/Return pattern.
    /// </summary>
    /// <typeparam name="T">Type derived from PooledMonoBehaviour to pool</typeparam>
    /// <example>
    /// <code>
    /// var bulletPool = new ObjectPool&lt;Projectile&gt;(bulletPrefab, 10, 100);
    /// var bullet = bulletPool.Get();
    /// // ... use bullet
    /// bullet.Return(); // Returns to pool automatically
    /// </code>
    /// </example>
    public class ObjectPool<T>: IDisposable where T : PooledMonoBehaviour
    {
        private readonly GameObject _objectPrototype;
        private UnityEngine.Pool.ObjectPool<T> _pool;


        /*********************** PUBLIC INTERFACE ***********************/

        /// <summary>
        /// Creates a new object pool for the specified GameObject prefab.
        /// </summary>
        /// <param name="objectToPool">Prefab with component T to instantiate for pooling</param>
        /// <param name="initialPoolSize">Number of objects pre-created on initialization (default: 10)</param>
        /// <param name="maxPoolSize">Maximum pool capacity before objects are destroyed (default: 1000)</param>
        public ObjectPool(GameObject objectToPool, int initialPoolSize = 10, int maxPoolSize = 1000)
        {
            _objectPrototype = objectToPool;
            _pool = new UnityEngine.Pool.ObjectPool<T>(OnCreatePooledObject, OnObjectTookFromPool,
                OnObjectReturnedToPool,
                OnObjectDestroyedFromPool, true, initialPoolSize, maxPoolSize);
        }

        /// <summary>
        /// Retrieves an object from the pool. Creates new instance if pool is empty.
        /// Object is automatically activated and OnSpawned() is called.
        /// </summary>
        /// <returns>Active pooled object ready for use</returns>
        public T Get()
        {
            return _pool.Get();
        }

        /// <summary>
        /// Disposes the pool and destroys all pooled objects.
        /// Call this when the pool is no longer needed to free resources.
        /// </summary>
        public void Dispose()
        {
            _pool?.Dispose();
        }

        
        /************************* INNER LOGIC *************************/
        
        private void Release(PooledMonoBehaviour obj)
        {
            T gObj = obj as T;
            _pool.Release(gObj);
        }

        private T OnCreatePooledObject()
        {
            GameObject bullet = Object.Instantiate(_objectPrototype);
            T component = bullet.GetComponent<T>();
            component.OnReturnToPoolRequest = Release;
            return component;
        }

        private void OnObjectReturnedToPool(T obj)
        {
            obj.OnReleased();
            obj.transform.SetParent(null);
            obj.gameObject.SetActive(false);
        }

        private void OnObjectTookFromPool(T obj)
        {
            obj.gameObject.SetActive(true);
            obj.OnSpawned();
        }

        private void OnObjectDestroyedFromPool(T obj)
        {
            if (obj != false && obj.gameObject != false)
                Object.Destroy(obj.gameObject);
        }
        
    } // end of class
}