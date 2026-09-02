
using System.Collections.Generic;
using Galactic1.Code.GameDatabase.Registries;
using UnityEngine;

namespace Galactic1.PoolObject
{
    /// <summary>
    /// Generic пул с hard cap и авто-возвратом по Duration.
    /// TConfig передаётся объекту один раз при создании.
    /// </summary>
    public class ObjectPool<T, TConfig>
        where T : Component, IPoolable
        where TConfig : ScriptableObject
    {
        private readonly Queue<T> _free = new();
        private readonly List<T> _allInstances = new();

        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly RuntimeId _poolKey;
        private readonly TConfig _config;
        private readonly int _maxSize;

        public int Count => _free.Count;
        public int TotalInstances => _allInstances.Count;
        public float LastUsedTime { get; private set; }
        public float TimeSinceLastUse => Time.unscaledTime - LastUsedTime;

        public ObjectPool(
            T prefab,
            int initialSize,
            int maxSize = 100,
            Transform parent = null,
            TConfig config = null,
            RuntimeId key = null)
        {
            _prefab = prefab;
            _parent = parent;
            _poolKey = key;
            _config = config;
            _maxSize = maxSize > 0 ? maxSize : int.MaxValue;

            LastUsedTime = Time.unscaledTime;

            for (int i = 0; i < initialSize; i++)
                Return(CreateInstance());
        }

        // ── Get ───────────────────────────────────────
        public T Get()
        {
            LastUsedTime = Time.unscaledTime;

            T obj;

            if (_free.Count > 0)
            {
                obj = _free.Dequeue();
            }
            else
            {
                // overflow guard
                if (_allInstances.Count >= _maxSize)
                {
                    Debug.LogWarning(
                        $"[ObjectPool<{typeof(T).Name}>] " +
                        $"Pool '{_poolKey}' hit MaxSize={_maxSize}. " +
                        $"Returning null.");
                    return null;
                }

                obj = CreateInstance();
            }

            obj.ResetState();
            obj.OnSpawn();
            return obj;
        }

        // ── Return ────────────────────────────────────
        public void Return(T obj)
        {
            if (obj == null) return;

            obj.OnDespawn();
            _free.Enqueue(obj);
        }

        // ── Cleanup ───────────────────────────────────
        public void Clear() => _free.Clear();

        public void DestroyAll()
        {
            foreach (var obj in _allInstances)
            {
                if (obj == null) continue;
                if (obj.gameObject.activeSelf) obj.OnDespawn();
                Object.Destroy(obj.gameObject);
            }

            _free.Clear();
            _allInstances.Clear();
        }

        // ── Private ───────────────────────────────────
        private T CreateInstance()
        {
            var obj = Object.Instantiate(_prefab, _parent);
            obj.gameObject.SetActive(false);

            // типизированный SetConfig — без unconstrained generic
            if (obj is IPoolItemConfig<TConfig> configurable)
                configurable.SetConfig(_config);

            obj.OnCreate();
            obj.SetPoolKey(_poolKey);
            _allInstances.Add(obj);
            return obj;
        }
    }

}