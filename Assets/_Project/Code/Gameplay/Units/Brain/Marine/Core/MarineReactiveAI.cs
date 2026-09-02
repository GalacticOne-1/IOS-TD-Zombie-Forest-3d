using UnityEngine;

namespace Galactic1.Code.Gameplay.Units
{
    /// <summary>
    /// Тонкая MonoBehaviour-обёртка над PlayerCommandBrain.
    ///
    /// Единственная причина её существования — Unity требует MonoBehaviour
    /// для подписки на Physics-коллбэки, [RequireComponent], GetComponent и т.п.
    ///
    /// Вся реальная логика находится в PlayerCommandBrain.
    /// Этот класс только:
    ///   — хранит ссылку на Brain;
    ///   — пробрасывает вызовы из внешнего кода (SquadController, WeaponSystem).
    ///
    /// Tick() здесь нет — UnitInstance тикает Brain напрямую.
    /// </summary>
    [RequireComponent(typeof(PhysicsPerception))]
    [RequireComponent(typeof(SuppressionReceiver))]
    [RequireComponent(typeof(CoverFinder))]
    public sealed class MarineReactiveAI : MonoBehaviour
    {
        private PlayerCommandBrain _brain;

        /// <summary>
        /// Публичный lock — читается SquadController для проверки разрешений.
        /// Делегируем к Brain, чтобы внешний API не изменился.
        /// </summary>
        public AICommandLock CommandLock => _brain?.CommandLock;

        public bool IsEnabled
        {
            get => _brain?.IsEnabled ?? false;
            set { if (_brain != null) _brain.IsEnabled = value; }
        }

        // ── Инициализация ──────────────────────────────────────────────

        /// <summary>
        /// Вызывается из SurvivorInstance.Entity_Dependency_Injection()
        /// вместо старого Initialize(unit, stateMachine, weaponSlot, cfg).
        /// Brain уже создан и прошёл Initialize к этому моменту.
        /// </summary>
        public void BindBrain(PlayerCommandBrain brain)
        {
            _brain = brain;
        }

        private void OnDestroy()
        {
            // Brain.Dispose() вызывается из UnitInstance.Entity_Destroy(),
            // здесь ничего дополнительного не нужно.
        }

        // ── Внешний API (контракт не изменился) ───────────────────────

        /// <summary>
        /// Вызывается SquadController при выдаче нового приказа.
        /// </summary>
        public void OnPlayerCommand(IUnitCommand command)
            => _brain?.OnPlayerCommand(command);

        /// <summary>
        /// Вызывается когда приказ выполнен.
        /// </summary>
        public void OnCommandCompleted()
            => _brain?.NotifyCommandCompleted();

        /// <summary>
        /// Внешнее подавление от тяжёлого оружия.
        /// </summary>
        public void ApplySuppression(Vector3 shotOrigin)
            => _brain?.ApplySuppression(shotOrigin);
    }
}