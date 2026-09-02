using System.Collections;
using Galactic1;
using Galactic1.PoolObject;
using UnityEngine;

namespace Galactic1.AbstractFactory
{
    public class ZombieDeathTester : MonoBehaviour
    {
        [Header("Body Parts Prefabs")]
        public GameObject[] bodyParts;

        [Header("Effect BasicSettings")] 
        public float delay = 1;
        public float explosionForce = 5f;
        public float torqueForce = 200f;
        public Vector3 spawnPosition = Vector3.zero;

        [Header("Shooter Reference")]
        public Transform shooter; // Перетащи сюда игрока или стрелка
        public Transform target;
        
        
        // void Update()
        // {
        //     if (Input.GetKeyDown(KeyCode.Space))
        //     {
        //         if (shooter == null)
        //         {
        //             shooter = FindObjectOfType<PlayerEntity>().tr;
        //         }
        //         
        //         SpawnGore(Random.Range(0, 2) == 0);
        //     }
        // }

        void SpawnGore(bool smallBit)
        {
            // Если стрелок указан, считаем направление
            Vector2 directionAway = Vector2.right; // по умолчанию вправо
            if (shooter != null)
            {
                directionAway = (spawnPosition - shooter.position).normalized;
            }
            
            int partsToSpawn = Random.Range(2, 5);
            for (int i = 0; i < partsToSpawn; i++)
            {
                ServiceLocator.Current.Get<EffectRequestSystem>().Request(
                    new EffectRequest()
                    {
                        Id = null,//smallBit ? "Body Bit Small" : "Body Bit Big",
                        Position = spawnPosition,
                    },

                    EffectPriority.Normal,

                    _ =>
                    {
                        Rigidbody2D rb = _.GetComponent<Rigidbody2D>();
                        if (rb != null)
                        {
                            Vector2 randomOffset = Random.insideUnitCircle * 0.3f;
                            Vector2 finalDirection = (directionAway + randomOffset).normalized;

                            float force = Random.Range(explosionForce - 2, explosionForce);

                            rb.gravityScale = 2.2f;
                            rb.AddForce(finalDirection * force, ForceMode2D.Impulse);
                            rb.AddTorque(Random.Range(-100f, 100f));

                            StartCoroutine(WaitUntilBelowY(rb, spawnPosition.y-1, smallBit));
                        }

                    });
            }
        }

        IEnumerator WaitUntilBelowY(Rigidbody2D rb, float groundY, bool bloodSmall)
        {
            while (rb != null && rb.transform.position.y > groundY)
            {
                yield return null;
            }

            if (rb != null)
            {
                // Отключаем гравитацию и останавливаем
                rb.gravityScale = 0f;
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;

                // Фиксируем позицию по Y
                Vector3 pos = rb.transform.position;
                pos.y = groundY;
                rb.transform.position = pos;
                
                // Спавним кровь в момент "удара об землю"
                //Instantiate(blood, bloodPos, Quaternion.identity);
                //ServiceLocator.Current.Get<PoolManager_>()
                //.PlayEffect(bloodSmall ? "Blood Small" : "BLood Big", pos, 4f);
                ServiceLocator.Current.Get<EffectRequestSystem>().Request(
                    new EffectRequest()
                    {
                        Id = null,//bloodSmall ? "Blood Small" : "Blood Big",
                        Position = pos,
                    },
                    EffectPriority.Normal);
            }
        }

    }


}