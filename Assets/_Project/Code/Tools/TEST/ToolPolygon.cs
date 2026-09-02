
using UnityEngine;

namespace Galactic1.Test
{
    public class ToolPolygon : Singleton<ToolPolygon>
    {
        public bool work;
        
        
        [Space]
        [SerializeField] private Vector2 startCoord;
        [SerializeField] private int[] objects;
        
        
        
        
        public void LoadPolygon()
        {
            if (work && !PlayerPrefs.HasKey("Tool_Polygon"))
            {
                PlayerPrefs.SetString("Tool_Polygon", "yes");
                
                Vector2 coord = startCoord;
                var l = objects.Length;
                for (int i = 0; i < l; i++)
                {
                    /*var g = (ServiceLocator.Current.Get<LibController>().equipment[0].list[objects[i]] as FurnitureConfigs).CreateObj();
                    g.transform.position = coord;
                    coord.x += 2;

                    
                    ServiceLocator.Current.Get<ContructRepository>().i_grid_obj = g.GetComponent<ConstructObjData>();
                    var saveData = new CGridObj();
                    saveData.STATE = (int)GridController.EGridState.OBJECT;
                    saveData.assetId = ServiceLocator.Current.Get<LibController>().equipment[0].list[objects[i]].ID;
                    saveData.coord = g.transform.position.ConvertVector2();
                    saveData.hp = 100;// LibController.I.build[HUBLink.asset_construct].hpDefault;
                    // new AddToSaveGrid(saveData, out int saveId);
                    // _PointerHub.i_grid_obj.ID = saveId;
                    GridController.I.AddToGrid(ServiceLocator.Current.Get<ContructRepository>().i_grid_obj.obj.GetComponent<ConstructObjData>(), out int _id);
                
                    // доп. сохранение для ящиков и станков
                    if(ServiceLocator.Current.Get<ContructRepository>().i_grid_obj.obj.GetComponent<INewSaveData>() != null)
                        ServiceLocator.Current.Get<ContructRepository>().i_grid_obj.obj.GetComponent<INewSaveData>().NewSaveData();
                
                    // for tools
                    ServiceLocator.Current.Get<ContructRepository>().i_grid_obj.obj.GetComponent<IConstructActivator>().SetActivated();
                    ServiceLocator.Current.Get<ContructRepository>().i_grid_obj = null;*/
                }
                
                
                // добавляем предметы в первый ящик
                // for (int i = 0; i < 3; i++)
                // {
                //     new InventoryBox_FREE_SLOT_ALL(out int id, out sbyte slot);
                //     new InventoryBox_ADD(id, slot, new CPlayerInventory()
                //     {
                //         unlock = true,
                //         type = 0,
                //         category = 0,
                //         id = Random.Range(0, 20),
                //         volume = 10,
                //     });
                // }
                
                AddResources();
            }
        }



        public void AddResources()
        {
            // *** добавляем ресурсы для крафта
            
            // #1 chest
            Add(0, (int)EItems.Plant_Fiber);
            Add(0, (int)EItems.Plant_Fiber);
            Add(0, (int)EItems.Limestone);
            Add(0, (int)EItems.Limestone);
            Add(0, (int)EItems.Common_Log);
            Add(0, (int)EItems.Common_Log);
            Add(0, (int)EItems.Common_Log);
            Add(0, (int)EItems.Common_Log);
            Add(0, (int)EItems.Common_Log);
            Add(0, (int)EItems.Common_Log);
            Add(0, (int)EItems.Common_Log);
            Add(0, (int)EItems.Common_Log);
            Add(0, (int)EItems.Common_Log);
            Add(0, (int)EItems.Common_Plank);
            Add(0, (int)EItems.Common_Plank);
            
            // #2
            Add(0, (int)EItems.Berry);
            Add(0, (int)EItems.Berry);
            Add(0, (int)EItems.Raw_Meat);
            Add(0, (int)EItems.Raw_Turkey);
            Add(0, (int)EItems.Empty_Bottle);
            Add(0, (int)EItems.Empty_Bottle);
            Add(0, (int)EItems.Charcoal);
            Add(0, (int)EItems.Charcoal);
            Add(0, (int)EItems.Charcoal);
            Add(0, (int)EItems.Charcoal);
            Add(0, (int)EItems.Animal_Rawhide);
            Add(0, (int)EItems.Copper_Bar);
            Add(0, (int)EItems.Copper_Bar);
            Add(0, (int)EItems.Aluminium_Bar);
            Add(0, (int)EItems.Aluminium_Bar);
            
            
            // #3
            Add(0, (int)EItems.Scrap_Metal);
            Add(0, (int)EItems.Scrap_Metal);
            Add(0, (int)EItems.Iron_Ore);
            Add(0, (int)EItems.Iron_Ore);
            Add(0, (int)EItems.Copper_Ore);
            Add(0, (int)EItems.Copper_Ore);
            Add(0, (int)EItems.Iron_bar);
            Add(0, (int)EItems.Iron_bar);
            Add(0, (int)EItems.Iron_bar);
            Add(0, (int)EItems.Iron_bar);
            Add(0, (int)EItems.Iron_bar);
            Add(0, (int)EItems.Copper_Bar);
            Add(0, (int)EItems.Copper_Bar);
            Add(0, (int)EItems.Aluminium_Bar);
            Add(0, (int)EItems.Aluminium_Bar);
            
            
            
            
            
            //Add(1, (int)EEquipment.Pickaxe);
            //Add(1, (int)EEquipment.Hatchet);
            //Add(1, (int)EEquipment.Iron_Hatchet);
            //Add(1, (int)EEquipment.Iron_Pickaxe);
            //Add(1, (int)EEquipment.Spear);
            //Add(1, (int)EEquipment.baseball_bat);
            //Add(1, (int)EEquipment.Crowbar);
            //Add(1, (int)EEquipment.glock_17);
            //Add(1, (int)EEquipment.Iron_Makeshift_Bat);
            //Add(1, (int)EEquipment.Hammer);
            // Add(1, (int)EEquipment.Skull_Crusher);
            // Add(1, (int)EEquipment.Machete);
            // Add(1, (int)EEquipment.glock_17);
            // Add(1, (int)EEquipment.Zip_Gun);
            // Add(1, (int)EEquipment.AK_47);
            // Add(1, (int)EEquipment.Mini_Uzi);
            // Add(1, (int)EEquipment.Shotgun);


            void Add(int type, int key)
            {
                // new InventoryBox_FREE_SLOT_ALL(out int id, out sbyte slot);
                // if (slot != -1)
                // {
                //     new LIB_GetAsset_key(
                //         type,
                //         0,
                //         key,
                //         out AssetItems items,
                //         out InventoryConfigs inventory);
                //     var _s = inventory as AssetEquipmement != null ? (inventory as AssetEquipmement).Durability : 0;
                //     
                //     new InventoryBox_ADD_KEY(id, slot, new CPlayerInventory()
                //     {
                //         unlock = true,
                //         type = type,
                //         category = 0,
                //         id = key,
                //         volume = (short)(type == 1 ? 1 : 20),
                //         strength = (short)_s
                //     });
                // }
            }
        }
    }
}