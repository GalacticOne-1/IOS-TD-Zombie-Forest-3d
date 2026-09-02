using System;
using Galactic1.Code.Systems.Raid;
using Galactic1.Game.Meta.Items;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Targeting
{
    public struct TargetingRequest
    {
        public IUnitRuntime User;
        public int QuickSlotIndex;

        public UseModule UseModule;
        public Action<Vector3> OnConfirm;
        public Action OnCancel;
    }
}