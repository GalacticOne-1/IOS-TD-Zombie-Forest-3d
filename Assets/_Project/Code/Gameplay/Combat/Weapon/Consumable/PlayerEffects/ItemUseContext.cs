using System;
using System.Collections.Generic;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.Systems.Raid;
using Galactic1.Game.Meta.Items;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Effect
{
    public sealed class ItemUseContext
    {
        public IUnitRuntime User;
        public ISceneUnit SceneUnit;
        public IInventorySource InventorySource;  // где лежит предмет
        public int SlotIndex;
        public int QuickSlotIndex;
        public bool UseSmartTarget;
        public List<IUnitRuntime> SquadMembers;
        public Vector3 TargetPosition;
        public Transform SpawnOrigin;
        public UseModule UseModule;
        
        /// выполняется сразу по активации способности (не ждет анимацию) 
        public Action OnConfirmed;
        public Action OnCancelled;
        
        public AbilityAnimationType? AnimationType;
    }
}