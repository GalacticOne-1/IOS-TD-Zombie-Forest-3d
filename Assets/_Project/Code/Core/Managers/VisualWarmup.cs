using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Galactic1.Code.Warmup
{
    /// <summary>
    /// Вешается на объект в Warmup-сцене. Спавнит по одному экземпляру каждого префаба в поле
    /// зрения камеры, даёт им отрендериться несколько кадров, затем всё удаляет.
    /// Цель: чтобы первый показ материалов/шейдеров/частиц случился здесь, а не в геймплее.
    /// Не гарантирует прогрев абсолютно всех PSO/вариантов (зависит от девайса и API).
    /// </summary>
    public class VisualWarmup : MonoBehaviour
    {
        [Header("Scene")]
        [Tooltip("Камера, которая рендерит прогрев. Должна совпадать по настройкам рендера с игровой камерой " +
                 "(тот же URP Asset/Renderer, MSAA, HDR), иначе будут прогреты другие pipeline state. " +
                 "Если пусто - берётся Camera.main.")]
        [SerializeField]
        private Camera warmupCamera;

        [Header("What to warm up")]
        [Tooltip("Зомби (все варианты с разными материалами), контейнеры, взрывы, кровь и т.д.")]
        [SerializeField]
        private List<GameObject> prefabs = new List<GameObject>();

        [Tooltip("Необязательно: коллекция вариантов шейдеров для WarmUp().")] [SerializeField]
        private ShaderVariantCollection shaderVariants;

        [Header("Behaviour")] [SerializeField] private bool autoStart = true;

        [Tooltip("Сколько объектов инстанцировать за кадр (чтобы не было спайка от самого прогрева).")]
        [SerializeField, Min(1)]
        private int spawnPerFrame = 2;

        [Tooltip("Сколько кадров рендерить объекты после спавна последнего.")] [SerializeField, Min(1)]
        private int framesToRender = 3;

        [Tooltip("Дополнительное время (сек, реальное) для частиц: чтобы они успели заэмититься и отрисоваться.")]
        [SerializeField, Min(0f)]
        private float particleSettleSeconds = 0.6f;

        [Header("Placement")]
        [Tooltip("Расстояние от камеры вдоль её оси, на котором размещаются объекты.")]
        [SerializeField, Min(0.1f)]
        private float distanceFromCamera = 10f;

        [SerializeField, Min(1)] private int columns = 4;

        [Header("Safety")]
        [Tooltip("Удалять MonoBehaviour-скрипты с временных копий до активации, чтобы не запускать геймплейную " +
                 "логику (Awake/OnEnable, ServiceLocator, AI). Выключите, если визуал зависит от скриптов " +
                 "(например, TextMeshPro в мире).")]
        [SerializeField]
        private bool stripScripts = true;

        [SerializeField] private bool log = true;

        public bool IsDone { get; private set; }
        public bool Success { get; private set; }
        public event Action<bool> Completed;

        private Coroutine _routine;
        private GameObject _inactiveRoot;
        private GameObject _activeRoot;
        private bool _finished;

        private void Start()
        {
            if (autoStart)
                Begin();
        }

        /// <summary>Запускает прогрев. Повторные вызовы игнорируются.</summary>
        public void Begin()
        {
            if (_routine != null || IsDone)
                return;
            _routine = StartCoroutine(Execute());
        }

        /// <summary>Для менеджера сцен: yield return warmup.Run(); Безопасно вызывать многократно.</summary>
        public IEnumerator Run()
        {
            Begin();
            while (!IsDone)
                yield return null;
        }

        private IEnumerator Execute()
        {
            float startTime = Time.realtimeSinceStartup;
            bool ok = false;
            int spawned = 0;
            int failed = 0;

            try
            {
                var cam = warmupCamera != null ? warmupCamera : Camera.main;
                if (cam == null)
                {
                    Debug.LogError("[VisualWarmup] Camera not found. Assign warmupCamera.", this);
                    yield break;
                }

                if (shaderVariants != null && !shaderVariants.isWarmedUp)
                    shaderVariants.WarmUp();

                var valid = new List<GameObject>(prefabs.Count);
                foreach (var p in prefabs)
                {
                    if (p != null) valid.Add(p);
                    else failed++;
                }

                if (failed > 0)
                    Debug.LogWarning($"[VisualWarmup] {failed} prefab slot(s) are empty.", this);

                // Один кадр, чтобы шейдерный WarmUp не слипался со спавном.
                yield return null;

                _inactiveRoot = new GameObject("[VisualWarmup] inactive") { hideFlags = HideFlags.DontSave };
                _inactiveRoot.SetActive(false);
                _activeRoot = new GameObject("[VisualWarmup] active") { hideFlags = HideFlags.DontSave };

                int rows = Mathf.Max(1, Mathf.CeilToInt(valid.Count / (float)columns));
                int spawnedThisFrame = 0;

                for (int i = 0; i < valid.Count; i++)
                {
                    int col = i % columns;
                    int row = i / columns;
                    float vx = Mathf.Lerp(0.1f, 0.9f, (col + 0.5f) / columns);
                    float vy = Mathf.Lerp(0.1f, 0.9f, (row + 0.5f) / rows);
                    Vector3 pos = cam.ViewportToWorldPoint(new Vector3(vx, vy, distanceFromCamera));

                    if (TrySpawn(valid[i], pos, cam.transform.rotation))
                        spawned++;
                    else
                        failed++;

                    if (++spawnedThisFrame >= spawnPerFrame)
                    {
                        spawnedThisFrame = 0;
                        yield return null;
                    }
                }

                // Даём отрендериться через реальные границы кадров.
                for (int f = 0; f < framesToRender; f++)
                    yield return null;

                // Частицы/аниматоры: ждём реальное время, пока не истечёт settle или таймаут.
                float settleEnd = Time.realtimeSinceStartup + particleSettleSeconds;
                while (Time.realtimeSinceStartup < settleEnd)
                    yield return null;

                ok = true;
            }
            finally
            {
                Finish(ok);
                if (log)
                {
                    float dt = Time.realtimeSinceStartup - startTime;
                    Debug.Log(
                        $"[VisualWarmup] configured visual warmup {(ok ? "completed" : "ended with problems")}: " +
                        $"spawned={spawned}, failed={failed}, time={dt:F2}s");
                }
            }
        }

        private bool TrySpawn(GameObject prefab, Vector3 worldPos, Quaternion rot)
        {
            GameObject instance = null;
            try
            {
                // Инстанс создаётся под неактивным родителем: Awake/OnEnable не вызываются,
                // пока мы не вычистим скрипты.
                instance = Instantiate(prefab, _inactiveRoot.transform);
                instance.transform.SetPositionAndRotation(worldPos, rot);

                if (stripScripts)
                {
                    var behaviours = instance.GetComponentsInChildren<MonoBehaviour>(true);
                    for (int i = behaviours.Length - 1; i >= 0; i--)
                        if (behaviours[i] != null)
                            DestroyImmediate(behaviours[i]);
                }

                foreach (var animator in instance.GetComponentsInChildren<Animator>(true))
                    animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

                instance.transform.SetParent(_activeRoot.transform, true);
                instance.SetActive(true);

                foreach (var ps in instance.GetComponentsInChildren<ParticleSystem>(true))
                    ps.Play(true);

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[VisualWarmup] Failed to warm up '{prefab.name}': {e}", this);
                if (instance != null)
                    Destroy(instance);
                return false;
            }
        }

        private void Finish(bool ok)
        {
            if (_finished)
                return;
            _finished = true;

            Cleanup();
            Success = ok;
            IsDone = true;
            Completed?.Invoke(ok);
        }

        private void Cleanup()
        {
            if (_activeRoot != null) Destroy(_activeRoot);
            if (_inactiveRoot != null) Destroy(_inactiveRoot);
            _activeRoot = null;
            _inactiveRoot = null;
        }

        private void OnDestroy()
        {
            // Сцена выгружается посреди прогрева: finally в корутине может не выполниться.
            Finish(false);
        }
    }
}