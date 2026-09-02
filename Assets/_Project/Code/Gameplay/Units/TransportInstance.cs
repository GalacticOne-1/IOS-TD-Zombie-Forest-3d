using Galactic1.AbstractFactory;

namespace Galactic1.Code.Gameplay.Units
{
    public class TransportInstance : _Entity
    {
        
        
        protected override void OnEnable()
        {
            //ServiceLocator.Current.Get<UnitRepository>().Register(UniqueId, this);
        }

        protected override void OnDisable()
        {
            //ServiceLocator.Current.Get<UnitRepository>().Unregister(UniqueId,this);
        }
        
        
        public override void Entity_Setup<T>(T data)
        {
            // if (data is PlayerLoadData loadData)
            // {
            //     RuntimeSource = loadData.RuntimeUnitViewSource;
            //     
            //     // === для визуала снаряги
            //     equipmentContainer = GetComponent<EquipmentContainer>();
            //     equipmentContainer.BindSource(RuntimeSource.EquipmentService);
            // }
            // else
            // {
            //     Debug.LogError($"Player got wrong data for Initialize {data}");
            // }
        }
        
        
        
    }
}