# serginian.Pooling

**The Lightweight Solution for Unity Object Pooling**

serginian.Pooling is a minimalist, production-ready pooling library that wraps Unity's native ObjectPool system with a clean, developer-friendly API. Built with just three focused scripts, it eliminates boilerplate setup while maintaining full compatibility with Unity's pooling infrastructure. Create a pool in one line, use it anywhere.

## What You Get

- **🎯 One-Line Pool Creation** — `new ObjectPool<T>(prefab, 10, 100)` and you're done
- **🔄 Automatic Lifecycle Management** — `OnSpawned()` and `OnReleased()` hooks for initialization and cleanup
- **✨ Particle System Support** — Built-in `PooledShurikenBehaviour` with auto-return on completion
- **🛠️ Built on Unity's Pool System** — Leverages `UnityEngine.Pool.ObjectPool` for reliability and performance
- **📦 Zero Dependencies** — Pure Unity, no external packages required

## Who Is This For?

serginian.Pooling is built **for programmers**. You'll need solid knowledge of **C#** and **Unity** to integrate pooling into your project effectively. If you're comfortable with:
- Generic types and inheritance
- Component-based architecture
- Unity's GameObject lifecycle

...then you're ready to optimize your game with serginian.Pooling.

---

## Table of Contents

- [Requirements](#requirements)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Core Concepts](#core-concepts)
  - [ObjectPool](#objectpool)
  - [PooledMonoBehaviour](#pooledmonobehaviour)
  - [PooledShurikenBehaviour](#pooledshurikenbehaviour)
- [Best Practices](#best-practices)
- [Examples](#examples)

## Requirements

- **Unity** 2021.3+ (compatible with Unity 6000.0+)
- No external dependencies

## Installation

Add the package to your Unity project via the Unity Package Manager using a Git URL:

1. Open **Window → Package Manager**
2. Click **+** → **Add package from git URL…**
3. Enter your repository URL, e.g.:
   ```
   https://github.com/serginian/serginian.Pooling.git
   ```

    Or add it directly to your `Packages/manifest.json`:

    ```json
    {
        "dependencies":
        {
            "com.serginian.pooling": "https://github.com/serginian/serginian.Pooling.git"
        }
    }
    ```

## Quick Start

Get up and running with object pooling in 3 steps:

### 1. Create a Pooled Component

Inherit from `PooledMonoBehaviour` and implement lifecycle hooks:

```csharp
using serginian.Pooling;
using UnityEngine;

public class Bullet : PooledMonoBehaviour
{
    [SerializeField] private float speed = 20f;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnSpawned()
    {
        // Initialize when taken from pool
        rb.velocity = transform.forward * speed;
    }

    public override void OnReleased()
    {
        // Clean up when returning to pool
        rb.velocity = Vector3.zero;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Return to pool instead of Destroy()
        Return();
    }
}
```

### 2. Create a Pool

Instantiate an `ObjectPool<T>` with your prefab:

```csharp
using serginian.Pooling;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    private ObjectPool<Bullet> bulletPool;

    void Start()
    {
        // Create pool: prefab, initial size, max size
        bulletPool = new ObjectPool<Bullet>(bulletPrefab, 10, 100);
    }

    void OnDestroy()
    {
        // Clean up pool when done
        bulletPool?.Dispose();
    }
}
```

### 3. Use the Pool

Get objects from the pool and let them return automatically:

```csharp
void Fire()
{
    var bullet = bulletPool.Get();
    bullet.transform.position = firePoint.position;
    bullet.transform.rotation = firePoint.rotation;
    // Bullet will call Return() when it hits something
}
```

**That's it!** Your objects are now pooled and recycled automatically.

---

## Core Concepts

## ObjectPool

`ObjectPool<T>` is a generic pool manager that wraps Unity's `UnityEngine.Pool.ObjectPool` with automatic lifecycle management.

### Constructor

```csharp
public ObjectPool(GameObject objectToPool, int initialPoolSize = 10, int maxPoolSize = 1000)
```

| Parameter | Description |
|---|---|
| `objectToPool` | Prefab containing a component of type `T` (must inherit from `PooledMonoBehaviour`) |
| `initialPoolSize` | Number of objects pre-instantiated on creation (default: 10) |
| `maxPoolSize` | Maximum pool capacity; excess objects are destroyed (default: 1000) |

### Methods

| Method | Description |
|---|---|
| `Get()` | Retrieves an object from the pool (creates new if empty); calls `OnSpawned()` |
| `Dispose()` | Destroys all pooled objects and cleans up resources |

### Example

```csharp
// Create a pool for projectiles
var projectilePool = new ObjectPool<Projectile>(projectilePrefab, 20, 200);

// Get an object
var projectile = projectilePool.Get();
projectile.transform.position = spawnPoint;

// Later, object calls Return() to go back to pool

// Clean up when done
projectilePool.Dispose();
```

---

## PooledMonoBehaviour

`PooledMonoBehaviour` is the abstract base class for all poolable components. Inherit from this to make your components work with `ObjectPool<T>`.

### Lifecycle Hooks

| Method | Description |
|---|---|
| `OnSpawned()` | Called when object is retrieved from pool (after `GameObject.SetActive(true)`) |
| `OnReleased()` | Called when object is returned to pool (before `GameObject.SetActive(false)`) |

### Methods

| Method | Description |
|---|---|
| `Return()` | Returns this object to its pool; call instead of `Destroy()` |

### Properties

| Property | Description |
|---|---|
| `OnReturnToPoolRequest` | Internal callback set by `ObjectPool`; do not modify manually |

### Implementation Pattern

```csharp
public class Enemy : PooledMonoBehaviour
{
    private int health;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public override void OnSpawned()
    {
        // Reset state when spawned
        health = 100;
        animator.SetTrigger("Spawn");
    }

    public override void OnReleased()
    {
        // Clean up before deactivation
        animator.SetTrigger("Despawn");
        health = 0;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Return(); // Return to pool instead of destroying
        }
    }
}
```

---

## PooledShurikenBehaviour

`PooledShurikenBehaviour` is a specialized pooled component for **Unity's Shuriken particle system** (`ParticleSystem`). It automatically returns to the pool when particle emission completes.

### Features

- **Auto-Return**: Monitors particle system and returns to pool when `ParticleSystem.IsAlive()` is false
- **Automatic Playback**: Calls `ParticleSystem.Play()` on spawn
- **Cleanup**: Stops and clears particles on return

### Usage

Perfect for one-shot VFX like explosions, impacts, muzzle flashes, and environmental effects.

```csharp
// Create a pool for explosion VFX
var explosionPool = new ObjectPool<PooledShurikenBehaviour>(explosionPrefab, 10, 50);

// Spawn an explosion
var explosion = explosionPool.Get();
explosion.transform.position = hitPoint;
explosion.transform.rotation = Quaternion.identity;

// Particle system plays automatically and returns to pool when finished
```

### Requirements

- GameObject must have a `ParticleSystem` component (enforced by `[RequireComponent]`)
- Particle system should have finite duration or looping disabled for auto-return to work

### How It Works

1. **OnSpawned()**: Plays the particle system and starts monitoring
2. **CheckIfAlive()**: Coroutine checks every 0.5 seconds if particles are still alive
3. **OnReleased()**: Stops particle system, clears particles, stops coroutines

---

## Best Practices

### Pool Sizing

- **Initial Size**: Set to the typical number of objects active at once
- **Max Size**: Set to peak concurrent usage + buffer (e.g., 2x typical usage)
- Undersized pools create objects on-demand (small performance hit)
- Oversized pools waste memory with unused pre-instantiated objects

```csharp
// Good: 20 bullets typically active, peak is 50
var bulletPool = new ObjectPool<Bullet>(bulletPrefab, 20, 100);

// Bad: Wasteful pre-allocation
var bulletPool = new ObjectPool<Bullet>(bulletPrefab, 1000, 10000);
```

### Lifecycle Management

- **OnSpawned()**: Use for initialization, reset state, start coroutines
- **OnReleased()**: Use for cleanup, stop coroutines, reset transforms
- Always call `Return()` instead of `Destroy()` for pooled objects

```csharp
public override void OnSpawned()
{
    // Good: Reset state
    health = maxHealth;
    isActive = true;
}

public override void OnReleased()
{
    // Good: Clean up
    StopAllCoroutines();
    isActive = false;
}
```

### Pool Disposal

Always dispose pools when they're no longer needed to prevent memory leaks:

```csharp
void OnDestroy()
{
    bulletPool?.Dispose();
    enemyPool?.Dispose();
    vfxPool?.Dispose();
}
```

### Common Pitfalls

❌ **Don't call `Destroy()` on pooled objects** — use `Return()` instead

```csharp
// Bad
Destroy(gameObject);

// Good
Return();
```

❌ **Don't forget to dispose pools** — memory leaks occur if pools aren't cleaned up

```csharp
// Bad: Pool never disposed
void OnDisable()
{
    // Pool still holds references to GameObjects
}

// Good: Explicitly dispose
void OnDestroy()
{
    bulletPool?.Dispose();
}
```

❌ **Don't keep references to returned objects** — they may be reused elsewhere

```csharp
// Bad
var bullet = bulletPool.Get();
bullet.Return();
bullet.transform.position = Vector3.zero; // Object might be in use elsewhere!

// Good
var bullet = bulletPool.Get();
// Use bullet...
bullet.Return();
bullet = null; // Clear reference
```

---

## Examples

### Example 1: Projectile Pool

```csharp
using serginian.Pooling;
using UnityEngine;

public class Projectile : PooledMonoBehaviour
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifetime = 5f;
    private Rigidbody rb;
    private float spawnTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnSpawned()
    {
        rb.velocity = transform.forward * speed;
        spawnTime = Time.time;
    }

    public override void OnReleased()
    {
        rb.velocity = Vector3.zero;
    }

    private void Update()
    {
        // Auto-return after lifetime expires
        if (Time.time - spawnTime >= lifetime)
            Return();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Return to pool on impact
        Return();
    }
}

public class Gun : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    private ObjectPool<Projectile> projectilePool;

    void Start()
    {
        projectilePool = new ObjectPool<Projectile>(projectilePrefab, 30, 100);
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            Fire();
        }
    }

    void Fire()
    {
        var projectile = projectilePool.Get();
        projectile.transform.position = firePoint.position;
        projectile.transform.rotation = firePoint.rotation;
    }

    void OnDestroy()
    {
        projectilePool?.Dispose();
    }
}
```

### Example 2: VFX Pool

```csharp
using serginian.Pooling;
using UnityEngine;

public class VFXManager : MonoBehaviour
{
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private GameObject impactPrefab;

    private ObjectPool<PooledShurikenBehaviour> explosionPool;
    private ObjectPool<PooledShurikenBehaviour> impactPool;

    void Start()
    {
        explosionPool = new ObjectPool<PooledShurikenBehaviour>(explosionPrefab, 10, 50);
        impactPool = new ObjectPool<PooledShurikenBehaviour>(impactPrefab, 20, 100);
    }

    public void PlayExplosion(Vector3 position)
    {
        var explosion = explosionPool.Get();
        explosion.transform.position = position;
        // Auto-returns when particles finish
    }

    public void PlayImpact(Vector3 position, Vector3 normal)
    {
        var impact = impactPool.Get();
        impact.transform.position = position;
        impact.transform.rotation = Quaternion.LookRotation(normal);
        // Auto-returns when particles finish
    }

    void OnDestroy()
    {
        explosionPool?.Dispose();
        impactPool?.Dispose();
    }
}
```

### Example 3: Enemy Pool

```csharp
using serginian.Pooling;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : PooledMonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;
    private NavMeshAgent agent;
    private Animator animator;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    public override void OnSpawned()
    {
        currentHealth = maxHealth;
        agent.enabled = true;
        animator.SetTrigger("Spawn");
    }

    public override void OnReleased()
    {
        agent.enabled = false;
        StopAllCoroutines();
        animator.SetTrigger("Death");
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Return(); // Return to pool
        }
    }
}

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float spawnInterval = 2f;

    private ObjectPool<Enemy> enemyPool;

    void Start()
    {
        enemyPool = new ObjectPool<Enemy>(enemyPrefab, 10, 50);
        InvokeRepeating(nameof(SpawnEnemy), 1f, spawnInterval);
    }

    void SpawnEnemy()
    {
        var spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        var enemy = enemyPool.Get();
        enemy.transform.position = spawnPoint.position;
        enemy.transform.rotation = spawnPoint.rotation;
    }

    void OnDestroy()
    {
        CancelInvoke();
        enemyPool?.Dispose();
    }
}
```

---

## Troubleshooting

### Common Issues

**Pool.Get() creates new objects every time**
- Ensure you're calling `Return()` on objects instead of `Destroy()`
- Check that `OnReturnToPoolRequest` is not being manually modified
- Verify objects inherit from `PooledMonoBehaviour`

**Particle system doesn't auto-return**
- Ensure particle system has finite duration (not looping)
- Check that `stopAction` is set to `Disable` or `Destroy` in particle system settings
- Verify `PooledShurikenBehaviour` component is attached

**Objects remain active after Return()**
- This is handled automatically; if objects stay active, check for external references keeping them alive
- Ensure you're not calling `SetActive(true)` after `Return()`

**Memory leaks**
- Always call `Dispose()` on pools in `OnDestroy()` or when done
- Check for lingering references to pooled objects in other scripts
