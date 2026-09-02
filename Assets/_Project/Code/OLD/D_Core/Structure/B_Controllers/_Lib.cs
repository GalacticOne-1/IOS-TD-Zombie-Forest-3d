using Galactic1.Mobile;
using Galactic1;
using UnityEngine;

namespace Galactic1
{
    /*
     *     Для получение ассeтов из библиотеки
     */





    public class LIB_GetEquipments
    {
        /// <summary>
        /// Передаст список ассетов
        /// </summary>
        /// <param name="list"></param>
        public LIB_GetEquipments(out InventoryConfigs[] list)
            => list = null;// ServiceLocator.Current.Get<LibController>().equipment[0].list;
    }

    public class LIB_GetAssetEquipment
    {
        /// <summary>
        /// Передаст ассет
        /// </summary>
        /// <param name="id"></param>
        /// <param name="asset"></param>
        public LIB_GetAssetEquipment(int id, out InventoryConfigs asset)
            => asset = null;// ServiceLocator.Current.Get<LibController>().equipment[0].list[id];

    }




    public class LIB_GetAsset_key
    {
        /// <summary>
        /// Находит нужный ассет по ключу 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="category"></param>
        /// <param name="asset_key">key enum</param>
        /// <param name="assetItems"></param>
        /// <param name="equipment"></param>
        public LIB_GetAsset_key(int type, int category, int asset_key, out AssetItems assetItems, out InventoryConfigs equipment)
        {
            assetItems = null;
            equipment = null;
            
            //DLog.Alert($">> get asset {type} _ {category} _ {asset_key}", EDlogColor.ORANGE);


            /*switch (type)
            {
                // GOODS
                case 0:
                {
                    
                    var l = ServiceLocator.Current.Get<LibController>().items.Length;
                    for (int i = 0; i < l; i++)
                    {
                        //DLog.Alert($"check {ServiceLocator.Current.Get<LibController>().items[i].GetAssetKey()} == {asset_key}");
                        if (ServiceLocator.Current.Get<LibController>().items[i].GetAssetKey() == asset_key)
                        {
                            assetItems = ServiceLocator.Current.Get<LibController>().items[i];
                            return;
                        }
                    }
                } break;

                // EQUIPMENT
                case 1:
                {
                    var l = ServiceLocator.Current.Get<LibController>().equipment[category].list.Length;
                    for (int i = 0; i < l; i++)
                    {
                        if (ServiceLocator.Current.Get<LibController>().equipment[category].list[i].GetAssetKey() == asset_key)
                        {
                            equipment = ServiceLocator.Current.Get<LibController>().equipment[category].list[i];
                            return;
                        }
                    }
                } break;
            }
            
            if(!assetItems && !equipment)
            {
                var _key = "";
                if (type == 0) _key = $"{(EItems)asset_key}";
                else _key = $"{(EEquipment)asset_key}";
                Debug.LogError($"Asset empty: {type}_{category}_{_key}");
            }*/
        }
    }
    
    
    public class LIB_GetAsset_id
    {
        /// <summary>
        /// Находит нужный ассет по id 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="category"></param>
        /// <param name="id">[] lib</param>
        /// <param name="assetItems"></param>
        /// <param name="equipment"></param>
        public LIB_GetAsset_id(int type, int category, int id, out AssetItems assetItems, out InventoryConfigs equipment)
        {
            assetItems = null;
            equipment = null;


            if (id == -1)
            {
                Debug.LogError($"LIB_GetAsset_id: {type}_{category}_{id}");
                FBA.CRASH($"LIB_GetAsset_id: {type}_{category}_{id}");
                return;
            }
            
            /*switch (type)
            {
                // GOODS
                case 0:
                {
                    assetItems = ServiceLocator.Current.Get<LibController>().items[id];
                } break;

                // EQUIPMENT
                case 1:
                {
                    equipment = ServiceLocator.Current.Get<LibController>().equipment[category].list[id];
                } break;
            }*/
            
            if(!assetItems && !equipment)
            {
                Debug.LogError($"Asset empty: {type}_{category}_{id}");
            }
        }
    }


    public class LIB_Convert_AssetKey_To_Id
    {
        /// <summary>
        /// Конвертирует key ассета в id for Lib[]
        /// </summary>
        /// <param name="type"></param>
        /// <param name="asset_key">enum</param>
        /// <param name="asset_id">Lib[]</param>
        public LIB_Convert_AssetKey_To_Id(int type, int asset_key, out int asset_id)
        {
            asset_id = -1;
            /*switch (type)
            {
                // GOODS
                case 0:
                {
                    var l = ServiceLocator.Current.Get<LibController>().items.Length;
                    for (int i = 0; i < l; i++)
                    {
                        if (ServiceLocator.Current.Get<LibController>().items[i].GetAssetKey() == asset_key)
                        {
                            asset_id = i;
                            break;
                        }
                    }
                    
                    if (asset_id == -1)
                    {
                        Debug.LogError($"LIB_Convert_AssetKey_To_Id: In Lib[] not have item < {(EItems)asset_key} >");
                    }
                } break;

                // EQUIPMENT
                case 1:
                {
                    var l = ServiceLocator.Current.Get<LibController>().equipment[0].list.Length;
                    for (int i = 0; i < l; i++)
                    {
                        if (ServiceLocator.Current.Get<LibController>().equipment[0].list[i].GetAssetKey() == asset_key)
                        {
                            asset_id = i;
                            break;
                        }
                    }
                    
                    if (asset_id == -1)
                    {
                        Debug.LogError($"LIB_Convert_AssetKey_To_Id: In Lib[] not have item < {(EEquipment)asset_key} >");
                    }
                } break;
            }*/

            
        }
    }
}