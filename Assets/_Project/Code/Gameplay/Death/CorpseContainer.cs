
using System;
using Galactic1.Code.UI.Inventory;
using UnityEngine;

namespace Galactic1.Gameplay.Death
{
    /// <summary>
    /// Компонент префаба трупа (corpse container).
    /// Хранит CorpseData и отвечает за интерактив (открыть, забрать, таймер удаления).
    /// Подразумевается, что prefab содержит коллайдер и UI-иконку.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class CorpseContainer : MonoBehaviour
    {
        public CorpseData Data { get; private set; }

        /// <summary>Время в секундах до автoудаления (0 — не удалять)</summary>
        [SerializeField] private float defaultLifetimeSeconds = 300f;

        private float _expireAtUnix = 0;

        public event Action<CorpseContainer> OnExpired;
        public event Action<CorpseContainer> OnLooted;

        /// <summary>
        /// Инициализация корпуса после создания
        /// </summary>
        public void Initialize(CorpseData data)
        {
            Data = data;

            if (Data.ExpireTimeUtc <= 0 && defaultLifetimeSeconds > 0 && !Data.IsPersistent)
            {
                var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                Data.ExpireTimeUtc = now + (long)defaultLifetimeSeconds;
            }

            _expireAtUnix = Data.ExpireTimeUtc;
        }

        private void Update()
        {
            if (Data == null) return;
            if (Data.IsPersistent) return;

            if (_expireAtUnix > 0)
            {
                var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                if (now >= _expireAtUnix)
                {
                    Expire();
                }
            }
        }

        /// <summary>
        /// Игрок забрал лут — вызываем событие и пометим контейнер
        /// </summary>
        public void LootTaken()
        {
            OnLooted?.Invoke(this);
            // здесь можно проиграть анимацию и затем уничтожить объект
            Destroy(gameObject);
        }

        private void Expire()
        {
            OnExpired?.Invoke(this);
            Destroy(gameObject);
        }

        /// <summary>
        /// Заполнить контейнер из Inventory (используется DeathSystem)
        /// </summary>
        public void FillFromInventory(IInventoryContainer container)
        {
            // Обход и упаковка предметов. Предполагается что IPlayerInventory даёт API для получения списков.
            // Примерный код — адаптируй под вашу реализацию инвентаря.
            // Data = new CorpseData
            // {
            //     Position = transform.position,
            //     Items = container.Inventory.ExtractAllAsStacks(), // <- ожидается метод: извлечь всё (и очистить инвентарь)
            //     IsPersistent = false
            // };
        }
    }
}
