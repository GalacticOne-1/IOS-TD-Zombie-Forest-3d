using System;
using System.Collections.Generic;
using UnityEngine;

namespace Galactic1.Runtime.Preview
{
    /// <summary>
    /// Главный сервис получения превью объектов.
    /// </summary>
    public class PreviewService : MonoBehaviour, IGameService
    {
        public PreviewDatabase database;

        private RuntimePreviewRenderer runtimeRenderer;

        private Dictionary<string, Sprite> runtimeCache = new();

        void Awake()
        {
            runtimeRenderer = new RuntimePreviewRenderer();

            if (database != null)
                database.Initialize();
        }

        public void RequestSprite(
            string id,
            GameObject prefab,
            Action<Sprite> callback)
        {
            if (database != null &&
                database.TryGetSprite(id, out var sprite))
            {
                callback(sprite);
                return;
            }
            
            DLog.Alert($"Texture not found! [{id}]", EDlogColor.ORANGE);

            // if (runtimeCache.TryGetValue(id, out sprite))
            // {
            //     callback(sprite);
            //     return;
            // }
            //
            // var rt = runtimeRenderer.Render(prefab);
            //
            // var tex = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
            //
            // RenderTexture.active = rt;
            //
            // tex.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);
            //
            // tex.Apply();
            //
            // sprite = Sprite.Create(
            //     tex,
            //     new Rect(0,0,tex.width,tex.height),
            //     new Vector2(0.5f,0.5f));
            //
            // runtimeCache[id] = sprite;
            //
            // callback(sprite);
        }
    }
}