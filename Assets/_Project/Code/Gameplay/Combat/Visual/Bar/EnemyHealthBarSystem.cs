using System.Collections.Generic;
using Galactic1.Code.Gameplay.Combat.Events;
using Galactic1.Code.Systems.Lifecycle;
using Galactic1.Code.Systems.Runtime.Enemy;
using Galactic1.Code.UI.HUD.Enemy;
using Galactic1.PoolObject;
using UnityEngine;

namespace Galactic1.Code.Systems.UI
{
    /// <summary>
    /// Demand-driven система полосок здоровья врагов.
    /// Архитектура: Aliens: Dark Descent / XCOM / Helldivers combat-feedback HUD.
    ///
    /// Ключевые правила:
    ///   — Виджет создаётся ТОЛЬКО после первого GameplayHitEvent по врагу.
    ///   — HP синхронизируется через HealthChangedEvent (урон, DoT, реген, хил).
    ///   — GameplayHitEvent используется ТОЛЬКО для Show() + Reset timer.
    ///   — Виджет возвращается в пул при: таймаут / смерть / деспавн.
    ///   — Фрустум-отсечение: Hide() без возврата в пул.
    ///   — После смерти IsDead = true блокирует создание нового виджета.
    ///
    /// Зависимости: PoolManager, Canvas, IObjectPoolConfig, EventBus.
    /// Не знает об EnemySceneLifecycleSystem, EnemyRepository, RaidRuntime.
    ///
    /// Масштабирование:
    ///   300 врагов → 10–20 активных виджетов (только у получивших урон).
    /// </summary>
    public sealed class EnemyHealthBarSystem : IUpdate, IGameService
    {
        // ── Константы ─────────────────────────────────────────────────

        /// <summary>
        /// Время в секундах до автоматического скрытия полоски
        /// после последнего попадания.
        /// </summary>
        private const float VisibleDuration = 5f;

        // ── Зависимости ───────────────────────────────────────────────

        private readonly PoolManager _pool;
        private readonly Canvas _hudCanvas;
        private readonly IObjectPoolConfig _barConfig;

        // ── Кэш камеры ────────────────────────────────────────────────

        // Camera.main — поиск по тегу, кэшируем один раз.
        // При смене камеры вызывать RefreshCamera() явно.
        private Camera _camera;

        // Аллоцируем один раз — ноль аллокаций в LateUpdate.
        private readonly Plane[] _frustumPlanes = new Plane[6];

        // ── Единый словарь состояния ──────────────────────────────────

        // Ключ: unitId
        // Содержит запись на каждого известного врага.
        // Widget == null означает: враг зарегистрирован, но урон ещё не получал.
        private readonly Dictionary<string, EnemyUiEntry> _entries = new();

        /// <summary>
        /// Единая запись состояния одного врага.
        ///
        /// Root         — Transform визуального корня, всегда заполнен.
        /// Widget       — null до первого попадания, затем виджет из пула.
        /// HideTime     — Time.time когда виджет уйдёт в пул.
        /// WasInFrustum — кэш предыдущего состояния фрустума (избегаем лишних Show/Hide).
        /// IsDead       — блокирует создание виджета после смерти.
        /// </summary>
        private sealed class EnemyUiEntry
        {
            public Transform Root;
            public UnitIndicatorWidget Widget;
            public float HideTime;
            public bool WasInFrustum;
            public bool IsDead;
            
            // Кэш последнего известного HP.
            // Заполняется в OnHealthChanged даже если виджета ещё нет.
            // -1 означает: HealthChangedEvent ещё не приходил.
            public float LastKnownHp  = -1f;
            public float LastKnownMax = -1f;
        }

        // ── EventBinding — именованные поля для корректной отписки ────

        private readonly EventBinding<EnemySceneCreatedEvent> _spawnBinding;
        private readonly EventBinding<EnemySceneDestroyedEvent> _destroyBinding;
        private readonly EventBinding<CombatHitEvent> _hitBinding;
        private readonly EventBinding<CombatDeathEvent> _deathBinding;
        private readonly EventBinding<HealthChangedEvent> _healthChangedBinding;

        // Переиспользуемый список для сбора ID на удаление — ноль аллокаций в LateUpdate.
        private readonly List<string> _toRemove = new();

        // ── Конструктор ───────────────────────────────────────────────

