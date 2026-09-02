using UnityEngine;

namespace Galactic1.Code.WorldMap
{
    public abstract class WorldMapLabelBase : MonoBehaviour
    {
        [Header("Offset")] 
        [SerializeField] private Transform transfromRoot;
        [SerializeField] private Transform transfromScale;
        [SerializeField] protected Vector3 worldOffset = new Vector3(0, 2f, 0); // над нодой
        
        [Header("Scaling")]
        [SerializeField] private float minDistance = 30f;
        [SerializeField] private float maxDistance = 200f;
        [SerializeField] private AnimationCurve scaleCurve;
        [SerializeField] private float minScale = 0.7f;
        [SerializeField] private float maxScale = 1.5f;
        
        
        
        protected MapNode boundNode;
        protected Camera cam;
        
        protected float curveValue;
        protected float scale;
        protected float distance, clampDistance;
        
        
        
        
        
        /// <summary>
        /// Привязать маркер к конкретной ноде.
        /// </summary>
        public virtual void Bind(MapNode node)
        {
            boundNode = node;
            cam = Camera.main;
            
            transfromRoot.position = boundNode.transform.position + worldOffset;
        }
        
        
        
        
        private void LateUpdate()
        {
            if (boundNode == null || cam == null)
                return;

            UpdateScale();
        }
        
        
        private void UpdateScale()
        {
            distance = Vector3.Distance(
                cam.transform.position,
                boundNode.transform.position
            );

            clampDistance = Mathf.InverseLerp(minDistance, maxDistance, distance);
            curveValue = scaleCurve.Evaluate(clampDistance);
            scale = Mathf.Lerp(minScale, maxScale, curveValue);

            transfromScale.localScale = Vector3.one * scale;
        }
    }
}