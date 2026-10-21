using Galactic1.Localisation;
using UnityEngine;

namespace Galactic1
{
    [CreateAssetMenu(fileName = "BuildingConfig", menuName = "Game Configs/Entities/Inventory/New Structure Config")]
    public class BuildableConfig 
    {

        
        [Space] 
        [SerializeField] private EEquipment assetKey;
        public int GetAssetKey() => (int)assetKey;
        [SerializeField] private bool isDefense;


        #region PRODUCTION

        [Header("* PRODUCTION")]
        [SerializeField] private CRecipeType recipeType;

        public CRecipeType RecipeType => recipeType;

        [System.Serializable]
        public struct CRecipeType
        {
            public byte requireSlot;                // кол-во активных слотов
            public bool requireFuel;                // нужно топливо
        }


        [SerializeField] private EItems[] productionList;
        public EItems[] ProductionList => productionList;

        #endregion




        [Space] [Header("* Features")] 
        public byte requireTime;
        public sbyte requireFloor;
        public byte maxAmount;
        [Range(0,6)]
        public byte energy;

        public byte requiresBlueprint;              // 0 = доступно сразу

        

        #region PREFAB

        
        
        /// <summary>
        /// Создание объекта в сцене и настройка для работы
        /// </summary>
        /// <param name="hold"></param>
        /// <returns></returns>
        public GameObject CreateObj()
        {
            GameObject g = null;// prefab.CreateGO(ServiceLocator.Current.Get<Environment>().playerObj);
            Obj_add_func(g);
            
            //g.GetComponent<WeaponABS>().weapon = wp;
            return g;
        }
        
        /// <summary>
        /// Доп загрузка для разных объектов
        /// </summary>
        /// <param name="obj"></param>
        protected virtual void Obj_add_func(GameObject obj){}
        
        

        #endregion
        
    }
}