using Galactic1;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Galactic1
{
    public abstract class CoreBtn : MonoBehaviour
    {
        [SerializeField] protected EBtnSound useSound = EBtnSound.noSound;
        [SerializeField, Header("Для вызова звука")] protected int soundId;
        [SerializeField, Header("true - что бы c вибро")] private bool vibro;
        [SerializeField, Header("true - будет работать даже в обучении")] protected bool freeUse;
        [SerializeField, Tooltip("Игнорирует общую блокировку CORT.CLOSE_BUTTTONS")] public bool ignoreBlock;

        //[SerializeField, Tooltip("Спрайты для состояния")] 
        //private IconHub.ESpButtons stateOn, stateOff, stateSelected;

        [Space]
        [SerializeField] private bool stateTxt;
       // [SerializeField] protected IconHub.EColTxt txtStateOn, txtStateOff;

        protected EStateButton STATE = EStateButton.ENABLE;
        
        
        
        /// Для блокировки кнопки
        public bool CLICK_BLOCKED { set; get; }
        
        /// Зависимость от прогресса игрока
        public byte REQUIRED_PROGRESS { set; get; }
        
        
        
        public enum EBtnSound
        {
            noSound, defaultSound, soundUI, soundGame
        }
        
        public UnityEvent _event;
        public DFunc onPointerDown;


        /// <summary>
        /// Звук при нажатии
        /// </summary>
        protected void SoundClick()
        {
            switch (useSound)
            {
                case EBtnSound.defaultSound:
                    //ServiceLocator.Current.Get<AudioController>().Sound_UI(0);
                    break;
                
                case EBtnSound.soundUI:
                    //ServiceLocator.Current.Get<AudioController>().Sound_UI(soundId);
                    break;
                
                case EBtnSound.soundGame:
                    //ServiceLocator.Current.Get<AudioController>().Sound_Game(soundId);
                    break;
            }
        }
        
        /// <summary>
        /// Запуск вибро
        /// </summary>
        protected void Vibro()
        {
            //if (vibro)
                //ServiceLocator.Current.Get<W_Options>().Vibro();
        }

        protected virtual bool ClickBlocked()
        {
            new TUTORIAL_AvailButton(gameObject, out bool isTutorial);
            return CORT.BLOCK_BUTTTONS && !ignoreBlock                      // глобальная блокировка всех кнопок
                   || CLICK_BLOCKED                                         // блокировка отдельной кнопки
                   || isTutorial && !freeUse;                              // блокировка по обучению
        }
        
        
        /// <summary>
        /// Для функции при клике
        /// </summary>
        public virtual void OnClick() {}
        
        /// <summary>
        /// Для вызова обычного клика, но из скритпа
        /// </summary>
        public virtual void CallClick() {}



        #region STATE BTN

        public void ToState(EStateButton state)
        {
            CLICK_BLOCKED = false;
            switch (state)
            {
                case EStateButton.DISABLE:
                {
                    CLICK_BLOCKED = true;
                    //GetComponent<Image>().sprite = ServiceLocator.Current.Get<IconHub>().GetSpriteButtons(stateOff);
                    State_Disable();
                } break;
                
                case EStateButton.ENABLE:
                {
                    //GetComponent<Image>().sprite = ServiceLocator.Current.Get<IconHub>().GetSpriteButtons(stateOn);
                    State_Enable();
                } break;
                
                case EStateButton.SELECTED:
                {
                    //GetComponent<Image>().sprite = ServiceLocator.Current.Get<IconHub>().GetSpriteButtons(stateSelected);
                    State_Selected();
                } break;
                
                case EStateButton.TXT_REGULAR:
                {
                    if (STATE != EStateButton.DISABLE)
                    {
                        State_TXT_Regular();
                    }
                } break;
                
                case EStateButton.TXT_ALERT:
                {
                    if (STATE != EStateButton.DISABLE)
                    {
                        State_TXT_Alert();
                    }
                } break;

                case EStateButton.ENABLE_ONLY_TEXT:
                {
                    State_Enable();
                } break;
                
                case EStateButton.DISABLE_ONLY_TEXT:
                {
                    CLICK_BLOCKED = true;
                    State_Disable();
                } break;
            }
        }
        
        protected virtual void State_Enable(){}
        
        protected virtual void State_Disable(){}
        protected virtual void State_Selected(){}
        
        protected virtual void State_TXT_Regular(){}
        
        protected virtual void State_TXT_Alert(){}
        
        #endregion
    }
}