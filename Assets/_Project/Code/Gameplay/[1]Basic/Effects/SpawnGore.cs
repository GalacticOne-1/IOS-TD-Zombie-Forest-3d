using System.Collections;
using System.Collections.Generic;
using Galactic1;
using Galactic1.PoolObject;
using UnityEngine;

namespace Galactic1.AbstractFactory
{
    public class SpawnGore : MonoBehaviour
    {
        public Transform spawnPosition;
        
        
        private int explosionForce = 8;

        
        
        

        public void SpawnSmallBites(GameObject shooter)
        {
            Spawn(shooter, true);
        }
        
        public void SpawnBigBites(GameObject shooter)
        {
            Spawn(shooter, false);
        }

        void Spawn(GameObject shooter, bool smallBit)
        {
            float goreChance = 0.6f; // 60% шанс

            if (Random.value > goreChance)
                return; // Пропускаем gore-эффект
            
            // Если стрелок указан, считаем направление
            Vector2 directionAway = shooter == null 
                ? Vector2.up 
                : (spawnPosition.position - shooter.transform.position).normalized;

            // Сколько кусков заспавнить (рандомно от 2 до 5, но не больше, чем есть)
            int minParts = 2;
            int maxParts = 5;
            int partsToSpawn = Random.Range(minParts, maxParts);

            // Перемешиваем список, чтобы выбрать случайные части
            //List<GameObject> shuffledParts = new List<GameObject>(bodyParts);
            //ShuffleList(shuffledParts);

            // Используем первые N элементов из перемешанного списка
            for (int i = 0; i < partsToSpawn; i++)
            {
                //GameObject partPrefab = shuffledParts[i];
                //GameObject instance = Instantiate(partPrefab, spawnPosition.position, Quaternion.Euler(0, 0, Random.Range(0, 360)));
                ServiceLocator.Current.Get<EffectRequestSystem>().Request(
                    new EffectRequest()
                    {
                        Id = null,//smallBit ? "Body Bit Small" : "Body Bit Big",
                        Position = spawnPosition.position,
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

                            StartCoroutine(WaitUntilBelowY(rb, transform.position.y, smallBit));
                        }

                    });
            }
        }
        
        void ShuffleList<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
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