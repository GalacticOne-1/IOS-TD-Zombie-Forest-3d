using UnityEngine;

namespace Galactic1
{
    public class SpriteChanger : MonoBehaviour
    {
        [SerializeField] private Sprite[] spriteList; // список спрайтов

        private SpriteRenderer spriteRenderer;

        private void OnEnable()
        {
            // Автоматически получаем компонент SpriteRenderer
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
                if (spriteRenderer == null)
                {
                    Debug.LogError("SpriteRenderer не найден на объекте: " + gameObject.name);
                    return;
                }
            }

            ChangeSprite();
        }

        private void ChangeSprite()
        {
            if (spriteList == null || spriteList.Length == 0)
            {
                Debug.LogWarning("Список спрайтов пуст у объекта: " + gameObject.name);
                return;
            }

            // Выбираем случайный спрайт из списка
            int index = Random.Range(0, spriteList.Length);
            spriteRenderer.sprite = spriteList[index];
        }
    }
    
}