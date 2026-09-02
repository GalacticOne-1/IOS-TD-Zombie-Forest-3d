using System;
using System.Collections.Generic;


namespace Galactic1.Utility
{
    public static class DictionaryUtility
    {
        // Преобразовать словарь в список
        public static List<KeyValuePairSerializable<TKey, TValue>> ToList<TKey, TValue>(Dictionary<TKey, TValue> dict)
        {
            var list = new List<KeyValuePairSerializable<TKey, TValue>>(dict.Count);
            foreach (var kvp in dict)
            {
                list.Add(new KeyValuePairSerializable<TKey, TValue>(kvp.Key, kvp.Value));
            }

            return list;
        }

        // Преобразовать список обратно в словарь
        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(List<KeyValuePairSerializable<TKey, TValue>> list)
        {
            var dict = new Dictionary<TKey, TValue>(list.Count);
            foreach (var kvp in list)
            {
                dict[kvp.Key] = kvp.Value;
            }

            return dict;
        }
    }
    
    [Serializable]
    public struct KeyValuePairSerializable<TKey, TValue>
    {
        public TKey Key;
        public TValue Value;

        public KeyValuePairSerializable(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }
    }
}