
using Galactic1.AbstractFactory;
using Galactic1.Code.AbstractFactory;
using Galactic1.Code.Gameplay.Damage;
using Galactic1.Code.Gameplay.Targeting;
using Galactic1.Code.Gameplay.Units;
using UnityEngine;

namespace Galactic1.Code.Gameplay.BaseBuilding
{
    // защитные здания в орде
    public class FacilityDefenseInstance : FacilityInstance
    {
        [SerializeField] private FacilityDamageVisualizer _damageVisualizer;

        private IDamageableSceneFacility _damageable;
        
        
        
        
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

        public override void Bind(ISceneFacility adapter)
        {
            base.Bind(adapter);

            _damageable = adapter as IDamageableSceneFacility;

            if (_damageable == null)
                return; // не боевое сооружение

            if (_damageVisualizer == null)
                _damageVisualizer = GetComponentInChildren<FacilityDamageVisualizer>(true);
            
            _damageVisualizer.Initialize(_damageable.CurrentHP, _damageable.MaxHP);
            _damageable.OnHealthChanged += HandleHealthChanged;
            _damageable.OnDestroyed += Entity_Die;
        }

        public override void Entity_Destroy()
        {
            if (_damageable != null)
            {
                _damageable.OnHealthChanged -= HandleHealthChanged;
                _damageable.OnDestroyed -= Entity_Die;
                _damageable = null;
            }

            base.Entity_Destroy();
        }

        private void HandleHealthChanged(float currentHp, float maxHp)
        {
            _damageVisualizer.SetHealth(currentHp, maxHp);
        }
    }
}