using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Galactic1.EditorTools.PrefabScreenshot
{
    public static partial class PrefabScreenshotGenerator
    {
        /// <summary>
        /// Temporary preview scene.
        /// </summary>
        private static Scene _previewScene;

        /// <summary>
        /// Temporary camera.
        /// </summary>
        private static Camera _camera;

        /// <summary>
        /// Temporary directional light.
        /// </summary>
        private static Light _light;

        /// <summary>
        /// Ambient/light-rig constants. Key light offset is expressed relative to
        /// the camera's own rotation, so the lit side of the object always matches
        /// the side the camera is actually looking at, regardless of cameraYaw/Pitch.
        /// </summary>
        private static readonly Quaternion KeyLightCameraOffset = Quaternion.Euler(40f, -35f, 0f);

        private static readonly Color AmbientFillColor = new Color(0.35f, 0.35f, 0.4f);

        /// <summary>
        /// Main entry point.
        /// </summary>
        public static void Generate(ScreenshotSettings settings)
        {
            if (settings.prefabFolder == null)
            {
                EditorUtility.DisplayDialog(
                    "Prefab Screenshot",
                    "Please select a prefab folder.",
                    "OK");

                return;
            }

            if (settings.outputFolder == null)
            {
                EditorUtility.DisplayDialog(
                    "Prefab Screenshot",
                    "Please select an output folder.",
                    "OK");

                return;
            }

            string prefabFolder =
                AssetDatabase.GetAssetPath(settings.prefabFolder);

            string outputFolder =
                AssetDatabase.GetAssetPath(settings.outputFolder);

            string[] guids =
                AssetDatabase.FindAssets(
                    "t:Prefab",
                    new[]
                    {
                        prefabFolder
                    });

            if (guids.Length == 0)
            {
                EditorUtility.DisplayDialog(
                    "Prefab Screenshot",
                    "No prefabs found.",
                    "OK");

                return;
            }

            if (!Directory.Exists(outputFolder))
                Directory.CreateDirectory(outputFolder);

            _previewScene =
                EditorSceneManager.NewPreviewScene();

            try
            {
                CreateEnvironment(settings);

                for (int i = 0; i < guids.Length; i++)
                {
                    float progress =
                        (float)i / guids.Length;

                    string assetPath =
                        AssetDatabase.GUIDToAssetPath(
                            guids[i]);

                    EditorUtility.DisplayProgressBar(
                        "Generating Screenshots",
                        Path.GetFileNameWithoutExtension(assetPath),
                        progress);

                    GameObject prefab =
                        AssetDatabase.LoadAssetAtPath<GameObject>(
                            assetPath);

                    if (prefab == null)
                        continue;

                    GameObject instance =
                        PrefabUtility.InstantiatePrefab(
                            prefab,
                            _previewScene) as GameObject;

                    if (instance == null)
                        continue;

                    try
                    {
                        RenderPrefab(
                            instance,
                            prefab.name,
                            outputFolder,
                            settings);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogException(e);
                    }
                    finally
                    {
                        if (instance != null)
                            Object.DestroyImmediate(instance);
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();

                DestroyEnvironment();

                EditorSceneManager.ClosePreviewScene(
                    _previewScene);

                AssetDatabase.Refresh();
            }

            Debug.Log(
                $"Generated {guids.Length} screenshots.");
        }

        /// <summary>
        /// Creates preview camera and light.
        /// </summary>
        private static void CreateEnvironment(
            ScreenshotSettings settings)
        {
            var cameraGO = new GameObject("Preview Camera");

            SceneManager.MoveGameObjectToScene(
                cameraGO,
                _previewScene);

            _camera = cameraGO.AddComponent<Camera>();

            _camera.scene = _previewScene;
            _camera.enabled = false;
            _camera.fieldOfView = 30f;
            _camera.nearClipPlane = 0.01f;
            _camera.farClipPlane = 500f;
            _camera.clearFlags = CameraClearFlags.SolidColor;

            _camera.backgroundColor =
                settings.transparentBackground
                    ? new Color(0, 0, 0, 0)
                    : Color.gray;

            var camData = cameraGO.AddComponent<UniversalAdditionalCameraData>();
            camData.renderType = CameraRenderType.Base;

            var lightGO = new GameObject("Directional Light");

            SceneManager.MoveGameObjectToScene(
                lightGO,
                _previewScene);

            _light = lightGO.AddComponent<Light>();

            _light.type = LightType.Directional;
            _light.intensity = settings.lightIntensity;
            _light.shadows = LightShadows.None;

            lightGO.transform.rotation = Quaternion.Euler(40, -35, 0);

            // FIX (dark screenshots): give the preview scene a soft ambient fill so
            // the shadow side of the object never falls to pure black. Preview
            // scenes have no skybox/ambient by default under URP.
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = AmbientFillColor;
        }

        /// <summary>
        /// Destroy temporary objects.
        /// </summary>
        private static void DestroyEnvironment()
        {
            if (_camera != null)
                Object.DestroyImmediate(_camera.gameObject);

            if (_light != null)
                Object.DestroyImmediate(_light.gameObject);
        }

        // =========
        // PART 2
        // =========

        /// <summary>
        /// Main render function per prefab instance.
        /// </summary>
        private static void RenderPrefab(
            GameObject instance,
            string fileName,
            string outputFolder,
            ScreenshotSettings settings)
        {
            var renderers = instance.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                Debug.LogWarning($"Prefab {fileName} has no renderers.");
                return;
            }

            Bounds bounds = CalculateBounds(renderers);
            bounds.Expand(bounds.size * (settings.padding - 1f));

            // Camera must be positioned before the light: the light's direction
            // is derived from the camera's final rotation (key-light-relative-to-view).
            SetupCamera(bounds, settings);
            SetupLight(bounds, settings);

            int size = settings.imageSize;

            var rt = new RenderTexture(
                size,
                size,
                24,
                RenderTextureFormat.ARGB32);

            rt.antiAliasing = settings.useMSAA ? settings.antiAliasing : 1;

            _camera.targetTexture = rt;

            SceneView.RepaintAll();
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            _camera.Render();

            RenderTexture.active = rt;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.ReadPixels(new Rect(0, 0, size, size), 0, 0);
            tex.Apply();

            _camera.targetTexture = null;
            RenderTexture.active = null;

            Object.DestroyImmediate(rt);

            if (settings.cropTransparentPixels)
                tex = CropTransparent(tex);

            SavePNG(tex, fileName, outputFolder);

            Object.DestroyImmediate(tex);
        }

        // ---------------- CAMERA ----------------

        private static void SetupCamera(Bounds bounds, ScreenshotSettings settings)
        {
            CameraRigMath.Apply(_camera, bounds, settings);
            _camera.backgroundColor = new Color(0, 0, 0, 0);
        }

        // ---------------- LIGHT ----------------

        /// <summary>
        /// FIX (dark / wrongly-lit screenshots): a Directional Light's position is
        /// meaningless — only its rotation matters. The previous implementation set
        /// `_light.transform.position` (a no-op for a directional light) and pointed
        /// it at a fixed world-space offset, so the lit side never followed the
        /// camera as cameraYaw/cameraPitch changed, leaving the visible side in shadow.
        /// Now the key light's rotation is derived from the camera's rotation, so the
        /// object's lit side always matches the side the camera is looking at.
        /// </summary>
        private static void SetupLight(Bounds bounds, ScreenshotSettings settings)
        {
            _light.intensity = settings.lightIntensity;

            Quaternion cameraRotation = _camera.transform.rotation;
            _light.transform.rotation = cameraRotation * KeyLightCameraOffset;
        }

        // ---------------- BOUNDS ----------------

        private static Bounds CalculateBounds(Renderer[] renderers)
        {
            Bounds b = renderers[0].bounds;

            for (int i = 1; i < renderers.Length; i++)
                b.Encapsulate(renderers[i].bounds);

            return b;
        }

        // ---------------- SAVE ----------------

        private static void SavePNG(Texture2D tex, string fileName, string folder)
        {
            byte[] png = tex.EncodeToPNG();

            string path = Path.Combine(folder, fileName + ".png");

            File.WriteAllBytes(path, png);

            Debug.Log($"Saved: {path}");
        }

        // ---------------- CROP ----------------

        private static Texture2D CropTransparent(Texture2D tex)
        {
            int width = tex.width;
            int height = tex.height;

            int minX = width, minY = height, maxX = 0, maxY = 0;

            Color32[] pixels = tex.GetPixels32();

            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                Color32 c = pixels[y * width + x];

                if (c.a > 5)
                {
                    if (x < minX) minX = x;
                    if (y < minY) minY = y;
                    if (x > maxX) maxX = x;
                    if (y > maxY) maxY = y;
                }
            }

            int w = maxX - minX;
            int h = maxY - minY;

            if (w <= 0 || h <= 0)
                return tex;

            Texture2D cropped = new Texture2D(w, h, TextureFormat.RGBA32, false);

            cropped.SetPixels(tex.GetPixels(minX, minY, w, h));
            cropped.Apply();

            Object.DestroyImmediate(tex);

            return cropped;
        }
    }
}