using System;
using Galactic1.Code.Gameplay.Audio;
using Galactic1.Code.Gameplay.Combat.Events;
using Galactic1.Code.Systems.Tutorial.Presentation;
using Galactic1.Configs;
using Galactic1.Systems;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;


namespace Galactic1.UI.Core
{
    /// <summary>
    /// Advanced UI Button:
    /// • Pointer events
    /// • New Input System friendly
    /// • Scale animations
    /// • Hold
    /// • Double click
    /// • Long press
    /// </summary>
    public abstract class BaseUIButton : MonoBehaviour,
        IPointerDownHandler, IPointerUpHandler,
        IPointerEnterHandler, IPointerExitHandler
    {

        // -------------------- STRUCTURES --------------------

        [System.Serializable]
        public struct Audio
        {
            public bool useSound;
            [Tooltip("Установить конфиг, если нужен другой звук вместо default")]
            public SimpleAudioConfig config;
        }

        [System.Serializable]
        public struct EventSettings
        {
            [Tooltip("true - кнопка будет срабатывать сразу при нажатии")]
            public bool usePoinerDownEvent;
            public UnityEvent onClick;
            public UnityEvent onUp;                 // просто для получения события "релиз кнопки"
            public UnityEvent onDoubleClick;
            public UnityEvent onHold;
            public UnityEvent onLongPress;
        }

        [System.Serializable]
        public class EffectSettings
        {
            [Header("Animation")] 
            public bool animateScale = true;
            public float pressedScale = 0.92f;
            public float animationSpeed = 21f;

            [Header("Hold")] 
            public float holdDelay = 0.25f; // time before hold triggers continuously

            [Header("Long Press")] 
            public float longPressTime = 0.6f;

            [Header("Double Click")] 
            public float doubleClickThreshold = 0.25f;
        }
        
        [Serializable]
        public class AutoRepeat
        {
            public bool  useAutoRepeat    = false;
            public float autoRepeatDelay  = 0;      // пауза перед первым повтором
            public float autoRepeatRate   = 0.5f;   // интервал между кликами в начале
            public float autoRepeatMinRate = 0.05f; // минимальный интервал (максимальная скорость)
            public float autoRepeatAccel  = 0.8f;  // множитель ускорения (< 1 = быстрее)
        }

        // -------------------- SETTINGS --------------------

        [Header("Basic")] 
        [SerializeField] protected Audio audio;

        [SerializeField, Tooltip("true - будет работать даже в обучении")]
        protected bool workInTutorial;

        [SerializeField, Tooltip("Игнорирует общую блокировку CORT.CLOSE_BUTTTONS")]
        public bool ignoreBlock;

        [SerializeField] protected bool interactable = true;
        [SerializeField] protected bool useVibration = false;

        private TutorialTargetBehaviour _tutorialBehaviour;
        

        [Space]
        [Header("Style Config")] 
        [SerializeField] bool requiredStyle = true;
        [FormerlySerializedAs("autoStyle")]
        [Tooltip("Стили бyдyт применятся от внутренних событий down/up и тд")]
        [SerializeField] bool autoState = true;
        [field:SerializeField] public string styleId { get; private set; } = "button_regular_normal";
        [SerializeField] private ButtonStyleConfig _styleConfig;

        [Header("Events")] public EventSettings events;

        [Header("Effects")] [SerializeField] private EffectSettings effects = new();

        [Header("Auto Repeat")] [SerializeField] private AutoRepeat autoRepeat = new();
        

        // -------------------- INTERNAL --------------------

        /// <summary>
        /// Ad button -> если есть экстра условия, проверяем через это событие
        /// <br/>onLock должен вернуть true что бы клик не прошел
        /// </summary>
        public DFuncResponse onLock; // для рекламной кнопки AdButtonPresenter


        private bool adBlocked;
        protected Vector3? originalScale;
        protected bool isPointerDown;
        protected bool isPointerInside;
        protected float pointerDownTime;
        protected float lastClickTime;
        protected bool longPressTriggered;
        private float autoRepeatTimer;
        private float autoRepeatCurrentRate;
        private bool autoRepeatFired;
        private bool isInitialized;

        public enum ButtonState
        {
            Normal,
            Disabled,
            Highlighted,
            Pressed,
            Selected
        }

        public ButtonState CurrentState { get; private set; }

        // -------------------- UNITY --------------------

        private void Awake()
        {
            Initialize();
        }
        public virtual void Initialize(DIContainer container = null)
        {
            if (isInitialized) 
                return;
            isInitialized = true;

            
            _tutorialBehaviour = GetComponent<TutorialTargetBehaviour>();
            
            originalScale = transform.localScale;

            // === если стиля нет, загружаем сами
            if (_styleConfig == null && ServiceLocator.Current != null)
            {
                var styleDatabase = ServiceLocator.Current.Get<ConfigProvider>().Get<UIStyleDatabase>();
                _styleConfig = styleDatabase.Get<ButtonStyleConfig>(styleId);
                AutoState(ButtonState.Normal);
            }
            
            // === если нет звука берем базовый
            if (audio.config == null && ServiceLocator.Current != null)
            {
                audio.config = ServiceLocator.Current.Get<ConfigProvider>()
                    .Get<UIAudioDatabase>()
                    .Get<SimpleAudioConfig>("audio_cue_button_default");
            }
        }

        private void Update()
        {
            if(!originalScale.HasValue) 
                return;
            
            if (effects.animateScale)
            {
                Vector3 target = isPointerDown 
                    ? originalScale.Value * effects.pressedScale 
                    : originalScale.Value;
                transform.localScale =
                    Vector3.Lerp(transform.localScale, target, Time.deltaTime * effects.animationSpeed);
            }

            if (ClickBlocked() || !isPointerDown) return;

            float heldTime = Time.time - pointerDownTime;

            if (!longPressTriggered && heldTime >= effects.longPressTime)
            {
                longPressTriggered = true;
                events.onLongPress?.Invoke();
            }

            if (heldTime >= effects.holdDelay)
            {
                if (!autoRepeat.useAutoRepeat)
                    events.onHold?.Invoke();
            }
            
            if (autoRepeat.useAutoRepeat)
            {
                if (heldTime >= autoRepeat.autoRepeatDelay)
                {
                    autoRepeatTimer -= Time.deltaTime;

                    if (autoRepeatTimer <= 0f)
                    {
                        autoRepeatFired = true;
                        HandleClick();

                        // Ускоряем — уменьшаем интервал до минимума
                        autoRepeatCurrentRate = Mathf.Max(
                            autoRepeat.autoRepeatMinRate,
                            autoRepeatCurrentRate * autoRepeat.autoRepeatAccel);

                        autoRepeatTimer = autoRepeatCurrentRate;
                    }
                }
            }
        }
        
        void Clear()
        {
            isPointerDown = false;
        }

        // -------------------- POINTER EVENTS --------------------

        /// <summary>Called when pointer presses down on this button.</summary>
        public void OnPointerDown(PointerEventData eventData)
        {
            if (ClickBlocked()) return;

            Clear();
            isPointerDown = true;
            pointerDownTime = Time.time;
            longPressTriggered = false;
            autoRepeatFired = false;
            autoRepeatTimer = autoRepeat.autoRepeatDelay <= 0f ? 0f : autoRepeat.autoRepeatRate;
            autoRepeatCurrentRate = autoRepeat.autoRepeatRate;

            if (events.usePoinerDownEvent)
                HandleClick();

            AutoState(ButtonState.Pressed);
        }

        /// <summary>Called when pointer releases from this button.</summary>
        public void OnPointerUp(PointerEventData eventData)
        {
            if (ClickBlocked()) return;
            isPointerDown = false;

            if (eventData.dragging)
            {
                AutoState(ButtonState.Normal);
                return;
            }

            float timeSinceLastClick = Time.time - lastClickTime;
            lastClickTime = Time.time;

            if (isPointerInside)
            {
                if (timeSinceLastClick <= effects.doubleClickThreshold)
                    events.onDoubleClick?.Invoke();
                else if(!events.usePoinerDownEvent)
                {
                    if (!autoRepeatFired) // Не кликаем если autoRepeat уже сработал
                        HandleClick();
                }
                else
                    events.onUp?.Invoke();
            }

            AutoState(ButtonState.Normal);
        }

        /// <summary>Called when pointer enters the button area.</summary>
        public void OnPointerEnter(PointerEventData eventData) => isPointerInside = true;

        /// <summary>Called when pointer exits the button area.</summary>
        public void OnPointerExit(PointerEventData eventData) => isPointerInside = false;

        // -------------------- CLICK HANDLING --------------------

        protected virtual bool ClickBlocked()
        {
            new TUTORIAL_AvailButton(gameObject, out bool isTutorial);
            return CORT.BLOCK_BUTTTONS && !ignoreBlock // глобальная блокировка всех кнопок
                   || !interactable // блокировка отдельной кнопки
                   || isTutorial && !workInTutorial; // блокировка по обучению
        }

        /// <summary>Programmatically triggers a click event.</summary>
        public void TriggerClick() => HandleClick();

        /// <summary>Handles the click, including vibration, sound, and invoking events.</summary>
        protected virtual bool HandleClick()
        {
            if (ClickBlocked()) 
                return false;

            // воспроизведение звука / вибрации
            PlayClickFeedback();

            events.onClick?.Invoke();
            
            // === событие для тутора вызывается только если на кнопке есть TutorialTargetBehaviour
            if(_tutorialBehaviour != null)
            {
                EventBus<UITargetInteractedEvent>.Raise(new UITargetInteractedEvent()
                {
                    TargetId = _tutorialBehaviour.TargetId
                });
            }
            
            return true;
        }

        protected void PlayClickFeedback()
        {
            if (audio.useSound)
            {
                EventBus<AudioUIEvent>.Raise(new AudioUIEvent(audio.config?.ToData()));
            }

            if (useVibration)
                ServiceLocator.Current.Get<SettingsManager>().Vibro(); 
        }

        // -------------------- STATE --------------------
        
        /// <summary>
        /// Для внешней прпедачи конфига
        /// </summary>
        /// <param name="styleConfig"></param>
        public void SetStyleConfig(ButtonStyleConfig styleConfig)
        {
            _styleConfig = styleConfig;
            SetState(ButtonState.Normal);
        }

        /// <summary>Sets the interactable state of the button.</summary>
        /// <param name="yes">True to make button interactable, false to disable it.</param>
        public void SetInteractable(bool yes)
        {
            if (!yes) Clear();
            interactable = yes;
            SetState(interactable ? ButtonState.Normal : ButtonState.Disabled);
        }

        /// <summary>
        /// Устанавливает состояние кнопки
        /// <br/>Не влияет на графику кнопки
        /// </summary>
        /// <param name="yes"></param>
        public void SetInteractableOnly(bool yes)
        {
            if (!yes) Clear();
            interactable = yes;
        }

        public void SetSelected(bool yes)
            => SetState(yes ? ButtonState.Selected : ButtonState.Normal);

        // использовать для внутренних вызовов, от событий самой кнопки
        void AutoState(ButtonState state) { if (autoState && !adBlocked) SetState(state); }

        /// <summary>Sets the visual state of the button (Normal, Disabled, Pressed).</summary>
        /// <param name="state">ButtonState to apply.</param>
        private void SetState(ButtonState state)
        {
            CurrentState = state;
            if (!requiredStyle || _styleConfig == null) 
                return;
            
            switch (state)
            {
                case ButtonState.Normal:
                    gameObject.CMP_Image().sprite = _styleConfig.normal;
                    break;
                case ButtonState.Disabled:
                    gameObject.CMP_Image().sprite = _styleConfig.disabled;
                    break;
                case ButtonState.Highlighted:
                    gameObject.CMP_Image().sprite = _styleConfig.highlighted;
                    break;
                case ButtonState.Pressed:
                    gameObject.CMP_Image().sprite = _styleConfig.pressed;
                    break;
                case ButtonState.Selected:
                    gameObject.CMP_Image().sprite = _styleConfig.selected;
                    break;
            }
        }


        protected void AdStatus(bool allowed)
        {
            if (allowed)
            {
                adBlocked = false;
                SetState(ButtonState.Normal);
            }
            else
            {
                SetState(ButtonState.Disabled);
                adBlocked = true;
            }
        }
    }
}
