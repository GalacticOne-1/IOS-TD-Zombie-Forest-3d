
using System.Collections.Generic;
using Galactic1.Code.Gameplay.Combat;

namespace Galactic1.Code.Gameplay.AoE
{
    /// <summary>
    /// Сервис временных AoE-зон.
    ///
    /// Responsibilities:
    /// - Регистрирует новые зоны.
    /// - Тикает активные зоны каждый кадр.
    /// - Удаляет истёкшие зоны.
    ///
    /// Tick вызывается вручную из GameLoop / MonoBehaviourMaster,
    /// не через MonoBehaviour.Update — архитектурное требование проекта.
    /// </summary>
    public sealed class TemporalAoEService : IGameService, IUpdate
    {
        private CombatEventService combatEventService;
        private readonly List<TemporalAoEZone> _zones = new(16);


        public TemporalAoEService()
        {
            EventBus<SceneServicesResetReusableEvent>.Register(new EventBinding<SceneServicesResetReusableEvent>(() =>
            {
                Dispose();
            }));
        }


        // ─────────────────────────────────────────────
        // Регистрация
        // ─────────────────────────────────────────────

        /// <summary>
        /// Создать и зарегистрировать новую зону по запросу.
        /// Вызывается из GrenadeProjectile.Explode().
        /// </summary>
        public void Register(TemporalAoERequest request)
        {
            combatEventService ??= ServiceLocator.Current.Get<CombatEventService>();
            
            var zone = new TemporalAoEZone(request, combatEventService);
            _zones.Add(zone);
        }

        // ─────────────────────────────────────────────
        // IUpdate — подключается к MonoBehaviourMaster
        // ─────────────────────────────────────────────

        public void IUpdateClear()
        {
            ServiceLocator.Current.Get<MonoBehaviourMaster>().update.Remove(this);
        }

        public void UpdateM()
        {
            // Используем Time.deltaTime через обёртку чтобы не завязываться на Unity напрямую
            // При переходе на GameLoop.Tick(dt) — просто передавать dt сюда
            Tick(UnityEngine.Time.deltaTime);
        }

        // ─────────────────────────────────────────────
        // Tick — будущий путь: GameLoop.Tick(dt)
        // ─────────────────────────────────────────────

        public void Tick(float dt)
        {
            // Обходим с конца чтобы безопасно удалять элементы
            for (int i = _zones.Count - 1; i >= 0; i--)
            {
                _zones[i].Tick(dt);

                if (_zones[i].IsExpired)
                    _zones.RemoveAt(i);
            }
        }

        // ─────────────────────────────────────────────
        // Инициализация
        // ─────────────────────────────────────────────

        /// <summary>
        /// Подключить сервис к update-петле. Вызывается при старте рейда.
        /// </summary>
        public void Initialize()
        {
            ServiceLocator.Current.Get<MonoBehaviourMaster>().update.Add(this);
        }

        /// <summary>
        /// Отключить и очистить все зоны. Вызывается при завершении рейда.
        /// </summary>
        public void Dispose()
        {
            _zones.Clear();
            IUpdateClear();
        }
    }
}