        public EnemyHealthBarSystem(
            PoolManager pool,
            Canvas hudCanvas,
            IObjectPoolConfig barConfig)
        {
            _pool = pool;
            _hudCanvas = hudCanvas;
            _barConfig = barConfig;

            _spawnBinding = new EventBinding<EnemySceneCreatedEvent>(OnEnemyCreated);
            _destroyBinding = new EventBinding<EnemySceneDestroyedEvent>(OnEnemyDestroyed);
            _hitBinding = new EventBinding<CombatHitEvent>(OnHit);
            _deathBinding = new EventBinding<CombatDeathEvent>(OnDeath);
            _healthChangedBinding = new EventBinding<HealthChangedEvent>(OnHealthChanged);

            EventBus<EnemySceneCreatedEvent>.Register(_spawnBinding);
            EventBus<EnemySceneDestroyedEvent>.Register(_destroyBinding);
            EventBus<CombatHitEvent>.Register(_hitBinding);
            EventBus<CombatDeathEvent>.Register(_deathBinding);
            EventBus<HealthChangedEvent>.Register(_healthChangedBinding);

            RefreshCamera();
            
            // * регистрируем для обновления
            ServiceLocator.Current.Get<MonoBehaviourMaster>().update.Add(this);
        }

        // ── EventBus handlers ─────────────────────────────────────────

        /// <summary>
        /// Враг заспавнен. Регистрируем Transform — виджет НЕ создаём.
        /// </summary>
        private void OnEnemyCreated(EnemySceneCreatedEvent e)
        {
            if (e.UIAnchor == null)
            {
                Debug.LogError($"Enemy '{e.UnitId}' has null UIAnchor");
                return;
            }
            
            _entries[e.UnitId] = new EnemyUiEntry
            {
                Root = e.UIAnchor,
                WasInFrustum = false,
                LastKnownHp = -1f,
                LastKnownMax = -1f
            };
        }

        /// <summary>
        /// Враг деспавнен. Возвращаем виджет в пул и удаляем запись.
        /// </summary>
        private void OnEnemyDestroyed(EnemySceneDestroyedEvent e)
        {
            ReturnToPool(e.UnitId);
            _entries.Remove(e.UnitId);
        }

        /// <summary>
        /// Попадание: только активация Show() и сброс таймера.
        /// HP НЕ обновляем — это задача OnHealthChanged.
        /// </summary>
        private void OnHit(CombatHitEvent e)
        {
            if (e.Target is not ISceneEnemy sceneEnemy) return;

            var unitId = sceneEnemy.RuntimeBase.Id;

            if (!_entries.TryGetValue(unitId, out var entry)) return;

            // Мёртв — игнорируем запоздавшие попадания (очередь событий)
            if (entry.IsDead) return;

            // Ленивое создание виджета при первом попадании
            if (entry.Widget == null)
            {
                var widget = _pool.Get<UnitIndicatorWidget>(_barConfig);

                if (widget == null)
                {
                    DLog.Alert($"[HealthBarSystem] Pool вернул null для '{unitId}'. " +
                               "Проверь регистрацию пула HealthBarWidget.", EDlogColor.RED);
                    return;
                }
                
                if (entry.Root == null)
                {
                    DLog.Alert($"Missing UIAnchor for '{unitId}'", EDlogColor.RED);
                    return;
                }

                widget.Bind(entry.Root);
                entry.Widget = widget;
                
                // Применяем закэшированный HP — HealthChangedEvent уже пришёл
                // раньше чем был создан виджет
                if (entry.LastKnownMax > 0f)
                    entry.Widget.SetHealth(entry.LastKnownHp, entry.LastKnownMax);
            }

            if (!entry.Widget.IsVisible)
                entry.Widget.Show();
            entry.HideTime = Time.time + VisibleDuration;
        }

        /// <summary>
        /// HP изменился (урон, DoT, реген, хил, бафф, дебафф).
        /// Единственное место обновления fillAmount в виджете.
        /// </summary>
        private void OnHealthChanged(HealthChangedEvent e)
        {
            if (!_entries.TryGetValue(e.UnitId, out var entry)
                || entry.IsDead)
                return;

            // Кэшируем всегда — независимо от наличия виджета
            entry.LastKnownHp  = e.CurrentHealth;
            entry.LastKnownMax = e.MaxHealth;

            // Виджета ещё нет — данные сохранены, OnHit подхватит их при создании
            if (entry.Widget == null) return;

            entry.Widget.SetHealth(e.CurrentHealth, e.MaxHealth);
        }

        /// <summary>
        /// Смерть врага.
        /// Ставим IsDead = true — блокируем создание виджета от запоздавших событий.
        /// Возвращаем виджет в пул немедленно.
        /// Запись остаётся до OnEnemyDestroyed (деспавн сцен-объекта).
        /// </summary>
        private void OnDeath(CombatDeathEvent e)
        {
            if (e.Victim is not ISceneEnemy sceneEnemy) return;

            var unitId = sceneEnemy.RuntimeBase.Id;

            if (!_entries.TryGetValue(unitId, out var entry)) return;

            entry.IsDead = true;
            ReturnToPool(unitId);
        }

