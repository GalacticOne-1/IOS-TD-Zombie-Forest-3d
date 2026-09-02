using System;
using System.Collections.Generic;
using System.Linq;
using Galactic1.Code.GameDatabase.Registries;
using UnityEngine;

namespace Galactic1.PoolObject
{
    /// <summary>
    /// Central pool manager with two lifetime tiers:
    /// - Global: lives entire app session (UI FX, sounds, common VFX)
    /// - Scene:  destroyed on ClearScene() — call this on raid exit
    /// </summary>
    public class PoolManager : MonoBehaviour, IGameService
    {
        // ── State ─────────────────────────────────────
        private readonly Dictionary<RuntimeId, IPoolWrapper> _pools = new();
        private readonly Dictionary<RuntimeId, PoolLifetime> _lifetimes = new();
        private readonly Dictionary<RuntimeId, float> _lastUsed = new();

        [Header("Idle Cleanup")] [SerializeField]
        private float _unloadDelay = 30f;

        [SerializeField] private float _cleanupInterval = 10f;

        private float _cleanupTimer;

        // ── Registration ──────────────────────────────

        public void RegisterPool<T, TC>(
            IObjectPoolConfig config,
            T prefab,
            TC so,
            PoolLifetime lifetime = PoolLifetime.Scene,
            Transform parent = null)
            where T : Component, IPoolable
            where TC : ScriptableObject
        {
            if (!Validate(config, prefab)) return;
            if (IsRegistered(config.Id)) return;

            var pool = new ObjectPool<T, TC>(
                prefab,
                config.ObjectPoolParam.InitialSize,
                config.ObjectPoolParam.MaxSize,
                parent ?? transform,
                so,
                config.Id);

            Register(config.Id, new PoolWrapper<T, TC>(pool), lifetime);
        }

        public void AutoRegisterFromResources<T, TC>(
            IObjectPoolConfig config,
            TC so,
            string prefabPath,
            PoolLifetime lifetime = PoolLifetime.Scene,
            Transform parent = null)
            where T : Component, IPoolable
            where TC : ScriptableObject
        {
            if (IsRegistered(config.Id)) return;

            var prefab = Resources.Load<T>(prefabPath);
            if (prefab == null)
            {
                Debug.LogError(
                    $"[PoolManager] Prefab not found: '{config.PrefabPath}' " +
                    $"(id: '{config.Id}')");
                return;
            }

            RegisterPool(config, prefab, so, lifetime, parent);
        }

        // ── Get / Return ──────────────────────────────

        /// <summary>
        /// Взять объект из пула по конфигу.
        /// </summary>
        public T Get<T>(IObjectPoolConfig config)
            where T : Component, IPoolable
        {
            if (TryGetWrapper<T>(config.Id, out var wrapper))
                return wrapper.Get();

            Debug.LogWarning(
                $"[PoolManager] Pool not found: key='{config.Id.DebugKey}' type={typeof(T).Name}");
            return null;
        }

        /// <summary>
        /// Вернуть объект в пул. Использует PoolKey самого объекта —
        /// не нужно передавать строку вручную.
        /// </summary>
        public void Return<T>(T obj)
            where T : Component, IPoolable
        {
            if (obj == null) return;

            var key = obj.PoolKey;

            if (TryGetWrapper<T>(key, out var wrapper))
            {
                wrapper.Return(obj);
                return;
            }

            Debug.LogWarning($"[PoolManager] Return failed: key='{key}' — destroying.");
            Destroy(obj.gameObject);
        }
        
        // PoolManager.cs
        // Вместо Return<T> — принимаем IPoolable, ищем через рефлексию тип не нужен
        public void Return(IPoolable obj)
        {
            if (obj == null) return;

            var key = obj.PoolKey;

            if (key == null)
            {
                Debug.LogError("[PoolManager] Return: PoolKey is empty.");
                return;
            }

            if (!_pools.TryGetValue(key, out var wrapper))
            {
                Debug.LogError(
                    $"[PoolManager] Return: key='{key}' not found. " +
                    $"Registered: [{string.Join(", ", _pools.Keys)}]");

                if (obj is Component c) Destroy(c.gameObject);
                return;
            }

            wrapper.Return(obj);   // IPoolWrapper.Return(IPoolable)
        }

        // ── Lifecycle ─────────────────────────────────

        public void ClearScene()
        {
            var keys = _lifetimes
                .Where(kv => kv.Value == PoolLifetime.Scene)
                .Select(kv => kv.Key)
                .ToList();

            foreach (var key in keys) DestroyByKey(key);

            Debug.Log($"[PoolManager] Scene cleared ({keys.Count} pools).");
        }

