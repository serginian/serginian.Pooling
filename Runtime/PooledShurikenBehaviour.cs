using System.Collections;
using UnityEngine;

namespace serginian.Pooling
{
    /// <summary>
    /// Pooled particle system that automatically returns to pool when emission completes.
    /// Useful for VFX like explosions, impacts, muzzle flashes, etc.
    /// </summary>
    /// <example>
    /// <code>
    /// var vfxPool = new ObjectPool&lt;PooledShurikenBehaviour&gt;(explosionPrefab, 20, 50);
    /// var explosion = vfxPool.Get();
    /// explosion.transform.position = hitPoint;
    /// // Automatically returns to pool when particle system finishes
    /// </code>
    /// </example>
    [RequireComponent(typeof(ParticleSystem))]
    public class PooledShurikenBehaviour : PooledMonoBehaviour
    {
        private ParticleSystem _particleSystem;
        private static readonly WaitForSeconds CheckDelay = new(0.5f);

        /*********************** MONO BEHAVIOUR ***********************/
        
        private void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
        }

        
        /*********************** PUBLIC INTERFACE ***********************/

        /// <summary>
        /// Stops and clears the particle system when returned to pool.
        /// </summary>
        public override void OnReleased()
        {
            StopAllCoroutines();
            _particleSystem.Stop();
            _particleSystem.Clear();
        }

        /// <summary>
        /// Plays the particle system and starts monitoring for completion.
        /// Automatically returns to pool when particles finish emitting.
        /// </summary>
        public override void OnSpawned()
        {
            _particleSystem.Play();
            StartCoroutine(CheckIfAlive());
        }

        
        /*********************** INNER LOGIC ***********************/
        
        private IEnumerator CheckIfAlive()
        {
            while (true)
            {
                yield return CheckDelay;
                if (_particleSystem.IsAlive(true))
                    continue;

                Return();
                yield break;
            }
        }
        
    } // end of class
}