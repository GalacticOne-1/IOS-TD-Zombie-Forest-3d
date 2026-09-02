using System;
using System.Collections.Generic;
using Galactic1.Code.UI.Interaction;
using Galactic1.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Code.UI.UnitCard
{
    /// <summary>
    /// UI карточки юнита + локальная state machine (только UI состояние).
    /// </summary>
    public sealed class UnitCardView : MonoBehaviour, IUIFocusable
    {
        
        [SerializeField] private GameObject cLock;
        
        [Header("Unit")] 
        [SerializeField] private TMP_Text callSignText;
        [SerializeField] private Image hpFill;
        [SerializeField] private RawImage unitPortrait;

        [Header("Weapon")] 
        [SerializeField] private GameObject weaponRoot;
        [SerializeField] private GameObject weaponEmptyLabel;
        [SerializeField] private TMP_Text weaponDurabilityText;
        [SerializeField] private TMP_Text ammoText;
        [SerializeField] private TMP_Text ammoTotalText;
        [SerializeField] private Image durabilityFill;
        [SerializeField] private Image ammoFill;

        [Header("Ability Slots")] 
        [SerializeField] private GameObject quickSlotRoot;
        [SerializeField] private GameObject quickSlotPreviewRoot;


        [Header("Buttons")] 
        [SerializeField] private GameObject weaponButton;
        [SerializeField] private GameObject quickSlotButton;
        [SerializeField] private GameObject abilityCancelButton;
        [SerializeField] private RectTransform rootRect;

        public int Priority => UIPriority.Ability;

        // =========================
        // State
        // =========================
        private enum State
        {
            Normal,
            AbilitySelect,
            AbilityActive,
            Lock
        }

        private State currentState;

        private UnitCardQuickSlot[] quickSlots;
        
        
        private int activeAbilityIndex = -1;
        private int activeSlots;
        private List<QuickSlotViewDTO> _currentAbilityData;

        // callbacks (в Presenter)
        private Action onWeaponClick;
        private Action<int> onAbilitySelected;
        private Action onCancel;

        private UIStyleResolver _styleResolver;



        // =========================
        // Init
        // =========================
        public void Initialize(UIStyleResolver styleResolver)
        {
            _styleResolver = styleResolver;

            var l = quickSlotRoot.transform.childCount;
            quickSlots = new UnitCardQuickSlot[l];
            for (int i = 0; i < l; i++)
            {
                quickSlots[i] = quickSlotRoot.GetChild(i).GetComponent<UnitCardQuickSlot>();
            }
            
            
            SwitchState(State.Normal);
        }

        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);
        
        
        
        // =========================
        // IUIFocusable
        // =========================
        public bool ContainsScreenPoint(Vector2 screenPos)
        {
            return UIRaycastUtility.IsPointerOver(gameObject, screenPos);
        }

        public void OnFocusLost()
        {
            if (currentState != State.Normal)
                SwitchState(State.Normal);
        }
        
        // =========================
        // Render
        // =========================

        public void RenderUnit(string callSign, RenderTexture texture)
        {
            callSignText.text = callSign;
            unitPortrait.texture = texture;
        }

        public void RenderHP(float current, float max)
        {
            hpFill.fillAmount = max > 0 ? current / max : 0f;
        }

        public void RenderAmmo(int inClip, int clipSize, int total)
        {
            ammoText.text = inClip.ToString();
            ammoTotalText.text = total.ToString();
            ammoFill.fillAmount = clipSize > 0 ? (float)inClip / clipSize : 0f;
        }

        public void RenderDurability(int durability, float durability01)
        {
            durabilityFill.fillAmount = durability01;
            weaponDurabilityText.text = $"{durability}%";
            weaponDurabilityText.color = _styleResolver.ResolveValueColor(ValueRangeType.Durability, durability01);
        }

        public void RenderWeaponHeader(bool empty, Sprite icon)
        {
            weaponEmptyLabel.SetActive(empty);
            weaponRoot.SetActive(!empty);
            if (!empty && icon != null)
                weaponButton.GetChild(0).CMP_Image().sprite = icon;
        }

        public void RenderAbilities(List<QuickSlotViewDTO> data)
        {
            _currentAbilityData = data; // сохраняем для GetAbilityDTO
            activeSlots = 0;
            var l = quickSlots.Length;

            for (int i = 0; i < l; i++)
            {
                bool has = i < data.Count && data[i].HasItem;

                quickSlots[i].Show(has);
                quickSlotPreviewRoot.GetChild(i).gameObject.SetActive(has);

                if (!has) continue;

                activeSlots++;
                var d = data[i];
                quickSlots[i].Bind(d.Icon, d.Count);
                quickSlotPreviewRoot.GetChild(i).CMP_Image().sprite = d.Icon;
            }
        }
        
        public QuickSlotViewDTO GetAbilityDTO(int visualIndex)
        {
            if (_currentAbilityData == null || 
                visualIndex < 0 || 
                visualIndex >= _currentAbilityData.Count)
                return default;
            return _currentAbilityData[visualIndex];
        }
        
        public void HighlightSingleSlot(int index)
        {
            for (int i = 0; i < quickSlots.Length; i++)
                quickSlots[i].Show(index == -1 && i < activeSlots || i == index);
        }

        // =========================
        // Bindings
        // =========================
        public void BindWeaponClick(Action action)
        {
            onWeaponClick = action;
            weaponButton.RegisterButtonClick(() => onWeaponClick?.Invoke());
            weaponEmptyLabel.RegisterButtonClick(() => onWeaponClick?.Invoke());
        }
        
        
        public void SetAbilityButtonInteractable(bool interactable)
        {
            quickSlotButton.ButtonSetInteractable(interactable);
        }

        public void BindAbilityButton(Action openAction)
        {
            quickSlotButton.RegisterButtonClick(() =>
            {
                openAction?.Invoke();
                SwitchState(State.AbilitySelect);
            });
        }

        public void BindAbilitySlots(Action<int> onSelected)
        {
            onAbilitySelected = onSelected;

            var l = quickSlotRoot.transform.childCount;
            for (int i = 0; i < l; i++)
            {
                int index = i;
                quickSlots[i].gameObject.RegisterButtonClick(() => { ActivateAbility(index); });
            }
        }

        public void BindCancel(Action cancelAction)
        {
            onCancel = cancelAction;

            abilityCancelButton.RegisterButtonClick(() =>
            {
                cancelAction?.Invoke();
                SwitchState(State.Normal);
            });
        }
        
        
        public void SwitchToNormal() => SwitchState(State.Normal);
        public void SwitchToAbilitySelect() => SwitchState(State.AbilitySelect);
        public void SwitchToAbilityActive() => SwitchState(State.AbilityActive);
        public void SwitchToLock() => SwitchState(State.Lock);

        // =========================
        // State Machine
        // =========================
        private void SwitchState(State state)
        {
            currentState = state;

            switch (state)
            {
                case State.Normal:
                    quickSlotPreviewRoot.SetActive(true);
                    quickSlotRoot.SetActive(false);
                    abilityCancelButton.SetActive(false);
                    quickSlotButton.gameObject.SetActive(true);
                    cLock.SetActive(false);
                    ResetSlots();
                    break;

                case State.AbilitySelect:
                    quickSlotPreviewRoot.SetActive(false);
                    quickSlotRoot.SetActive(true);
                    abilityCancelButton.SetActive(false);
                    quickSlotButton.gameObject.SetActive(true);
                    cLock.SetActive(false);
                    ResetSlots();
                    break;

                case State.AbilityActive:
                    quickSlotPreviewRoot.SetActive(false);
                    quickSlotRoot.SetActive(true);
                    abilityCancelButton.SetActive(true);
                    quickSlotButton.gameObject.SetActive(true);
                    cLock.SetActive(false);
                    break;
                
                case State.Lock:
                    quickSlotPreviewRoot.SetActive(false);
                    quickSlotRoot.SetActive(false);
                    abilityCancelButton.SetActive(false);
                    quickSlotButton.gameObject.SetActive(false);
                    cLock.SetActive(true);
                    break;
            }
        }

        private void ActivateAbility(int index)
        {
            activeAbilityIndex = index;
            onAbilitySelected?.Invoke(index);
        }

        private void ResetSlots()
        {
            activeAbilityIndex = -1;
            HighlightSingleSlot(-1);
        }
    }
}