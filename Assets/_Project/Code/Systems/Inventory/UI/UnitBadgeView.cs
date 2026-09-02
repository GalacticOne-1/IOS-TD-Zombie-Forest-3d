
using System;
using Galactic1.Code.Systems.Squad;
using Galactic1.Code.UI.Units;
using Galactic1.UI.CharacterPreview;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Code.UI.Inventory
{
    /// <summary>
    /// UI element for a single unit inside the management scroll list.
    /// Responsible only for visuals and click forwarding.
    /// </summary>
    public sealed class UnitBadgeView : MonoBehaviour
    {
        [Header("UI Refs")]
        [SerializeField] private RawImage portrait;
        [SerializeField] private Image highlight, hpFill;
        [SerializeField] private GameObject badge;
        [SerializeField] private GameObject root;
        [SerializeField] private GameObject deadLabel;

        private CharacterPortraitHandle portraitHandle;
        private int viewIndex;
        private UnitDisplayData unit;
        
        
        private readonly CompositeDisposable _disposables = new();

        /// <summary>
        /// Bind unit data to this view.
        /// </summary>
        public void Bind(
            UnitDisplayData unit, 
            int index, 
            Action<int, string> onClicked,
            bool isSelected,
            StrategicSquadSystem squadSystem,
            CharacterPortraitCache portraitCache)
        {
            // 🔥 ВАЖНО
            ClearBindings();
            
            this.unit = unit;
            viewIndex = index;
            
            // Запрашиваем портрет
            portrait.texture = portraitCache.GetPortrait(unit.ArchetypeId);
            
            gameObject.SetActive(true);
            gameObject.RegisterButtonClick(() => onClicked?.Invoke(index, unit.Id));

            squadSystem.OnSquadChanged += OnSquadChanged;
            _disposables.Add(Disposable.Create(() => squadSystem.OnSquadChanged -= OnSquadChanged));
            
            OnSquadChanged(unit.Id, squadSystem.IsInSquad(unit.Id));

            RefreshStatic();
            RefreshHP();
            SetHighlight(isSelected);
        }

        private void ClearBindings()
        {
            _disposables.Clear();
            
            portraitHandle?.Dispose();
            portraitHandle = null;
            portrait.texture = null;
        }

        private void OnDestroy()
        {
            ClearBindings();
        }
        
        private void OnSquadChanged(string changedUnitId, bool isInSquad)
        {
            if (changedUnitId == unit.Id)
            {
                badge.SetActive(isInSquad);
            }
        }

      

        private void RefreshStatic()
        {
            // подписка через _disposables — автоматически отпишется в ClearBindings()
            unit.Stats.Get(StatId.Health)
                .Subscribe(hp =>
                {
                    bool isDead = hp <= 0;
                    deadLabel.SetActive(isDead);
                    hpFill.transform.parent.gameObject.SetActive(!isDead);
                    
                    hpFill.fillAmount = Mathf.Clamp01(hp / unit.Stats.MaxHP);
                })
                .AddTo(_disposables);

            // Начальное состояние
            // bool initialDead = unit.Stats.IsDead;
            // deadLabel.SetActive(initialDead);
            // hpFill.transform.parent.gameObject.SetActive(!initialDead);
        }

        /// <summary>
        /// Updates HP bar fill.
        /// </summary>
        public void RefreshHP()
        {
            // if (hpFill == null || unit == null)
            //     return;
            //
            // unit.Stats.Get(StatId.Health)
            //     .Subscribe(hp =>
            //     {
            //         hpFill.fillAmount = Mathf.Clamp01(hp / unit.Stats.MaxHP);
            //     })
            //     .AddTo(_disposables);
        }

        /// <summary>
        /// Visual selection state.
        /// </summary>
        public void SetHighlight(bool selected)
        {
            if (highlight != null)
                highlight.enabled = selected;
                
                SetSelectedPosition(selected);
        }
        
        /// <summary>
        /// Sets vertical offset for selection highlight.
        /// </summary>
        public void SetSelectedPosition(bool selected)
        {
            if (root == null)
                return;

            var pos = root.transform.localPosition;
            pos.y = selected ? 20f : 0f;
            root.transform.localPosition = pos;
        }

    }
}
