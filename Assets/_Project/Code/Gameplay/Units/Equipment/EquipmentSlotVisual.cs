using System;
using Galactic1.Core.Enums;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Equipment
{
    [Serializable]
    public class EquipmentSlotVisual
    {
        public EquipSlotType slot;
        public Transform attachment1;     // куда ставится модель
        public Transform attachment2;
    }
}