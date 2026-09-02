
using System.Collections.Generic;
using Galactic1.Code.Systems.GameTime;
using Galactic1.Game.Buildings.Proxy;
using Galactic1.Game.Meta.Items;

namespace Galactic1.Code.Systems.Runtime.Building
{
    /// <summary>
    /// Runtime логика здания-хранилища.
    ///
    /// Отвечает за:
    /// • регистрацию storage в CampRuntime
    /// • проверку фильтра предметов
    /// • хранение предметов
    /// • автоматический сбор продукции
    /// </summary>
    public class StorageFacilityRuntime : 
        BaseCampFacilityRuntime, 
        IStorageFacilityRuntime
    {
        
        private readonly StorageModule _module;
        public override FacilityType Type => FacilityType.Storage;
        public override bool CanUpgrade => false;

        

        public StorageType StorageType => _module.StorageType;

        public StorageModule Module => _module;
        public IReadOnlyList<ItemTag> SupportedTags { get; }
        

        public StorageFacilityRuntime(
            FacilityProxy proxy,
            StorageModule module,
            CampRuntime campRuntime,
            GameTimeService timeService)
            : base(proxy, module, timeService)
        {
            _module = module;

            SupportedTags = _module.AllowedTags;
            
            // регистрация storage в лагере
            campRuntime.RegisterStorage(module);
        }


        public override void Dispose(){}
        
        public bool Supports(ItemTag tag)
        {
            var l = SupportedTags.Count;
            for (int i = 0; i < l; i++)
            {
                if (SupportedTags[i] == tag)
                    return true;
            }

            return false;
        }
    }
}