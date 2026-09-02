using Galactic1.Game.Meta.Items;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Grid
{
    public static class GridBlockedMaskGenerator
    {
        
        private const GridZoneTag AlwaysBlockedTag = GridZoneTag.Locked;
        private static Color32[] _pixels;
        
        
        
        public static Texture2D Build(
            GridSettingsConfig gridConfig,
            GridBlockedAreaService blockedAreaService,
            FacilityModule config = null)
        {
            int width = gridConfig.GridSize.x;
            int height = gridConfig.GridSize.y;

            var texture = new Texture2D(width, height, TextureFormat.R8, false, true)
            {
                name = "GridBlockedMask",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            Fill(texture, gridConfig, blockedAreaService, config);

            return texture;
        }

        /// <summary>
        /// Пересобирает существующую текстуру на месте (без аллокации новой).
        /// Используется как для editor-time live-update, так и для
        /// runtime-переключения маски под выбранное здание.
        /// </summary>
        public static void Rebuild(
            Texture2D texture,
            GridSettingsConfig gridConfig,
            GridBlockedAreaService blockedAreaService,
            FacilityModule config = null)
        {
            int width = gridConfig.GridSize.x;
            int height = gridConfig.GridSize.y;

            if (texture.width != width || texture.height != height)
                texture.Reinitialize(width, height);

            Fill(texture, gridConfig, blockedAreaService, config);
        }

        private static void Fill(
            Texture2D texture,
            GridSettingsConfig gridConfig,
            GridBlockedAreaService blockedAreaService,
            FacilityModule config)
        {
            int width = gridConfig.GridSize.x;
            int height = gridConfig.GridSize.y;

            var blocked = blockedAreaService.BlockedCells;
            
            if (_pixels == null || _pixels.Length != width * height)
                _pixels = new Color32[width * height];

            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                var cell = new Vector2Int(x, y);

                // tag == GridZoneTag.None, если клетка нигде не размечена
                blocked.TryGetValue(cell, out var tag);

                bool isBlocked = config == null
                    ? tag == AlwaysBlockedTag 
                    : !config.IsZoneAllowed(tag);

                byte v = isBlocked ? (byte)255 : (byte)0;
                _pixels[y * width + x] = new Color32(v, v, v, v);
            }

            texture.SetPixels32(_pixels);
            texture.Apply(false, false);
        }
    }
}