using System;

namespace Galactic1.Code.Gameplay.Units
{
    // ─────────────────────────────────────────────
    //  6. AICommandLock — блокирует реактивный AI
    //     пока активен приказ от игрока.
    //
    //  Принцип:
    //    SquadController.Execute(cmd) → Lock()
    //    UnitStateMachine достиг финального состояния → Unlock()
    //
    //  MarineReactiveAI проверяет IsLocked перед
    //  каждым реактивным действием.
    // ─────────────────────────────────────────────

    public sealed class AICommandLock
    {
        public bool IsLocked { get; private set; }

        // Тип текущего приказа — AI может реагировать
        // даже под локом если это атака (авто-цель замена)
        public Type LockedCommandType { get; private set; }

        public void Lock(IUnitCommand command)
        {
            IsLocked = true;
            LockedCommandType = command?.GetType();
        }

        public void Unlock()
        {
            IsLocked = false;
            LockedCommandType = null;
        }

        // Авто-атака разрешена даже если залочены на Move/Cover —
        // морпех стреляет по врагу во время движения
        public bool AllowAutoAttack =>
            !IsLocked; //|| LockedCommandType == typeof(MoveCommand)
                      //|| LockedCommandType == typeof(TakeCoverCommand);

        // Авто-укрытие разрешено только если нет активного приказа
        public bool AllowAutoCover => !IsLocked;

        // Авто-reload всегда разрешён
        public bool AllowAutoReload => true;
    }
}