
using System.Collections;
using System.Collections.Generic;
using Galactic1.Code.Gameplay.Audio;
using Galactic1.Configs;
using Galactic1.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Code.Gameplay.Tasks.Presentation
{
    /// <summary>
    /// Persistent HUD-панель списка активных задач.
    ///
    /// Панель находится в Overlay Root и может отображаться поверх
    /// любых обычных экранов.
    ///
    /// Поведение:
    ///
    /// - когда обычные экраны закрыты — панель постоянно видима;
    /// - при открытии любого экрана панель скрывается;
    /// - при изменении задачи панель плавно появляется на несколько секунд,
    ///   даже если другой экран сейчас открыт;
    /// - каждое значимое изменение задачи перезапускает
    ///   activityVisibleDuration;
    /// - после окончания activityVisibleDuration панель скрывается,
    ///   если экран всё ещё открыт;
    /// - если обычных экранов нет, панель остаётся видимой.
    ///
    /// GameObject панели никогда не деактивируется через SetActive(false).
    /// Видимость управляется только через CanvasGroup.
    ///
    /// Важное правило:
    /// Panel не знает, сколько живёт Completed-задача.
    /// Она реагирует только на фактические изменения данных:
    ///
    /// OnTasksChanged
    /// OnTaskActivity
    ///
    /// Lifecycle Completed-задачи полностью принадлежит ScenarioTaskService.
    /// </summary>
    public sealed class ScenarioTaskPanel : UIScreenPanel
    {
        [Header("View")] 
        [SerializeField] private TMP_Text taskQuText;
        [SerializeField] private ScrollRect scrollRect;

        [SerializeField] private ScenarioTaskView viewPrefab;

        [Header("Visibility")] [SerializeField]
        private CanvasGroup canvasGroup;

        [SerializeField] private float showDuration = 0.3f;

        [SerializeField] private float hideDuration = 0.3f;

        /// <summary>
        /// Время временной видимости панели после последней activity.
        ///
        /// Это НЕ completion-delay задачи.
        /// </summary>
        [SerializeField] private float activityVisibleDuration = 5f;


        private ScenarioTaskPanelToggle _taskPanelToggle;
        private IScenarioTaskService _taskService;


        private readonly List<ScenarioTaskView> _pool = new();

        private EventBinding<UIScreenOpenedEvent> _screenOpenedBinding;
        private EventBinding<UIScreenClosedEvent> _screenClosedBinding;

        private Coroutine _visibilityCoroutine;
        private Coroutine _hideTimerCoroutine;

        private AudioCueData completeTaskAudio;

        private bool _isInitialized;

        /// <summary>
        /// Панель в данный момент находится в show/hide animation.
        /// </summary>
        private bool _isTransitioning;
        private bool _isHidingTransition;
        private bool _wasEmpty;

        /// <summary>
        /// Сейчас открыт обычный UIScreen.
        ///
        /// Название отражает смысл флага:
        /// когда false — HUD должен быть постоянно видим.
        /// когда true — HUD может быть временно показан только по activity.
        /// </summary>
        private bool _hasOpenScreen;

        /// <summary>
        /// Данные изменились во время transition.
        ///
        /// Refresh будет выполнен после завершения transition.
        /// </summary>
        private bool _refreshPending;







        public override void Initialize(DIContainer container, UIScreenId id)
        {
            base.Initialize(container, id);

            /*
             * GameObject панели никогда не выключается.
             * Управляем только CanvasGroup.
             */
            gameObject.SetActive(true);

            EnsureCanvasGroup();

            _taskService = ServiceLocator.Current.Get<IScenarioTaskService>();

            _taskPanelToggle = GetComponent<ScenarioTaskPanelToggle>();

            _taskService.OnTasksChanged += OnTasksChanged;
            _taskService.OnTaskActivity += OnTaskActivity;

            _screenOpenedBinding = new EventBinding<UIScreenOpenedEvent>(OnScreenOpened);

            _screenClosedBinding = new EventBinding<UIScreenClosedEvent>(OnScreenClosed);

            EventBus<UIScreenOpenedEvent>.Register(_screenOpenedBinding);

            EventBus<UIScreenClosedEvent>.Register(_screenClosedBinding);

            _isInitialized = true;


            completeTaskAudio = ServiceLocator.Current.Get<ConfigProvider>()
                .Get<UIAudioDatabase>()
                .Get<SimpleAudioConfig>("audio_cue_complete")
                .ToData();

            /*
             * По умолчанию HUD-панель видима.
             *
             * Если на момент Initialize уже открыт экран,
             * соответствующий UIScreenOpenedEvent приведёт её к hidden state.
             */
            ShowImmediate();

            Refresh();
        }

        public override void Remove()
        {
            StopVisibilityCoroutine();
            StopHideTimer();

            if (_taskService != null)
            {
                _taskService.OnTasksChanged -= OnTasksChanged;
                _taskService.OnTaskActivity -= OnTaskActivity;
            }

            if (_screenOpenedBinding != null)
            {
                EventBus<UIScreenOpenedEvent>
                    .Deregister(_screenOpenedBinding);

                _screenOpenedBinding = null;
            }

            if (_screenClosedBinding != null)
            {
                EventBus<UIScreenClosedEvent>
                    .Deregister(_screenClosedBinding);

                _screenClosedBinding = null;
            }

            _taskService = null;
            _isInitialized = false;

            base.Remove();
        }

        private void EnsureCanvasGroup()
        {
            if (canvasGroup != null)
                return;

            canvasGroup = GetComponent<CanvasGroup>();

            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        /// <summary>
        /// Фактическое изменение данных задач.
        ///
        /// Здесь только обновляем содержимое.
        /// Решение о временной видимости принимает OnTaskActivity.
        /// </summary>
        private void OnTasksChanged()
        {
            if (!_isInitialized)
                return;

            /*
             * Если сейчас идёт transition,
             * не трогаем карточки посреди animation.
             *
             * Последнее состояние будет прочитано после transition.
             */
            if (_isTransitioning)
            {
                _refreshPending = true;
                return;
            }

            Refresh();
        }

        /// <summary>
        /// Значимая activity задачи:
        ///
        /// - создание;
        /// - обновление progress;
        /// - completion;
        /// - появление новой задачи после удаления Completed.
        ///
        /// RemoveTask это событие НЕ вызывает.
        /// </summary>
        private void OnTaskActivity()
        {
            if (!_isInitialized)
                return;

            _refreshPending = true;

            StopHideTimer();

            if (IsFullyVisible())
            {
                Refresh();
                _refreshPending = false;

                StartHideTimer();
                return;
            }

            /*
             * Если уже идёт SHOW — вторую ShowRoutine не запускаем,
             * текущая сама применит Refresh и таймер по завершении.
             *
             * Если идёт HIDE — не выходим молча (как раньше),
             * а прерываем скрытие и запускаем показ.
             */
            if (_isTransitioning && !_isHidingTransition)
                return;

            StartShowAnimation();
        }

        private void OnScreenOpened(UIScreenOpenedEvent evt)
        {
            _hasOpenScreen = true;

            StopVisibilityCoroutine();
            StopHideTimer();

            HideImmediate();
        }

        private void OnScreenClosed(UIScreenClosedEvent evt)
        {
            if (!evt.AllScreensClosed) return;

            _hasOpenScreen = false;

            /*
             * Если сейчас идёт activity SHOW — не прерываем его.
             * ShowRoutine по завершении сам проверит _hasOpenScreen (уже false)
             * и не запустит hide-таймер, панель останется полностью видимой.
             *
             * Но если сейчас идёт HIDE (панель гасла из-за истёкшего
             * activityVisibleDuration, пока экран ещё был открыт) —
             * его нужно прервать: экраны уже закрыты, HUD обязан быть виден.
             */
            if (_isTransitioning && !_isHidingTransition)
                return;

            StopHideTimer();

            ShowImmediate();

            if (_refreshPending)
            {
                Refresh();
                _refreshPending = false;
            }
        }

        private void StartShowAnimation()
        {
            StopVisibilityCoroutine();
            StopHideTimer();

            _visibilityCoroutine = StartCoroutine(ShowRoutine());
        }

        private IEnumerator ShowRoutine()
        {
            _taskPanelToggle.ForceShowPanel(false);
            _isTransitioning = true;
            _isHidingTransition = false;

            float duration = Mathf.Max(0f, showDuration);
            float startAlpha = canvasGroup.alpha;

            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            if (duration <= 0f)
            {
                canvasGroup.alpha = 1f;
            }
            else
            {
                float elapsed = 0f;

                while (elapsed < duration)
                {
                    elapsed += Time.unscaledDeltaTime;

                    float normalizedTime = Mathf.Clamp01(elapsed / duration);

                    canvasGroup.alpha =
                        Mathf.Lerp(
                            startAlpha,
                            1f,
                            normalizedTime);

                    yield return null;
                }

                canvasGroup.alpha = 1f;
            }

            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            _isTransitioning = false;
            _visibilityCoroutine = null;

            /*
             * Пока шла animation, могли произойти:
             *
             * Complete
             * Remove
             * Add next task
             *
             * Поэтому здесь читаем актуальное состояние сервиса,
             * а не пытаемся воспроизвести историю событий.
             */
            if (_refreshPending)
            {
                Refresh();
                _refreshPending = false;
            }

            /*
             * Если обычный экран закрыт,
             * панель должна остаться постоянно видимой.
             *
             * Если экран открыт — запускаем временный activity timer.
             */
            if (_hasOpenScreen)
                StartHideTimer();
        }

        private void StartHideTimer()
        {
            StopHideTimer();

            _hideTimerCoroutine = StartCoroutine(HideAfterDelayRoutine());
        }

        /// <summary>
        /// Ждёт только activityVisibleDuration.
        ///
        /// Completion-delay здесь намеренно отсутствует.
        /// ScenarioTaskService гарантирует, что к моменту окончания
        /// activity timer данные уже будут приведены к актуальному состоянию
        /// через OnTasksChanged / OnTaskActivity.
        /// </summary>
        private IEnumerator HideAfterDelayRoutine()
        {
            float duration = Mathf.Max(0f, activityVisibleDuration);

            if (duration > 0f)
                yield return new WaitForSecondsRealtime(duration);

            _hideTimerCoroutine = null;

            /*
             * За время ожидания состояние могло измениться.
             *
             * Если экран закрыли — панель должна остаться видимой.
             */
            if (_isTransitioning || !_hasOpenScreen)
                yield break;

            /*
             * Скрываем только если activity timer действительно
             * дошёл до конца без нового OnTaskActivity.
             *
             * Новая activity уже остановила старую coroutine
             * через StopHideTimer().
             */
            _visibilityCoroutine = StartCoroutine(HideRoutine());
        }

        private IEnumerator HideRoutine()
        {
            _isTransitioning = true;
            _isHidingTransition = true;

            float duration = Mathf.Max(0f, hideDuration);
            float startAlpha = canvasGroup.alpha;

            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            if (duration <= 0f)
            {
                canvasGroup.alpha = 0f;
            }
            else
            {
                float elapsed = 0f;

                while (elapsed < duration)
                {
                    elapsed += Time.unscaledDeltaTime;

                    float normalizedTime = Mathf.Clamp01(elapsed / duration);

                    canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, normalizedTime);

                    yield return null;
                }

                canvasGroup.alpha = 0f;
            }

            _isTransitioning = false;
            _visibilityCoroutine = null;
        }

        private void StopVisibilityCoroutine()
        {
            if (_visibilityCoroutine == null)
                return;

            StopCoroutine(_visibilityCoroutine);

            _visibilityCoroutine = null;
            _isTransitioning = false;
            _isHidingTransition = false;
        }

        private void StopHideTimer()
        {
            if (_hideTimerCoroutine == null)
                return;

            StopCoroutine(_hideTimerCoroutine);

            _hideTimerCoroutine = null;
        }

        private bool IsFullyVisible()
        {
            return !_isTransitioning &&
                   canvasGroup.alpha >= 0.999f;
        }

        private void ShowImmediate()
        {
            StopVisibilityCoroutine();

            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            _taskPanelToggle.RootButtons(true);

        }

        private void HideImmediate()
        {
            StopVisibilityCoroutine();

            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            _taskPanelToggle.RootButtons(false);
        }

        private void Refresh()
        {
            if (_taskService == null)
                return;

            var tasks = _taskService.GetTasks();
            
            taskQuText.text = tasks.Count.ToString();
            
            bool isEmpty = tasks.Count == 0;

            if (isEmpty && !_wasEmpty)
                _taskPanelToggle.ForceHidePanel(!_hasOpenScreen);

            if (!isEmpty && _wasEmpty)
                _taskPanelToggle.ForceShowPanel(true);

            _wasEmpty = isEmpty;

            EnsurePoolSize(tasks.Count);

            for (int i = 0; i < _pool.Count; i++)
            {
                bool shouldBeActive = i < tasks.Count;

                if (_pool[i].gameObject.activeSelf != shouldBeActive)
                    _pool[i].gameObject.SetActive(shouldBeActive);
            }

            for (int i = 0; i < tasks.Count; i++)
                _pool[i].Bind(tasks[i], completeTaskAudio);

            scrollRect.SetSizeContentLayoutGroup(
                true,
                null,
                true,
                true);
        }

        private void EnsurePoolSize(int count)
        {
            while (_pool.Count < count)
                _pool.Add(Instantiate(viewPrefab, scrollRect.content));
        }
    }
}