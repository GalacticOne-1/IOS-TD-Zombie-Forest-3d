using Galactic1;
using UnityEngine;

namespace Galactic1
{


    public class CONSTRUCT_Get_Amount_key
    {
        /// <summary>
        /// Посчитает все объекты нужного ассета на базе игрока
        /// </summary>
        /// <param name="key"></param>
        /// <param name="exist_obj"></param>
        public CONSTRUCT_Get_Amount_key(int key, out int exist_obj)
        {
            exist_obj = 0;

            new LIB_Convert_AssetKey_To_Id(1, key, out int id);
            
            // var l = GAMEPLAY_old.DataGameplay().gridObj.Length;
            // for (int i = 0; i < l; i++)
            // {
            //     if (GAMEPLAY_old.DataGameplay().gridObj[i].assetId == id)
            //         exist_obj++;
            // }
        }
    }
    
    public class CONSTRUCT_Get_Amount_id
    {
        /// <summary>
        /// Посчитает все объекты нужного ассета на базе игрока
        /// </summary>
        /// <param name="key"></param>
        /// <param name="exist_obj"></param>
        public CONSTRUCT_Get_Amount_id(int id, out int exist_obj)
        {
            exist_obj = 0;
            
            // var l = GAMEPLAY_old.DataGameplay().gridObj.Length;
            // for (int i = 0; i < l; i++)
            // {
            //     if (GAMEPLAY_old.DataGameplay().gridObj[i].assetId == id)
            //         exist_obj++;
            // }
        }
    }
}