using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Galactic1.Runtime.Preview;

namespace Galactic1.Preview
{
    /// <summary>
    /// Editor tool который bake'ит превью в atlas.
    /// </summary>
    public class PreviewBaker : EditorWindow
    {
        private int iconSize = 256;
        
        private string[] prefabFolders =
        {
            "Assets/Resources/Prefabs/Gameplay/Entities/Facilities",
            "Assets/Resources/Prefabs/Gameplay/Items"
        };

        private Action onComplete;
        
        
        

        [MenuItem("Tools/Preview/Bake Previews")]
        public static void Bake()
        {
            var window = GetWindow<PreviewBaker>();

            window.BakeAll();
        }

        void BakeAll()
        {
            string[] prefabGuids = AssetDatabase.FindAssets(
                "t:Prefab",
                prefabFolders
            );

            Dictionary<PreviewType, List<Texture2D>> textures = new();
            Dictionary<PreviewType, List<string>> ids = new();

            Camera camera = CreatePreviewCamera();

            foreach (var guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (prefab == null)
                    continue;

                var tag = prefab.GetComponent<PreviewTag>();

                if (tag == null)
                    continue;

                var tex = RenderPrefab(camera, prefab);

                if (!textures.ContainsKey(tag.type))
                {
                    textures[tag.type] = new List<Texture2D>();
                    ids[tag.type] = new List<string>();
                }

                textures[tag.type].Add(tex);
                ids[tag.type].Add(prefab.name);
            }

            foreach (var pair in textures)
            {
                BakeAtlas(pair.Key, pair.Value, ids[pair.Key]);
            }

            AssetDatabase.SaveAssets();
            
            //DestroyImmediate(camera.gameObject);

            onComplete();
        }
        
        void BakeAtlas(
            PreviewType type,
            List<Texture2D> textures,
            List<string> ids)
        {
            Texture2D atlas = new Texture2D(4096,4096, TextureFormat.ARGB32, false);

            Rect[] rects = atlas.PackTextures(textures.ToArray(),2,4096);

            string atlasPath = $"Assets/Resources/Preview/PreviewAtlas_{type}.png";
            
            // if (System.IO.File.Exists(atlasPath))
            // {
            //     AssetDatabase.DeleteAsset(atlasPath);
            // }

            byte[] png = atlas.EncodeToPNG();
            System.IO.File.WriteAllBytes(atlasPath, png);

            AssetDatabase.ImportAsset(atlasPath);

            Texture2D atlasAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(atlasPath);

            var db = ScriptableObject.CreateInstance<PreviewDatabase>();

            db.atlas = atlasAsset;

            for(int i=0;i<rects.Length;i++)
            {
                var r = rects[i];

                db.entries.Add(new PreviewEntry
                {
                    id = ids[i],
                    pixelRect = new Rect(
                        r.x * atlas.width,
                        r.y * atlas.height,
                        r.width * atlas.width,
                        r.height * atlas.height)
                });
            }

            string dbPath = $"Assets/Resources/Preview/PreviewDatabase_{type}.asset";

            AssetDatabase.CreateAsset(db, dbPath);
        }

        Camera CreatePreviewCamera()
        {
            var go = new GameObject("PreviewCamera");

            var cam = go.AddComponent<Camera>();

            cam.backgroundColor = new Color(0, 0, 0, 0);
            cam.clearFlags = CameraClearFlags.Color;

            cam.orthographic = false;
            cam.fieldOfView = 30;

            cam.nearClipPlane = 0.01f;
            cam.farClipPlane = 100;

            return cam;
        }

        Texture2D RenderPrefab(Camera cam, GameObject prefab)
        {
            var obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

            SetupObject(obj);

            Bounds bounds = CalculateBounds(obj);

            SetupCamera(cam, bounds);

            SetupLighting();

            var rt = new RenderTexture(iconSize, iconSize, 16, RenderTextureFormat.ARGB32);

            cam.targetTexture = rt;

            cam.Render();

            RenderTexture.active = rt;

            Texture2D tex = new Texture2D(iconSize, iconSize, TextureFormat.ARGB32, false);

            tex.ReadPixels(new Rect(0, 0, iconSize, iconSize), 0, 0);

            tex.Apply();

            cam.targetTexture = null;
            RenderTexture.active = null;

            DestroyImmediate(rt);
            DestroyImmediate(obj);

            return tex;
        }

        void SetupObject(GameObject obj)
        {
            obj.transform.position = Vector3.zero;
            obj.transform.rotation = Quaternion.identity;
        }

        Bounds CalculateBounds(GameObject obj)
        {
            var renderers = obj.GetComponentsInChildren<Renderer>();

            Bounds bounds = renderers[0].bounds;

            foreach (var r in renderers)
                bounds.Encapsulate(r.bounds);

            return bounds;
        }

        void SetupCamera(Camera cam, Bounds bounds)
        {
            Vector3 center = bounds.center;
            float size = bounds.extents.magnitude;

            // Камера выше объекта на высоту + отступ
            float verticalOffset = bounds.extents.y; // половина высоты
            float distance = size * 3f;             // отодвигаем по диагонали

            // изометрический угол
            Quaternion rotation = Quaternion.Euler(30, 45, 0); // стандартный ISO угол
            Vector3 dir = rotation * Vector3.forward;          // направление от центра к камере

            cam.transform.position = center + dir * distance + Vector3.up * verticalOffset;
            cam.transform.LookAt(center);
        }

        void SetupLighting()
        {
            var light = new GameObject("PreviewLight");

            var l = light.AddComponent<Light>();

            l.type = LightType.Directional;

            light.transform.rotation = Quaternion.Euler(50, 30, 0);
            onComplete += () => DestroyImmediate(light.gameObject);
        }
    }
}