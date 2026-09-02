using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Core.Rendering
{
    /// <summary>
    /// Универсальный сервис для изменения цветов и эффектов на материалах.
    /// Поддерживает:
    /// - SpriteRenderer / MeshRenderer / SkinnedMeshRenderer через MaterialPropertyBlock
    /// - UI Image / RawImage через инстанс материала
    /// </summary>
    public sealed class VisualMaterialService : IGameService
    {
        private readonly Dictionary<Renderer, MaterialPropertyBlock> rendererBlocks = new();
        private readonly Dictionary<Graphic, (Material original, Material instance)> mapMaterialInstances = new();

        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int OutlineColorId = Shader.PropertyToID("_OutlineColor");
        private static readonly int OutlineSizeId = Shader.PropertyToID("_OutlineSize");
        private static readonly int FlashAmountId = Shader.PropertyToID("_FlashAmount");

        
        
        
        
        #region Registration

        public void RegisterRenderer(Renderer renderer)
        {
            if (renderer == null) return;
            if (!rendererBlocks.ContainsKey(renderer))
                rendererBlocks[renderer] = new MaterialPropertyBlock();
        }

        public void UnregisterRenderer(Renderer renderer)
        {
            if (renderer == null) return;
            rendererBlocks.Remove(renderer);
        }

        public void RegisterGraphic(Graphic graphic)
        {
            if (graphic == null || mapMaterialInstances.ContainsKey(graphic)) 
                return;

            if (graphic.material != null)
            {
                var original = graphic.material;
                var instance = new Material(graphic.material);
                
                graphic.material = instance;
                
                mapMaterialInstances.Add(graphic, (original, instance));
            }
        }

        public void UnregisterGraphic(Graphic graphic)
        {
            if (graphic == null) return;

            if (mapMaterialInstances.TryGetValue(graphic, out var pair))
            {
                // вернуть оригинальный материал
                graphic.material = pair.original;

                // уничтожить инстанс
                Object.Destroy(pair.instance);
                
                mapMaterialInstances.Remove(graphic);
            }
        }

        #endregion

        #region Color Control (Renderer)

        public void SetColor(Renderer renderer, Color color)
        {
            if (!rendererBlocks.TryGetValue(renderer, out var mpb)) return;

            renderer.GetPropertyBlock(mpb);
            mpb.SetColor(ColorId, color);
            renderer.SetPropertyBlock(mpb);
        }

        public void SetOutlineColor(Renderer renderer, Color color)
        {
            if (!rendererBlocks.TryGetValue(renderer, out var mpb)) return;

            renderer.GetPropertyBlock(mpb);
            mpb.SetColor(OutlineColorId, color);
            renderer.SetPropertyBlock(mpb);
        }
        
        /// <summary>
        /// Устанавливает размер обводки для Renderer (SpriteRenderer, MeshRenderer, SkinnedMeshRenderer)
        /// </summary>
        public void SetOutlineSize(Renderer renderer, float size)
        {
            if (!rendererBlocks.TryGetValue(renderer, out var mpb)) return;

            renderer.GetPropertyBlock(mpb);
            mpb.SetFloat(OutlineSizeId, Mathf.Max(0f, size));
            renderer.SetPropertyBlock(mpb);
        }

        public void SetFlash(Renderer renderer, float amount01)
        {
            if (!rendererBlocks.TryGetValue(renderer, out var mpb)) return;

            renderer.GetPropertyBlock(mpb);
            mpb.SetFloat(FlashAmountId, Mathf.Clamp01(amount01));
            renderer.SetPropertyBlock(mpb);
        }

        public void Reset(Renderer renderer)
        {
            if (!rendererBlocks.ContainsKey(renderer)) return;
            renderer.SetPropertyBlock(null);
        }

        #endregion

        #region Color Control (UI Graphic)

        public void SetColor(Graphic graphic, Color color)
        {
            if (mapMaterialInstances.TryGetValue(graphic, out var mat))
            {
                mat.instance.SetColor(ColorId, color);
            }
        }

        public void SetOutlineColor(Graphic graphic, Color color)
        {
            if (mapMaterialInstances.TryGetValue(graphic, out var mat))
            {
                mat.instance.SetColor(OutlineColorId, color);
            }
        }
        
        /// <summary>
        /// Устанавливает размер обводки для UI Graphic (Image, RawImage)
        /// </summary>
        public void SetOutlineSize(Graphic graphic, float size)
        {
            if (mapMaterialInstances.TryGetValue(graphic, out var mat))
            {
                mat.instance.SetFloat(OutlineSizeId, Mathf.Max(0f, size));
            }
        }

        public void SetFlash(Graphic graphic, float amount01)
        {
            if (mapMaterialInstances.TryGetValue(graphic, out var mat))
            {
                mat.instance.SetFloat(FlashAmountId, Mathf.Clamp01(amount01));
            }
        }

        public void Reset(Graphic graphic)
        {
            if (mapMaterialInstances.TryGetValue(graphic, out var mat))
            {
                // возвращаем default цвет
                mat.instance.SetColor(ColorId, Color.white);
                mat.instance.SetColor(OutlineColorId, Color.white);
                mat.instance.SetFloat(FlashAmountId, 0f);
            }
        }

        #endregion
    }
}
