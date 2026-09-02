using Galactic1.Localisation;
using UnityEngine;

namespace Galactic1
{
    [CreateAssetMenu(fileName = "BuildingConfig", menuName = "Game Configs/Entities/Inventory/New Structure Config")]
    public class BuildableConfig : InventoryConfigs
    {

        
        [Space] 
        [SerializeField] private EEquipment assetKey;
        public override int GetAssetKey() => (int)assetKey;
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

        
        public override string GetMainFeatures()
        {
            if(!isDefense) return "";

            
            StatId[] require = new StatId[3] { StatId.Health, StatId.Damage, StatId.SlowAmount };
            byte n = 0;
            var resp = "";
            for (int i = 0; i < 3; i++)
            {
                GetAttribute(require[i], out CAttributes attr);
                if (attr.value > 0)
                {
                    if (n > 0) resp += "\n";
                    resp += $"{ServiceLocator.Current.Get<LocalisationService>().Data.attributes[(byte)attr.type]}: {attr.value}";
                    if(attr.type == StatId.SlowAmount) resp += "%";
                    n++;
                }
            }
                
            return resp;
        }

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