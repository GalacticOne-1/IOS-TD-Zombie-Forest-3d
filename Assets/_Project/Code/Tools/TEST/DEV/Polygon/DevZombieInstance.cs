using System.Linq;
using Galactic1.AbstractFactory;
using Galactic1.Code.Core.Lifecycle;
using Galactic1.Code.Gameplay.Damage;
using Galactic1.Code.Gameplay.Targeting;
using Galactic1.Code.Gameplay.Units;
using UnityEngine;

namespace DEV
{
    public class DevZombieInstance : _Entity, IDamageable
    {
        [SerializeField] private float hp = 100;

        public float Hp => hp;



        public override void Awake()
        {
            base.Awake();
            
            GetComponent<DamageReceiverProxy>().Bind(this);
            
            GetComponent<TargetInfoProxy>().Bind(GetComponent<TargetInfoBase>());
            var hitboxProxy = GetComponentsInChildren<HitboxProxy>();
            var l = hitboxProxy.Length;
            for (int i = 0; i < l; i++)
                hitboxProxy[i].Bind();
            
            
            // регистрируем тушу для доступа в поиске цели
            EventBus<SceneActivateEvent>.Register(new EventBinding<SceneActivateEvent>(() =>
            {
                var _proxies = GetComponentsInChildren<IEntityProxy>(true)
                    .OrderBy(p => p.Priority)
                    .ToArray();
                foreach (var p in _proxies)
                    p.Register();
            }));
            
        }

        
        public override void Entity_Setup<T>(T data)
        {
            
        }

        public void ApplyDamage(float damage)
        {
            if (hp <= 0) return;
            
            hp -= damage;

            if (hp <= 0)
            {
                gameObject.SetActive(false);
            }
        }
    }
}