using System;
using System.Collections.Generic;
using UnityEngine;

namespace Galactic1.Game.Meta.Items
{
    /// <summary>
    /// Adds inventory storage to building.
    /// </summary>
    [Serializable]
    public class StorageModule : FacilityModule
    {
        //[Header("Capacity")]
        [SerializeField] 
        private int capacity = 10;

        //[Header("Storage Type")]
        [SerializeField] 
        private StorageType storageType = StorageType.Regular;

        //[Header("Filtering")]
        [SerializeField] 
        private List<ItemTag> allowedTags = new();

        //[Header("Automation")]
        [SerializeField] 
        private bool autoCollectProduction = false;

        [Space, TextArea] 
        [SerializeField] private string specialDescription = "";
        
        
        public int Capacity => capacity;
        public StorageType StorageType => storageType;
        public IReadOnlyList<ItemTag> AllowedTags => allowedTags;
        public bool AutoCollectProduction => autoCollectProduction;

        public string SpecialDescription => specialDescription;
    }
}