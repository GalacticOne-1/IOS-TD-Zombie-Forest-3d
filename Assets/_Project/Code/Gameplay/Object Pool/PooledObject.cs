using System;
using Gameplay.AbstractFactory;
using UnityEngine;

namespace Galactic1.Structure
{
    public class PooledObject : MonoBehaviour
    {
        private void Awake()
        {
            // if (GetComponent<EnemyEntity>())
            //     GetComponent<_Object_>().OnDeactivate += _ =>
            //         ServiceLocator.Current.Get<EnemyPool>()
            //             .ReturnToPool(gameObject, GetComponent<_Entity>().EntityConfig.ConfigId);
            //
            // else
            //     GetComponent<_Object_>().OnDeactivate += _ =>
            //         ServiceLocator.Current.Get<BuildPool>()
            //             .ReturnToPool(gameObject, GetComponent<_Entity>().EntityConfig.ConfigId);
        }

        // public void Return() => ServiceLocator.Current.Get<EnemyPool>()
        //         .Return(gameObject, GetComponent<_Entity>().EntityConfig.ConfigId);


        private void OnDestroy()
        {
            // if (GetComponent<EnemyEntity>())
            //     GetComponent<_Object_>().OnDeactivate -= _ =>
            //         ServiceLocator.Current.Get<EnemyPool>()
            //             .ReturnToPool(gameObject, GetComponent<_Entity>().EntityConfig.ConfigId);
            //
            // else
            //     GetComponent<_Object_>().OnDeactivate -= _ =>
            //         ServiceLocator.Current.Get<BuildPool>()
            //             .ReturnToPool(gameObject, GetComponent<_Entity>().EntityConfig.ConfigId);
        }
    }

}