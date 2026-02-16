# Changelog

All notable changes to serginian.Pooling will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-02-16

### Initial Release

serginian.Pooling's first public release - a lightweight, minimalist object pooling library for Unity.

#### Added - Core Features

**ObjectPool**
- Generic `ObjectPool<T>` class wrapping `UnityEngine.Pool.ObjectPool`
- Constructor with configurable initial and maximum pool sizes
- `Get()` method for retrieving objects from pool (creates new if empty)
- `Dispose()` method for cleanup and resource management
- Automatic lifecycle management with `OnSpawned()` and `OnReleased()` callbacks
- Built-in protection against pool overflow (destroys excess objects)

**PooledMonoBehaviour**
- Abstract base class for all poolable components
- `OnSpawned()` lifecycle hook called when object is taken from pool
- `OnReleased()` lifecycle hook called when object is returned to pool
- `Return()` method for returning objects to pool (replaces `Destroy()`)
- Internal `OnReturnToPoolRequest` callback for pool integration

**PooledShurikenBehaviour**
- Specialized pooled component for Unity's Shuriken particle system
- Automatic return-to-pool when particle emission completes
- Built-in `CheckIfAlive()` coroutine monitoring (0.5s intervals)
- Automatic `ParticleSystem.Play()` on spawn
- Automatic `ParticleSystem.Stop()` and `ParticleSystem.Clear()` on release
- `[RequireComponent(typeof(ParticleSystem))]` attribute for safety

#### Documentation

- Complete README with Quick Start guide
- API reference for all three components
- Best practices section with pool sizing guidelines
- Three practical examples: projectiles, VFX, and enemies
- Troubleshooting section for common issues

---

[1.0.0]: https://github.com/serginian/serginian.Pooling/releases/tag/v1.0.0
