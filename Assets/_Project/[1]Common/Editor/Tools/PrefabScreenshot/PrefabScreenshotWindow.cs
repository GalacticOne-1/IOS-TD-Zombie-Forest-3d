using UnityEditor;
using UnityEngine;
using System.Linq;

namespace Galactic1.EditorTools.PrefabScreenshot
{
    public class PrefabScreenshotWindow : EditorWindow
    {
        private ScreenshotSettings settings;

        private PreviewRenderUtility _previewUtility;
        private GameObject _previewInstance;
        private GameObject[] _prefabs;
        private string[] _prefabNames;
        private int _selectedPrefabIndex;

        private Vector2 _settingsScroll;

        [MenuItem("Tools/Prefab Screenshot Generator")]
        public static void Open()
        {
            GetWindow<PrefabScreenshotWindow>("Prefab Screenshots");
        }

        private void OnEnable()
        {
            settings ??= new ScreenshotSettings();

            _previewUtility = new PreviewRenderUtility();
            _previewUtility.cameraFieldOfView = settings.fieldOfView;
            _previewUtility.camera.clearFlags = CameraClearFlags.SolidColor;
            _previewUtility.camera.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f);
            _previewUtility.lights[0].intensity = settings.lightIntensity;
            _previewUtility.lights[0].transform.rotation = Quaternion.Euler(40, -35, 0);

            RefreshPrefabList();
        }

        private void OnDisable()
        {
            if (_previewInstance != null)
                DestroyImmediate(_previewInstance);

            _previewUtility?.Cleanup();
            _previewUtility = null;
        }

        // ---------------- PREFAB LIST ----------------

        private void RefreshPrefabList()
        {
            if (settings.prefabFolder == null)
            {
                _prefabs = null;
                _prefabNames = null;
                return;
            }

            string folder = AssetDatabase.GetAssetPath(settings.prefabFolder);

            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { folder });

            _prefabs = guids
                .Select(g => AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(p => p != null)
                .ToArray();

            _prefabNames = _prefabs.Select(p => p.name).ToArray();

            _selectedPrefabIndex = 0;

            UpdatePreviewInstance();
        }

        private void UpdatePreviewInstance()
        {
            if (_previewInstance != null)
                DestroyImmediate(_previewInstance);

            if (_prefabs == null || _prefabs.Length == 0)
                return;

            var prefab = _prefabs[_selectedPrefabIndex];

            _previewInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            _previewUtility.AddSingleGO(_previewInstance);
        }

        // ---------------- GUI ----------------

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();

            // ---- LEFT: настройки ----
            EditorGUILayout.BeginVertical(GUILayout.Width(position.width * 0.55f));

            DrawSettingsPanel();

            EditorGUILayout.EndVertical();

            // ---- RIGHT: превью ----
            EditorGUILayout.BeginVertical();

