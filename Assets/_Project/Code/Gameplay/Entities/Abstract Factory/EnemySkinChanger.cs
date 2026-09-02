using UnityEngine;
using UnityEngine.U2D.Animation;

namespace Galactic1.AbstractFactory
{

    public class EnemySkinChanger : MonoBehaviour
    {
        // Варианты внешнего вида (названия Label в Sprite Library)
        public string[] skinVariants = { "Builder", "Stroller", "Helmet" };

        void Start()
        {
            // Получаем все SpriteResolver на дочерних костях
            SpriteResolver[] resolvers = GetComponentsInChildren<SpriteResolver>();

            // Выбираем случайный скин
            string chosenLabel = skinVariants[Random.Range(0, skinVariants.Length)];

            // Применяем ко всем SpriteResolver
            foreach (SpriteResolver resolver in resolvers)
            {
                string category = resolver.GetCategory(); // Например: "Body", "Head", "Arm"
                resolver.SetCategoryAndLabel(category, chosenLabel);
            }
        }
    }

}