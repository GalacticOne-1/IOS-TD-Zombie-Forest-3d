
using System;
using UnityEngine;


namespace Galactic1.Configs
{
    public static class JsonHelper
    {
        [Serializable]
        private class Wrapper<T> { public T[] Items; }

        public static T[] FromJson<T>(string json)
        {
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(FixJson(json));
            return wrapper?.Items ?? Array.Empty<T>();
        }

        private static string FixJson(string value)
        {
            if (!string.IsNullOrWhiteSpace(value) && !value.TrimStart().StartsWith("{"))
                value = "{\"Items\":" + value + "}";
            return value;
        }
    }
}