            DrawPreviewPanel();

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawSettingsPanel()
        {
            _settingsScroll = EditorGUILayout.BeginScrollView(_settingsScroll);

            GUILayout.Space(8);

            EditorGUILayout.LabelField(
                "Prefab Screenshot Generator",
                EditorStyles.boldLabel);

            GUILayout.Space(5);

            EditorGUI.BeginChangeCheck();

            settings.prefabFolder =
                (DefaultAsset)EditorGUILayout.ObjectField(
                    "Prefab Folder",
                    settings.prefabFolder,
                    typeof(DefaultAsset),
                    false);

            if (EditorGUI.EndChangeCheck())
                RefreshPrefabList();

            settings.outputFolder =
                (DefaultAsset)EditorGUILayout.ObjectField(
                    "Output Folder",
                    settings.outputFolder,
                    typeof(DefaultAsset),
                    false);

            GUILayout.Space(5);

            if (_prefabNames != null && _prefabNames.Length > 0)
            {
                EditorGUI.BeginChangeCheck();

                _selectedPrefabIndex =
                    EditorGUILayout.Popup(
                        "Preview Prefab",
                        _selectedPrefabIndex,
                        _prefabNames);

                if (EditorGUI.EndChangeCheck())
                    UpdatePreviewInstance();
            }

            settings.imageSize =
                EditorGUILayout.IntPopup(
                    "Image Size",
                    settings.imageSize,
                    new[] { "256", "512", "1024", "2048" },
                    new[] { 256, 512, 1024, 2048 });

            GUILayout.Space(10);

            EditorGUILayout.LabelField("Camera", EditorStyles.boldLabel);
            

            settings.cameraPitch =
                EditorGUILayout.Slider("Pitch", settings.cameraPitch, -90, 90);

            settings.cameraYaw =
                EditorGUILayout.Slider("Yaw", settings.cameraYaw, -180, 180);

            settings.cameraRoll =
                EditorGUILayout.Slider("Roll", settings.cameraRoll, -180, 180);

            settings.fieldOfView =
                EditorGUILayout.Slider("FOV", settings.fieldOfView, 10f, 90f);

            settings.cameraDistanceMultiplier =
                EditorGUILayout.Slider("Zoom (distance)", settings.cameraDistanceMultiplier, 0.5f, 6f);

            settings.padding =
                EditorGUILayout.Slider("Padding", settings.padding, 1.0f, 2.0f);

            
            EditorGUILayout.LabelField("Target Offset (norm.)");
            settings.targetOffsetNormalized = EditorGUILayout.Vector3Field(GUIContent.none, settings.targetOffsetNormalized);
            settings.targetOffsetNormalized = ClampNormalized(settings.targetOffsetNormalized);

            EditorGUILayout.LabelField("Position Offset (norm.)");
            settings.positionOffsetNormalized = EditorGUILayout.Vector3Field(GUIContent.none, settings.positionOffsetNormalized);
            settings.positionOffsetNormalized = ClampNormalized(settings.positionOffsetNormalized);

            GUILayout.Space(10);

            EditorGUILayout.LabelField("Rendering", EditorStyles.boldLabel);

            settings.transparentBackground =
                EditorGUILayout.Toggle("Transparent", settings.transparentBackground);

            settings.cropTransparentPixels =
                EditorGUILayout.Toggle("Auto Crop", settings.cropTransparentPixels);

            settings.useMSAA =
                EditorGUILayout.Toggle("MSAA", settings.useMSAA);

            EditorGUI.BeginDisabledGroup(!settings.useMSAA);

            settings.antiAliasing =
                EditorGUILayout.IntPopup(
                    "AA",
                    settings.antiAliasing,
                    new[] { "1", "2", "4", "8" },
                    new[] { 1, 2, 4, 8 });

            EditorGUI.EndDisabledGroup();

            settings.lightIntensity =
                EditorGUILayout.Slider("Light", settings.lightIntensity, 0.2f, 3f);

            GUILayout.Space(20);

            GUI.backgroundColor = Color.green;

            if (GUILayout.Button("Generate Screenshots", GUILayout.Height(40)))
            {
                PrefabScreenshotGenerator.Generate(settings);
            }

            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndScrollView();
        }

        private void DrawPreviewPanel()
        {
            GUILayout.Space(8);

            EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);

            Rect previewRect =
                GUILayoutUtility.GetRect(
                    100, 4000,
                    100, 4000,
                    GUILayout.ExpandWidth(true),
                    GUILayout.ExpandHeight(true));

            DrawPreview(previewRect);
        }

        // ---------------- PREVIEW RENDERING ----------------

        private void DrawPreview(Rect rect)
        {
            if (_previewUtility == null || _previewInstance == null)
            {
                EditorGUI.HelpBox(rect, "Select a prefab folder to preview.", MessageType.Info);
                return;
            }

            _previewUtility.BeginPreview(rect, GUIStyle.none);

            ApplyPreviewCamera();

            _previewUtility.lights[0].intensity = settings.lightIntensity;

            _previewUtility.Render();

            Texture resultRender = _previewUtility.EndPreview();

            GUI.DrawTexture(rect, resultRender, ScaleMode.StretchToFill, false);

            Repaint();
        }
        
        private static Vector3 ClampNormalized(Vector3 v)
        {
            return new Vector3(
                Mathf.Clamp(v.x, -2f, 2f),
                Mathf.Clamp(v.y, -2f, 2f),
                Mathf.Clamp(v.z, -2f, 2f));
        }

        private void ApplyPreviewCamera()
        {
            var renderers = _previewInstance.GetComponentsInChildren<Renderer>();

            if (renderers.Length == 0)
                return;

            Bounds bounds = renderers[0].bounds;

            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);

            CameraRigMath.Apply(_previewUtility.camera, bounds, settings);
        }
    }
}