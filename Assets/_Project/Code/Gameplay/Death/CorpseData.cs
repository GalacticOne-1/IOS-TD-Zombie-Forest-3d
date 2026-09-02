
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Galactic1.Gameplay.Death
{
    /// <summary>
    /// DTO, который хранит содержимое трупа/контейнера смерти.
    /// Сериализуемый — можно сохранить в SaveSystem при необходимости.
    /// </summary>
    [Serializable]
    public class CorpseData
    {
        public Vector3 Position;
        public float ExpireTimeUtc; // unix time seconds when corpse expires (0 = never)
        
        // Примитивный список предметов 
        public List<InventorySlotProxy> Items = new();

        // Доп. флаги
        public bool IsPersistent = false; // если true — не удалять автоматически (например в базе)
    }

}