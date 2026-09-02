
using Galactic1.Code.Gameplay.Animation;
using Galactic1.Code.Gameplay.Units;
using Galactic1.Code.Gameplay.Weapons.Logic;
using Galactic1.Code.Systems.Raid;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Weapons.View
{
    // ─────────────────────────────────────────────
    //  ISquadMember — что WeaponEquipSystem ожидает от юнита
    // ─────────────────────────────────────────────

    public interface ISquadMember
    {
        ISceneUnit UnitAdapter { get; }
        UnitAnimationController AnimationController { get; }
        Animator Animator { get; }
        IAmmoInventory AmmoInventory { get; }
        IOwnerStatsProvider StatsProvider { get; }
        UnitReloadHandler ReloadHandler { get; }
        WeaponSlot WeaponSlot { get; }
        WeaponHandle CurrentWeaponHandle { get; set; }
        UnitStateMachine StateMachine { get; }
    }
}