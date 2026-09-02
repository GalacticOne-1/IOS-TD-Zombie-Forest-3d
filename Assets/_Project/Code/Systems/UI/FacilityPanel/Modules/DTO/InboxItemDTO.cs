using System;
using Galactic1.Core.Enums;
using Galactic1.Game.Meta.Items;
using UnityEngine;

namespace Galactic1.Game.UI.Buildings.DTO
{
    /// <summary>
    /// DTO одного предмета во входящих.
    /// </summary>
    public class InboxItemDTO
    {
        public string SlotId;

        public ItemConfig Item;

        public int Count;

        public float Durability01;
        public int DurabilityCurrent;
        
        public int RemainingHours;
    }
}