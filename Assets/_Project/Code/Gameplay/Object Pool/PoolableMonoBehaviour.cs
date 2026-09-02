using System.Collections;
using Galactic1.Code.GameDatabase.Registries;
using UnityEngine;

namespace Galactic1.PoolObject
{
    /// <summary>
    /// Базовый класс для всех poolable объектов.
    /// Наследники реализуют только игровую логику —
    /// пул, lifecycle и авто-возврат здесь.
    /// </summary>
    [DisallowMultipleComponent]
    public abstract class PoolableMonoBehaviour : MonoBehaviour, IPoolable
    {
        // ── IPoolable ─────────────────────────────────
        public RuntimeId PoolKey { get; private set; }

        // ── Protected state ───────────────────────────
        protected Transform Root { get; private set; }
        protected bool RootChanged;
        
        protected Transform CachedTransform { get; private set; }
        protected bool IsSpawned { get; private set; }

        private float _duration, timerDuration;
        private Coroutine _autoReturnRoutine;

        // ── Unity ─────────────────────────────────────
        protected virtual void Awake()
        {
            Root = transform.parent;
            CachedTransform = transform;
            OnAwake();
        }

        /// <summary>
        /// Замена Awake для наследников — вместо override Awake.
        /// </summary>
        protected virtual void OnAwake() {}

        // ── IPoolable — вызывается из ObjectPool ──────

        /// <summary>
        /// Один раз при Instantiate. Кэш компонентов, подписки.
        /// </summary>
        public virtual void OnCreate() {}

        /// <summary>
        /// При взятии из пула. Конфиг уже применён через SetConfig.
        /// </summary>
        public virtual void OnSpawn()
        {
            IsSpawned = true;
            gameObject.SetActive(true);

            if (_duration > 0f)
                _autoReturnRoutine = StartCoroutine(AutoReturnRoutine());
        }

        /// <summary>
        /// При возврате в пул. Останавливает всё и деактивирует.
        /// </summary>
        public virtual void OnDespawn()
        {
            IsSpawned = false;

            // останавливаем корутину явно — не StopAllCoroutines,
            // чтобы не мешать корутинам наследника
            if (_autoReturnRoutine != null)
            {
                StopCoroutine(_autoReturnRoutine);
                _autoReturnRoutine = null;
            }

            if (RootChanged)
            {
                AttachTo(Root);
            }

            gameObject.SetActive(false);
        }

        /// <summary>
        /// Сброс transform и игровых значений перед OnSpawn.
        /// </summary>
        public virtual void ResetState()
        {
            RootChanged = false;
            CachedTransform.localScale = Vector3.one;
            CachedTransform.localRotation = Quaternion.identity;
        }

        public void SetPoolKey(RuntimeId key) => PoolKey = key;

        // ── Duration API ──────────────────────────────

        /// <summary>
        /// Установить длительность жизни объекта.
        /// Вызывай из SetConfig наследника.
        /// </summary>
        protected void SetDuration(float duration)
        {
            _duration = duration;
            timerDuration = duration;
        }

        // ── Return ────────────────────────────────────

        /// <summary>
        /// Вернуть себя в пул. Можно вызывать из наследника в любой момент.
        /// </summary>
        protected void ReturnToPool()
        {
            if (!IsSpawned) return; // защита от двойного возврата

            ServiceLocator.Current.Get<PoolManager>().Return((IPoolable)this);
        }

        // ── Auto-return coroutine ─────────────────────

        private IEnumerator AutoReturnRoutine()
        {
            timerDuration = _duration;

            while (timerDuration > 0f)
            {
                // пауза-aware: ждём пока не снимут паузу
                while (IsPaused())
                    yield return null;

                timerDuration -= Time.deltaTime;
                yield return null;
            }

            _autoReturnRoutine = null;
            ReturnToPool();
        }

        private static bool IsPaused()
        {
            var master = ServiceLocator.Current.Get<MonoBehaviourMaster>();
            return master != null && master.isPause;
        }

        // ── Helpers ───────────────────────────────────

        public virtual void SetPosition(Vector3 position)
            => CachedTransform.position = position;

        public virtual void AttachTo(Transform parent)
        {
            RootChanged = true;
            CachedTransform.SetParent(parent);
            CachedTransform.localPosition = Vector3.zero;
            CachedTransform.localRotation = Quaternion.identity;
        }
    }
}