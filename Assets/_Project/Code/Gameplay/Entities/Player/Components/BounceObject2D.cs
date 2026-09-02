
using UnityEngine;

namespace Galactic1.AbstractFactory.Player_Equipments
{

    /*
     *  Иммитация гравитации с отскоками
     */
    public class BounceObject2D : MonoBehaviour
    {
        [SerializeField] private bool firstContact;
        [Header("Настройки движения")] public float gravity = 9.8f; // Сила "гравитации"
        [Range(0f, 1f)] public float bounceDamping = 0.8f; // Затухание вертикального отскока
        [Range(0f, 1f)] public float horizontalDamping = 0.9f; // Затухание горизонтальной скорости
        public float randomDirectionRange = 1f; // Максимальная случайная составляющая по X




        private Vector2 velocity; // Скорость объекта по X и Y
        private bool isMoving = true; // Флаг движения
        private int bouncesLeft; // Счётчик оставшихся отскоков
        private float targetGroundY; // Текущая "земля", от которой отскакиваем

        private int maxBounces = 3; // Максимальное количество отскоков



        

        
        void Update()
        {
            if (!isMoving)
                return; // Движение отключено, объект лежит на земле

            // Применяем гравитацию
            velocity.y -= gravity * Time.deltaTime;

            // Двигаем объект
            transform.position += (Vector3)(velocity * Time.deltaTime);

            // Затухание горизонтальной скорости
            velocity.x *= horizontalDamping;

            // Проверка контакта с текущей "землёй"
            if (transform.position.y <= targetGroundY)
            {
                transform.position = new Vector3(transform.position.x, targetGroundY, transform.position.z);

                if (bouncesLeft > 0 && Mathf.Abs(velocity.y) > 0.1f)
                {
                    if (firstContact)
                    {
                        // Полная остановка объекта
                        velocity = Vector2.zero;
                        isMoving = false;
                        GetComponent<IBounceObject>().Complete();
                        return;
                    }
                    
                    // Настоящий отскок
                    float randomX = Random.Range(-randomDirectionRange, randomDirectionRange);
                    float randomY = Mathf.Abs(velocity.y) * bounceDamping;

                    velocity = new Vector2(randomX, randomY);
                    bouncesLeft--;

                    // Обновляем текущую "землю" для следующего контакта
                    targetGroundY = transform.position.y;
                }
                else
                {
                    // Полная остановка объекта
                    velocity = Vector2.zero;
                    isMoving = false;
                    GetComponent<IBounceObject>().Complete();
                }
            }
        }
        
        public void Launch(float targetGroundY)
        {
            velocity = Vector2.zero;
            bouncesLeft = maxBounces;
            this.targetGroundY = targetGroundY;

        }

    }
}