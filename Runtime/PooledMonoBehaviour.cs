using UnityEngine;
using UnityEngine.Events;

namespace serginian.Pooling
{
    /// <summary>
    /// Base class for all poolable MonoBehaviour components.
    /// Inherit from this to make your components work with ObjectPool.
    /// </summary>
    /// <example>
    /// <code>
    /// public class Bullet : PooledMonoBehaviour
    /// {
    ///     public override void OnSpawned()
    ///     {
    ///         // Initialize bullet when spawned from pool
    ///         GetComponent&lt;Rigidbody&gt;().velocity = transform.forward * speed;
    ///     }
    ///
    ///     public override void OnReleased()
    ///     {
    ///         // Clean up state when returning to pool
    ///         GetComponent&lt;Rigidbody&gt;().velocity = Vector3.zero;
    ///     }
    ///
    ///     void OnCollisionEnter()
    ///     {
    ///         Return(); // Return to pool instead of Destroy()
    ///     }
    /// }
    /// </code>
    /// </example>
    public abstract class PooledMonoBehaviour : MonoBehaviour
    {
        /// <summary>
        /// Internal callback set by ObjectPool. Do not modify manually.
        /// </summary>
        public UnityAction<PooledMonoBehaviour> OnReturnToPoolRequest;

        /// <summary>
        /// Called when object is retrieved from pool. Override to initialize component.
        /// Guaranteed to be called after GameObject.SetActive(true).
        /// </summary>
        public abstract void OnSpawned();

        /// <summary>
        /// Called when object is returned to pool. Override to reset component state.
        /// Guaranteed to be called before GameObject.SetActive(false).
        /// </summary>
        public abstract void OnReleased();

        /// <summary>
        /// Returns this object to its pool. Call instead of Destroy() for pooled objects.
        /// Triggers OnReleased() and deactivates the GameObject.
        /// </summary>
        public void Return()
        {
            OnReturnToPoolRequest?.Invoke(this);
        }

    } // end of class
}