        // ── LateUpdate ────────────────────────────────────────────────

        public void IUpdateClear()
        {
            ServiceLocator.Current.Get<MonoBehaviourMaster>().update.Remove(this);
        }

        /// <summary>
        /// Вызывается из EnemyHealthBarSystemTicker каждый LateUpdate.
        ///
        /// Порядок операций:
        ///   1. Пересчёт фрустума (один раз за кадр, без аллокаций)
        ///   2. Таймер жизни виджета
        ///   3. Фрустум-отсечение (Hide без возврата в пул)
        ///   4. Проекция WorldPos → Screen
        ///   5. Позиционирование на Canvas
        /// </summary>
        public void UpdateM()
        {
            if (_camera == null)
            {
                RefreshCamera();
                return;
            }

            if (_entries.Count == 0) return;

            float now = Time.time;

            // Пересчёт фрустума один раз на все виджеты за кадр
            GeometryUtility.CalculateFrustumPlanes(_camera, _frustumPlanes);

            _toRemove.Clear();

            foreach (var pair in _entries)
            {
                var entry = pair.Value;

                // Нет виджета — враг жив, но урон не получал
                if (entry.Widget == null) continue;

                // ── Таймер жизни ──────────────────────────────────────
                if (now >= entry.HideTime)
                {
                    _toRemove.Add(pair.Key);
                    continue;
                }

                // ── Фрустум-отсечение ─────────────────────────────────
                
                if (entry.Root == null)
                {
                    _toRemove.Add(pair.Key);
                    continue;
                }

                Vector3 worldPos = entry.Root.position;

                bool inFrustum = IsInFrustum(worldPos);

                // Уведомляем виджет только при изменении состояния
                if (inFrustum != entry.WasInFrustum)
                {
                    if (inFrustum) entry.Widget.Show();
                    else entry.Widget.Hide();

                    entry.WasInFrustum = inFrustum;
                }

                if (!inFrustum) continue;

                // ── Позиционирование ──────────────────────────────────
                Vector3 screenPos = _camera.WorldToScreenPoint(worldPos);

                // z < 0 — объект позади near plane камеры
                if (screenPos.z < 0f)
                {
                    if (entry.WasInFrustum)
                    {
                        entry.Widget.Hide();
                        entry.WasInFrustum = false;
                    }

                    continue;
                }

                entry.Widget.UpdateScreenPosition(new Vector2(screenPos.x, screenPos.y), _hudCanvas);
            }

            // Возвращаем просроченные виджеты в пул после итерации
            foreach (var unitId in _toRemove)
                ReturnToPool(unitId);
        }

        // ── Вспомогательное ───────────────────────────────────────────

        /// <summary>
        /// Вернуть виджет в пул. Обнуляет entry.Widget.
        /// Запись в _entries при этом НЕ удаляется.
        /// </summary>
        private void ReturnToPool(string unitId)
        {
            if (!_entries.TryGetValue(unitId, out var entry))
                return;

            if (entry.Widget == null)
                return;

            _pool.Return(entry.Widget);
            entry.Widget = null;
            entry.WasInFrustum = false;
        }

        private bool IsInFrustum(Vector3 worldPos)
        {
            // Bounds с небольшим размером вместо точки:
            // виджет не мигает когда враг проходит ровно по краю фрустума.
            var bounds = new Bounds(worldPos, Vector3.one * 0.5f);
            return GeometryUtility.TestPlanesAABB(_frustumPlanes, bounds);
        }

        /// <summary>
        /// Явное обновление ссылки на Camera.main.
        /// Вызывать при смене активной камеры (кат-сцены, режимы).
        /// </summary>
        public void RefreshCamera() => _camera = Camera.main;

        // ── Dispose ───────────────────────────────────────────────────

        public void Dispose()
        {
            IUpdateClear();
            EventBus<EnemySceneCreatedEvent>.Deregister(_spawnBinding);
            EventBus<EnemySceneDestroyedEvent>.Deregister(_destroyBinding);
            EventBus<CombatHitEvent>.Deregister(_hitBinding);
            EventBus<CombatDeathEvent>.Deregister(_deathBinding);
            EventBus<HealthChangedEvent>.Deregister(_healthChangedBinding);

            foreach (var pair in _entries)
            {
                if (pair.Value.Widget != null)
                    _pool.Return(pair.Value.Widget);
            }

            _entries.Clear();
        }
    }
}