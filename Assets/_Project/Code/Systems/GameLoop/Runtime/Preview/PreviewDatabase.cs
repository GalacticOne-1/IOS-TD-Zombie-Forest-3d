using System.Collections.Generic;
using UnityEngine;

namespace Galactic1.Runtime.Preview
{
    /// <summary>
    /// ScriptableObject содержащий atlas и UV координаты иконок.
    /// Загружается в runtime.
    /// </summary>
    [CreateAssetMenu(
        fileName = "PreviewDatabase",
        menuName = "Game Configs/Preview/Preview Database")]
    public class PreviewDatabase : ScriptableObject
    {
        public Texture2D atlas;

        public List<PreviewEntry> entries = new();

        private Dictionary<string, PreviewEntry> map;

        public void Initialize()
        {
            map = new Dictionary<string, PreviewEntry>();

            foreach (var e in entries)
                map[e.id] = e;
        }

        public bool TryGetSprite(string id, out Sprite sprite)
        {
            sprite = null;

            if (map == null)
                Initialize();

            if (!map.TryGetValue(id, out var entry))
                return false;

            sprite = Sprite.Create(
                atlas,
                entry.pixelRect,
                new Vector2(0.5f, 0.5f));

            return true;
        }
    }
}