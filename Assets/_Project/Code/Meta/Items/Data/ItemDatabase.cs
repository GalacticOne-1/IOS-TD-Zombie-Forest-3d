using UnityEngine;
using System.Collections.Generic;
using Galactic1.Core.Enums;
using Galactic1.Game.Meta.Items;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Galactic1.Items
{

    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "Game Configs/Inventory/New Item Database")]
    public class ItemDatabase : ScriptableObject
    {
        [SerializeField] private List<ItemConfig> allItems = new();

        /// <summary>
        /// Runtime readonly доступ.
        /// </summary>
        public IReadOnlyList<ItemConfig> Items => allItems;
        
        

       
        
        
#if UNITY_EDITOR

        public void EditorAdd(ItemConfig item)
        {
            if (item == null)
                return;

            if (!allItems.Contains(item))
            {
                Undo.RecordObject(this, "Add Item To Database");
                allItems.Add(item);
                EditorRebuild();
            }
        }

        public void EditorRemove(ItemConfig item)
        {
            if (item == null)
                return;

            if (allItems.Contains(item))
            {
                Undo.RecordObject(this, "Remove Item From Database");
                allItems.Remove(item);
                EditorRebuild();
            }
        }

        public void EditorSetItems(List<ItemConfig> newItems)
        {
            Undo.RecordObject(this, "Replace Item Database");
            allItems = newItems;
            EditorRebuild();
        }

        public void EditorRebuild()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

#endif
        
        
        
        
        
        

        
        
        public IReadOnlyCollection<ItemConfig> GetAllWeapons()
        {
            List<ItemConfig> result = new();
            
            var l = allItems.Count;
            for (int i = 0; i < l; i++)
            {
                if (allItems[i].HasModule<WeaponModule>())
                    result.Add(allItems[i]);
            }

            return result;
        }
        
        public IReadOnlyCollection<ItemConfig> GetAllArmors()
        {
            List<ItemConfig> result = new();
            
            var l = allItems.Count;
            for (int i = 0; i < l; i++)
            {
                if (allItems[i].HasModule<EquipmentModule>())
                    result.Add(allItems[i]);
            }

            return result;
        }
        
        public IReadOnlyCollection<ItemConfig> GetAllCraftStation()
        {
            List<ItemConfig> result = new();
            
            var l = allItems.Count;
            for (int i = 0; i < l; i++)
            {
                if (allItems[i].HasModule<CraftingStationModule>())
                    result.Add(allItems[i]);
            }

            return result;
        }
        
        public IReadOnlyCollection<ItemConfig> GetAllTransport()
        {
            List<ItemConfig> result = new();
            
            var l = allItems.Count;
            for (int i = 0; i < l; i++)
            {
                if (allItems[i].HasModule<VehicleModule>())
                    result.Add(allItems[i]);
            }

            return result;
        }

    }

}