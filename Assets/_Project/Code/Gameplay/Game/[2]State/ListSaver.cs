using System;
using System.Collections.Generic;
using UnityEngine;


namespace Galactic1.Core
{
    public static class ListSaver
    {
        /// <summary>
        /// Устанавливает объект по ключу (обновляет или добавляет).
        /// </summary>
        public static void Set<TEntry, TValue>(
            string key, 
            TValue value, 
            ref List<TEntry> list)
            where TEntry : IKeyValueClass<TValue>, new()
        {
            if (list == null) 
                list = new List<TEntry>();

            int index = list.FindIndex(x => x.Key == key);
            if (index >= 0)
            {
                var entry = list[index];
                entry.Value = value;
                list[index] = entry;
            }
            else
            {
                list.Add(new TEntry { Key = key, Value = value });
            }
        }

        /// <summary>
        /// Получает объект по ключу или возвращает default.
        /// <br/>(Если массив пустой сделает new)
        /// </summary>
        public static TValue Get<TEntry, TValue>(
            string key, 
            ref List<TEntry> list)
            where TEntry : IKeyValueClass<TValue>, new()
        {
            if (list == null)
                list = new List<TEntry>();

            int index = list.FindIndex(x => x.Key == key);
            if (index >= 0)
            {
                return list[index].Value;
            }

            // Если не найдено — создаём новый элемент
            var newEntry = new TEntry { Key = key, Value = Activator.CreateInstance<TValue>() };
            list.Add(newEntry);
            return newEntry.Value;
        }

        /// <summary>
        /// Удаляет объект по ключу. Возвращает true, если удалён.
        /// </summary>
        public static bool Remove<TEntry, TValue>(
            string key, 
            List<TEntry> list)
            where TEntry : IKeyValueClass<TValue>
        {
            if (list == null) 
                return false;
            
            int index = list.FindIndex(x => x.Key == key);
            if (index >= 0)
            {
                list.RemoveAt(index);
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Интерфейс для элементов списка, которые хранят key/class.
    /// </summary>
    public interface IKeyValueClass<TValue>
    {
        string Key { get; set; }
        TValue Value { get; set; }
    }

    [Serializable]
    public class ObjectEntry<T> : IKeyValueClass<T>
    {
        [SerializeField] private string key;
        [SerializeField] private T value;

        public string Key
        {
            get => key;
            set => key = value;
        }

        public T Value
        {
            get => value;
            set => this.value = value;
        }
    }

}