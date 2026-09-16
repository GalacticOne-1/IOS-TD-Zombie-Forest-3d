using System.Collections;
using System.Collections.Generic;
using Galactic1.Code.Gameplay.Audio;
using Galactic1.Configs;
using Galactic1.UI.Core;
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
    ///
    /// Panel не знает, сколько живёт Completed-задача.
    /// Она реагирует только на фактические изменения данных:
    ///
    /// OnTasksChanged
    /// OnTaskActivity
    ///
    /// Lifecycle Completed-задачи полностью принадлежит
    /// ScenarioTaskService.
    ///
    /// Visibility state machine:
    ///
    /// None
    /// Showing
    /// Hiding
    ///
    /// Любой новый Show или Hide сначала останавливает
    /// предыдущий visibility transition.
    /// </summary>
    public sealed class ScenarioTaskPanel : UIScreenPanel
    {
        [Header("View")] [SerializeField] private ScrollRect scrollRect;

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


        /// <summary>
        /// Coroutine текущего Show/Hide transition.
        ///
        /// Используется только для visibility animation.
        /// </summary>
        private Coroutine _visibilityCoroutine;

        /// <summary>
        /// Coroutine временного activity timer.
        /// </summary>
        private Coroutine _hideTimerCoroutine;


        private AudioCueData completeTaskAudio;


        private bool _isInitialized;


        /// <summary>
        /// Сейчас открыт обычный UIScreen.
        ///
        /// false:
        /// HUD должен быть постоянно видим.
        ///
        /// true:
        /// HUD может быть временно показан только по activity.
        /// </summary>
        private bool _hasOpenScreen;


        /// <summary>
        /// Данные изменились во время transition.
        ///
        /// Refresh будет выполнен после завершения transition.
        /// </summary>
        private bool _refreshPending;


        /// <summary>
        /// Текущее состояние visibility transition.
        ///
        /// None:
        /// Нет активной анимации.
        ///
        /// Showing:
        /// Панель появляется.
        ///
        /// Hiding:
        /// Панель скрывается.
        /// </summary>
        private EVisibilityTransition _visibilityTransition;


        private enum EVisibilityTransition
        {
            None,
            Showing,
            Hiding
        }


        #region Lifecycle


        public override void Initialize(DIContainer container, UIScreenId id)
        {
            base.Initialize(container, id);

            /*
             * GameObject панели никогда не выключается.
             * Управляем только CanvasGroup.
             */
            gameObject.SetActive(true);

            EnsureCanvasGroup();

            _taskService = ServiceLocator.Current
                .Get<IScenarioTaskService>();

            _taskPanelToggle = GetComponent<ScenarioTaskPanelToggle>();


            _taskService.OnTasksChanged += OnTasksChanged;

            _taskService.OnTaskActivity += OnTaskActivity;


            _screenOpenedBinding =
                new EventBinding<UIScreenOpenedEvent>(
                    OnScreenOpened);

            _screenClosedBinding =
                new EventBinding<UIScreenClosedEvent>(
                    OnScreenClosed);


            EventBus<UIScreenOpenedEvent>
                .Register(_screenOpenedBinding);

            EventBus<UIScreenClosedEvent>
                .Register(_screenClosedBinding);


            completeTaskAudio = ServiceLocator.Current
                .Get<ConfigProvider>()
                .Get<UIAudioDatabase>()
                .Get<SimpleAudioConfig>("audio_cue_complete")
                .ToData();


            _isInitialized = true;


            /*
             * По умолчанию HUD-панель видима.
             *
             * Если на момент Initialize уже открыт экран,
             * соответствующий UIScreenOpenedEvent приведёт
             * её к hidden state.
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


        #endregion


        #region Initialization


        private void EnsureCanvasGroup()
        {
            if (canvasGroup != null)
                return;


            canvasGroup = GetComponent<CanvasGroup>();


            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        
        private bool HasTasks()
        {
            if (_taskService == null)
                return false;

            return _taskService.GetTasks().Count > 0;
        }

        #endregion


        #region Task Events


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

            if (!HasTasks())
            {
                StopVisibilityCoroutine();
                StopHideTimer();

                _refreshPending = false;

                _taskPanelToggle.ForceHidePanel();

                Refresh();

                return;
            }

            /*
             * Если сейчас идёт visibility transition,
             * не трогаем карточки посреди animation.
             */
            if (IsVisibilityTransitionRunning())
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

            /*
             * Любая новая activity полностью перезапускает
             * activityVisibleDuration.
             */
            StopHideTimer();


            /*
             * Если список задач пуст,
             * панель должна быть свёрнута.
             */
            if (!HasTasks())
            {
                StopVisibilityCoroutine();

                _refreshPending = false;

                _taskPanelToggle.ForceHidePanel();

                Refresh();

                return;
            }


            /*
             * Если обычные экраны закрыты,
             * HUD должен оставаться постоянно видимым.
             */
            if (!_hasOpenScreen)
            {
                StopVisibilityCoroutine();

                _taskPanelToggle.ForceShowPanel();

                ShowImmediate();

                Refresh();

                _refreshPending = false;

                return;
            }


            /*
             * Если панель сейчас скрывается,
             * необходимо прервать HideRoutine.
             */
            if (_visibilityTransition == EVisibilityTransition.Hiding)
            {
                StopVisibilityCoroutine();

                StartShowAnimation();

                return;
            }


            /*
             * Если панель уже полностью видима,
             * повторно проигрывать fade-in не нужно.
             */
            if (IsFullyVisible())
            {
                Refresh();

                _refreshPending = false;

                StartHideTimer();

                return;
            }


            /*
             * Если панель уже появляется,
             * вторую ShowRoutine запускать не нужно.
             */
            if (_visibilityTransition == EVisibilityTransition.Showing)
                return;


            StartShowAnimation();
        }


        #endregion


        #region Screen Events


        private void OnScreenOpened(UIScreenOpenedEvent evt)
        {
            _hasOpenScreen = true;

            StopVisibilityCoroutine();
            StopHideTimer();

            HideImmediate();
        }


        private void OnScreenClosed(UIScreenClosedEvent evt)
        {
            if (!evt.AllScreensClosed)
                return;

            _hasOpenScreen = false;


            /*
             * Если список задач пуст,
             * панель должна остаться свёрнутой.
             */
            if (!HasTasks())
            {
                StopVisibilityCoroutine();
                StopHideTimer();

                _refreshPending = false;

                _taskPanelToggle.ForceHidePanel();

                HideImmediate();

                Refresh();

                return;
            }


            /*
             * При закрытии всех обычных экранов
             * HUD-панель должна стать постоянно видимой.
             *
             * Неважно, какая transition сейчас выполняется:
             *
             * - ShowRoutine;
             * - HideRoutine.
             *
             * Текущую animation необходимо прервать.
             */
            StopVisibilityCoroutine();
            StopHideTimer();


            /*
             * Без открытого обычного экрана HUD должен быть
             * постоянно видим.
             */
            //_taskPanelToggle.ForceShowPanel();

            ShowImmediate();

            if (_refreshPending)
            {
                Refresh();

                _refreshPending = false;
            }
        }


        #endregion


        #region Show / Hide Animation


        /// <summary>
        /// Запускает fade-in панели.
        ///
        /// Любой предыдущий visibility transition
        /// сначала останавливается.
        /// </summary>
        private void StartShowAnimation()
        {
            StopVisibilityCoroutine();

            StopHideTimer();


            _visibilityCoroutine =
                StartCoroutine(ShowRoutine());
        }


        private IEnumerator ShowRoutine()
        {
            /*
             * Если во время ожидания/запуска ShowRoutine
             * список задач стал пустым,
             * панель не должна показываться.
             */
            if (!HasTasks())
            {
                _visibilityTransition =
                    EVisibilityTransition.None;

                _visibilityCoroutine = null;

                _taskPanelToggle.ForceHidePanel();

                HideImmediate();

                yield break;
            }
            
            _visibilityTransition =
                EVisibilityTransition.Showing;


            _taskPanelToggle.ForceShowPanel();


            float duration =
                Mathf.Max(0f, showDuration);


            float startAlpha =
                canvasGroup.alpha;


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


                    float normalizedTime =
                        Mathf.Clamp01(elapsed / duration);


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


            /*
             * Transition завершён.
             */
            _visibilityTransition =
                EVisibilityTransition.None;

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
             * Если экран открыт —
             * запускаем временный activity timer.
             */
            if (_hasOpenScreen)
            {
                StartHideTimer();
            }
        }


        /// <summary>
        /// Запускает временный activity timer.
        ///
        /// После его окончания панель скрывается,
        /// если обычный экран всё ещё открыт.
        /// </summary>
        private void StartHideTimer()
        {
            StopHideTimer();

            _hideTimerCoroutine = StartCoroutine(HideAfterDelayRoutine());
        }


        /// <summary>
        /// Ждёт только activityVisibleDuration.
        ///
        /// Completion-delay здесь намеренно отсутствует.
        /// ScenarioTaskService отвечает за lifecycle задач.
        /// </summary>
        private IEnumerator HideAfterDelayRoutine()
        {
            float duration = Mathf.Max(0f, activityVisibleDuration);


            if (duration > 0f)
            {
                yield return new WaitForSecondsRealtime(duration);
            }


            _hideTimerCoroutine = null;


            /*
             * Если список задач пуст,
             * панель должна быть свёрнута.
             */
            if (!HasTasks())
            {
                StopVisibilityCoroutine();

                _taskPanelToggle.ForceHidePanel();

                yield break;
            }


            /*
             * Если экран закрыли —
             * панель должна остаться видимой.
             */
            if (!_hasOpenScreen)
                yield break;


            /*
             * Если за время ожидания начался другой transition,
             * не запускаем второй HideRoutine.
             */
            if (IsVisibilityTransitionRunning())
                yield break;


            /*
             * Скрываем только если activity timer
             * действительно дошёл до конца.
             */
            _visibilityCoroutine =
                StartCoroutine(HideRoutine());
        }


        private IEnumerator HideRoutine()
        {
            /*
             * Если список задач пуст,
             * панель должна быть свёрнута через Toggle.
             */
            if (!HasTasks())
            {
                _visibilityTransition =
                    EVisibilityTransition.None;

                _visibilityCoroutine = null;

                _taskPanelToggle.ForceHidePanel();

                yield break;
            }
            
            
            _visibilityTransition =
                EVisibilityTransition.Hiding;


            float duration =
                Mathf.Max(0f, hideDuration);


            float startAlpha =
                canvasGroup.alpha;


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


                    float normalizedTime =
                        Mathf.Clamp01(elapsed / duration);


                    canvasGroup.alpha =
                        Mathf.Lerp(
                            startAlpha,
                            0f,
                            normalizedTime);


                    yield return null;
                }


                canvasGroup.alpha = 0f;
            }


            _visibilityTransition =
                EVisibilityTransition.None;

            _visibilityCoroutine = null;
        }


        #endregion


        #region Coroutine Control


        /// <summary>
        /// Останавливает текущую Show/Hide animation.
        ///
        /// Важно:
        /// StopCoroutine предотвращает выполнение
        /// оставшейся части IEnumerator.
        ///
        /// Это защищает от ситуации, когда старая HideRoutine
        /// после нового Show записывает alpha = 0.
        /// </summary>
        private void StopVisibilityCoroutine()
        {
            if (_visibilityCoroutine != null)
            {
                StopCoroutine(_visibilityCoroutine);

                _visibilityCoroutine = null;
            }


            _visibilityTransition =
                EVisibilityTransition.None;
        }


        private void StopHideTimer()
        {
            if (_hideTimerCoroutine == null)
                return;


            StopCoroutine(_hideTimerCoroutine);

            _hideTimerCoroutine = null;
        }


        private bool IsVisibilityTransitionRunning()
        {
            return _visibilityTransition !=
                   EVisibilityTransition.None;
        }


        private bool IsFullyVisible()
        {
            return !IsVisibilityTransitionRunning() &&
                   canvasGroup.alpha >= 0.999f;
        }


        #endregion


        #region Immediate Visibility


        /// <summary>
        /// Немедленно показывает панель.
        ///
        /// Используется когда:
        ///
        /// - обычные экраны закрыты;
        /// - необходимо прервать HideRoutine;
        /// - панель должна быть постоянно видимой.
        /// </summary>
        private void ShowImmediate()
        {
            /*
             * На всякий случай останавливаем
             * текущую visibility animation.
             */
            StopVisibilityCoroutine();


            canvasGroup.alpha = 1f;

            canvasGroup.interactable = true;

            canvasGroup.blocksRaycasts = true;


            _taskPanelToggle.RootButtons(true);
        }


        /// <summary>
        /// Немедленно скрывает панель.
        ///
        /// Используется при открытии обычного UIScreen.
        /// </summary>
        private void HideImmediate()
        {
            /*
             * На всякий случай останавливаем
             * текущую visibility animation.
             */
            StopVisibilityCoroutine();


            canvasGroup.alpha = 0f;

            canvasGroup.interactable = false;

            canvasGroup.blocksRaycasts = false;


            _taskPanelToggle.RootButtons(false);
        }


        #endregion


        #region Refresh


        /// <summary>
        /// Обновляет содержимое панели.
        ///
        /// Источник данных:
        /// ScenarioTaskService.
        ///
        /// Panel не управляет lifecycle задач.
        /// </summary>
        private void Refresh()
        {
            if (_taskService == null)
                return;

            var tasks = _taskService.GetTasks();

            EnsurePoolSize(tasks.Count);

            for (int i = 0; i < _pool.Count; i++)
            {
                bool shouldBeActive =
                    i < tasks.Count;

                if (_pool[i].gameObject.activeSelf != shouldBeActive)
                {
                    _pool[i].gameObject.SetActive(
                        shouldBeActive);
                }
            }

            for (int i = 0; i < tasks.Count; i++)
            {
                _pool[i].Bind(
                    tasks[i],
                    completeTaskAudio);
            }

            scrollRect.SetSizeContentLayoutGroup(
                true,
                null,
                true,
                true);
        }


        private void EnsurePoolSize(int count)
        {
            while (_pool.Count < count)
            {
                _pool.Add(
                    Instantiate(
                        viewPrefab,
                        scrollRect.content));
            }
        }


        #endregion
    }
}