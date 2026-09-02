using UnityEngine;

namespace Galactic1.Runtime.Preview
{
    /// <summary>
    /// Runtime renderer для генерации превью,
    /// если оно отсутствует в baked atlas.
    /// </summary>
    public class RuntimePreviewRenderer
    {
        private Camera camera;
        private RenderTexture rt;
        private Transform anchor;

        public RuntimePreviewRenderer()
        {
            var go = new GameObject("RuntimePreviewCamera");

            camera = go.AddComponent<Camera>();
            camera.backgroundColor = new Color(0,0,0,0);
            camera.clearFlags = CameraClearFlags.Color;
            camera.orthographic = false;

            rt = new RenderTexture(256,256,16);

            camera.targetTexture = rt;

            anchor = new GameObject("PreviewAnchor").transform;
        }

        public RenderTexture Render(GameObject prefab)
        {
            var obj = GameObject.Instantiate(prefab, anchor);

            FrameObject(obj);

            camera.Render();

            GameObject.Destroy(obj);

            return rt;
        }

        private void FrameObject(GameObject obj)
        {
            var renderers = obj.GetComponentsInChildren<Renderer>();

            Bounds bounds = renderers[0].bounds;

            foreach (var r in renderers)
                bounds.Encapsulate(r.bounds);

            Vector3 center = bounds.center;

            float size = bounds.extents.magnitude;

            camera.transform.position = center + new Vector3(size, size, size);

            camera.transform.LookAt(center);
        }
    }
}