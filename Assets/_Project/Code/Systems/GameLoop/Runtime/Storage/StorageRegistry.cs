using System;
using System.Collections.Generic;
using Galactic1.Code.Systems.Runtime.Building;
using Galactic1.Game.Meta.Items;

namespace Galactic1.Code.Systems.Runtime
{
    /// <summary>
    /// Реестр всех построенных storage зданий.
    /// 
    /// Используется для проверки:
    /// разрешён ли автосбор предмета по ItemTag.
    /// </summary>
    public sealed class StorageRegistry : IGameService
    {
        private readonly List<IStorageFacilityRuntime> _storages = new();
        
        public event Action OnStorageChanged;
        
        

        /// <summary>
        /// Регистрация storage при создании runtime здания
        /// </summary>
        public void Register(IStorageFacilityRuntime storage)
        {
            if (storage == null)
                return;

            if (_storages.Contains(storage))
                return;

            _storages.Add(storage);
            OnStorageChanged?.Invoke();
        }

        /// <summary>
        /// Удаление storage при уничтожении здания
        /// </summary>
        public void Unregister(IStorageFacilityRuntime storage)
        {
            if (storage == null)
                return;

            _storages.Remove(storage);
            OnStorageChanged?.Invoke();
        }

        /// <summary>
        /// Проверяет существует ли storage поддерживающий tag
        /// </summary>
        public bool HasStorageForTag(ItemTag tag)
        {
            foreach (var storage in _storages)
            {
                if (storage.Supports(tag))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Проверяет поддерживается ли любой из тегов
        /// </summary>
        public bool HasStorageForAnyTag(IReadOnlyList<ItemTag> tags)
        {
            foreach (var tag in tags)
            {
                if (HasStorageForTag(tag))
                    return true;
            }

            return false;
        }
    }
}