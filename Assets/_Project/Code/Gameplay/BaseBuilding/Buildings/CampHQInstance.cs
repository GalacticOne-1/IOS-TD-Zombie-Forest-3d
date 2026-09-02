using Galactic1.AbstractFactory;
using Galactic1.Code.Gameplay.Damage;
using Galactic1.Code.Gameplay.Targeting;
using Galactic1.Code.Gameplay.Units;
using UnityEngine;

namespace Galactic1.Code.Gameplay.BaseBuilding
{
    // главное здание в орде
    public class CampHQInstance : FacilityInstance
    {

        [SerializeField] private Transform unitSpawnPointRoot;

        public Transform UnitSpawnPointRoot => unitSpawnPointRoot;


        protected override void Entity_Dependency_Injection()
        {
            _entityOption = new EntityOption()
            {
                isDetectable = true,
                useGravity = false
            };
            
            
            // 1. target info ─────────────────────────────────
            var targetInfoBase = GetComponent<TargetInfoBase>();
            targetInfoBase.Initialize(SceneContext);
            GetComponent<TargetInfoProxy>().Bind(targetInfoBase);
            // подключаем все хитбоксы на префабе
            var hitboxProxy = GetComponentsInChildren<HitboxProxy>();
            var l = hitboxProxy.Length;
            for (int i = 0; i < l; i++)
                hitboxProxy[i].Bind();
        }
        
        
        public override void Entity_Setup<T>(T data)
        {
            
        }
    }
}