using UnityEngine;
using UnityEngine.UI;

namespace Galactic1
{
    [System.Serializable]
    public class Bar
    {
        public RawImage targetRenderer;
        [Range(0f, 1f)] public float progress = 1f;
        public Color fillColor = Color.green;
        public Color borderColor = Color.black;
        public Color backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        public bool autoCalculateAspect = true;
        public float aspectRatioOverride = 1f;

        private Material _material;

        // Shader property IDs
        private static readonly int ProgressID = Shader.PropertyToID("_Progress");
        private static readonly int FillColorID = Shader.PropertyToID("_FillColor");
        private static readonly int BorderColorID = Shader.PropertyToID("_BorderColor");
        private static readonly int BackgroundColorID = Shader.PropertyToID("_BackgroundColor");
        private static readonly int AspectID = Shader.PropertyToID("_Aspect");
        
        
        public void Initialize()
        {
            if (targetRenderer == null) return;
            
            _material = Object.Instantiate(targetRenderer.material);
            targetRenderer.material = _material;
            
            UpdateMaterial();
        }


        public void UpdateMaterial()
        {
            if (_material == null || targetRenderer == null) return;

            var rt = targetRenderer.GetComponent<RectTransform>();
            float aspect = autoCalculateAspect
                ? rt.rect.width / rt.rect.height
                : aspectRatioOverride;

            _material.SetFloat(ProgressID, Mathf.Clamp01(progress));
            _material.SetColor(FillColorID, fillColor);
            _material.SetColor(BorderColorID, borderColor);
            _material.SetColor(BackgroundColorID, backgroundColor);
            _material.SetFloat(AspectID, aspect);
        }

        public void SetProgress(float value)
        {
            progress = Mathf.Clamp01(value);
            _material?.SetFloat(ProgressID, progress);
        }

        public void SetFillColor(Color color)
        {
            fillColor = color;
            _material?.SetColor(FillColorID, fillColor);
        }

        public void SetBorderColor(Color color)
        {
            borderColor = color;
            _material?.SetColor(BorderColorID, borderColor);
        }
    }

}