        public void DestroyAll()
        {
            foreach (var pool in _pools.Values)
            {
                pool.DestroyAll();
                pool.Dispose();
            }

            _pools.Clear();
            _lifetimes.Clear();
            _lastUsed.Clear();
            Resources.UnloadUnusedAssets();
        }

        public void DestroyByKey(RuntimeId id)
        {
            if (!_pools.TryGetValue(id, out var pool))
            {
                Debug.LogWarning($"[PoolManager] DestroyByKey: '{id}' not found.");
                return;
            }

            pool.DestroyAll();
            pool.Dispose();
            _pools.Remove(id);
            _lifetimes.Remove(id);
            _lastUsed.Remove(id);
            Resources.UnloadUnusedAssets();
        }

        // ── Idle Cleanup ──────────────────────────────

        // private void Update()
        // {
        //     _cleanupTimer += Time.unscaledDeltaTime;
        //     if (_cleanupTimer < _cleanupInterval) return;
        //     _cleanupTimer = 0f;
        //     RunIdleCleanup();
        // }

        private void RunIdleCleanup()
        {
            var now = Time.unscaledTime;
            var stale = _lastUsed
                .Where(kv => now - kv.Value > _unloadDelay
                             && _lifetimes.GetValueOrDefault(kv.Key) == PoolLifetime.Scene)
                .Select(kv => kv.Key)
                .ToList();

            foreach (var key in stale)
            {
                Debug.Log($"[PoolManager] Idle cleanup: '{key}'");
                DestroyByKey(key);
            }
        }

        // ── Diagnostics ───────────────────────────────

        public void LogPoolStats()
        {
            foreach (var (key, pool) in _pools)
            {
                var lt = _lifetimes.GetValueOrDefault(key).ToString();
                Debug.Log($"[PoolManager] '{key}' free={pool.Count} lifetime={lt}");
            }
        }

        // ── Private helpers ───────────────────────────

        private void Register(RuntimeId id, IPoolWrapper wrapper, PoolLifetime lt)
        {
            _pools[id] = wrapper;
            _lifetimes[id] = lt;
            _lastUsed[id] = Time.unscaledTime;
            Debug.Log($"[PoolManager] Registered '{id}' ({lt})");
        }

        private bool IsRegistered(RuntimeId id)
        {
            if (!_pools.ContainsKey(id)) return false;
            Debug.LogWarning($"[PoolManager] Already registered: '{id}'");
            return true;
        }

        private bool Validate<T>(IObjectPoolConfig cfg, T prefab) where T : Component
        {
            if (cfg?.Id && prefab != null) return true;
            Debug.LogWarning("[PoolManager] Invalid configId or null prefab.");
            return false;
        }

        private bool TryGetWrapper<T>(RuntimeId id, out ITypedPoolWrapper<T> result)
            where T : Component, IPoolable
        {
            result = null;
            if (!_pools.TryGetValue(id, out var raw)) return false;

            result = raw as ITypedPoolWrapper<T>;
            if (result != null)
            {
                _lastUsed[id] = Time.unscaledTime;
                return true;
            }

            Debug.LogWarning(
                $"[PoolManager] Type mismatch key='{id}' " +
                $"expected={typeof(T).Name}");
            return false;
        }

        // ── Internal wrappers ─────────────────────────

        private interface IPoolWrapper : IDisposable
        {
            int Count { get; }
            void DestroyAll();
            void Return(IPoolable obj);
        }

        // Типизированный интерфейс — убирает необходимость в as-касте
        private interface ITypedPoolWrapper<T> : IPoolWrapper
            where T : Component, IPoolable
        {
            T Get();
            void Return(T obj);
        }

        private class PoolWrapper<T, TC> : ITypedPoolWrapper<T>
            where T : Component, IPoolable
            where TC : ScriptableObject
        {
            private readonly ObjectPool<T, TC> _pool;
            public PoolWrapper(ObjectPool<T, TC> pool) => _pool = pool;

            public int Count => _pool.Count;
            public T Get() => _pool.Get();
            public void Return(T o) => _pool.Return(o);
            public void Dispose() => _pool.Clear();
            public void DestroyAll() => _pool.DestroyAll();
            
            // IPoolWrapper.Return — каст внутри враппера где T известен
            public void Return(IPoolable obj)
            {
                if (obj is T typed)
                {
                    _pool.Return(typed);
                    return;
                }

                Debug.LogError(
                    $"[PoolWrapper] Return: type mismatch. " +
                    $"Expected={typeof(T).Name} Got={obj.GetType().Name}");
            }
        }
    }

    // ─────────────────────────────────────────────────────────
    // Lifetime enum
    // ─────────────────────────────────────────────────────────

    public enum PoolLifetime
    {
        /// <summary>Lives the entire app session. Never auto-destroyed.</summary>
        Global,

        /// <summary>Destroyed on ClearScene() / raid exit.</summary>
        Scene
    }
}