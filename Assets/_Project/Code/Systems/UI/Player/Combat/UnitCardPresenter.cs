using System;
using System.Collections.Generic;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Gameplay.Abilities;
using Galactic1.Code.Gameplay.Effect;
using Galactic1.Code.Gameplay.Survivors.Repositories;
using Galactic1.Code.Systems.Raid;
using Galactic1.Code.Gameplay.Units;
using Galactic1.Code.Gameplay.Units.Stats;
using Galactic1.Code.UI.Interaction;
using Galactic1.UI.CharacterPreview;
using Galactic1.UI.Core;

namespace Galactic1.Code.UI.UnitCard
{
    /// <summary>
    /// Presenter карточки юнита.
    /// Читает только Runtime, НЕ Scene.
    /// </summary>
    public sealed class UnitCardPresenter : IDisposable
    {
        private readonly IUnitRuntime _runtime;
        private readonly SquadUICoordinator _squadUI;
        private readonly UnitCardView _view;
        private readonly ItemUseService _itemUseService;
        private readonly List<IUnitRuntime> _squadMembers;
        private readonly AbilityUseCoordinator _abilityUse;
        private UIInputRouter _inputRouter;

        private IWeaponWithEvents _weapon;

        private int _activeQuickIndex;
        private readonly Action<int, string> _onInventoryOpen;
        private int viewIndex;

        public UnitCardPresenter(
            int index,
            IUnitRuntime runtime,
            UnitCardView view,
            Action<int, string> onInventoryOpen,
            ItemUseService itemUseService,
            List<IUnitRuntime> squadMembers,
            AbilityUseCoordinator abilityUse, 
            SquadUICoordinator squadUI, 
            UIInputRouter inputRouter)
        {
            viewIndex = index;
            _runtime = runtime;
            _view = view;
            _itemUseService = itemUseService;
            _squadMembers = squadMembers;
            _abilityUse = abilityUse;
            _squadUI = squadUI;
            _inputRouter = inputRouter;
            _onInventoryOpen = onInventoryOpen;

            Bind();
        }

        // =========================
        // Bind
        // =========================
        private void Bind()
        {
            _view.Initialize(ServiceLocator.Current.Get<UIStyleResolver>());

            _runtime.Stats.OnStatChanged += OnStatChanged;
            _runtime.Stats.OnDeath += OnUnitDie;
            _runtime.Weapon.OnWeaponChanged += OnWeaponChanged;
            _runtime.InventorySource.Equipment.OnChangedPersistent += OnInventoryChanged;
            _runtime.Status.AbilityBusyChanged += OnAbilityBusyChanged;
            
            _squadUI.OnAbilitySelectOpened += OnOtherCardAbilityOpened;
            _squadUI.OnTargetingStarted += OnTargetingStarted;
            _squadUI.OnTargetingStopped += OnTargetingStopped;
            _view.BindAbilityButton(OnAbilityOpen);

            _view.BindWeaponClick(() => OnWeaponClick(viewIndex, _runtime.Id));
            _view.BindAbilityButton(OnAbilityOpen);
            _view.BindAbilitySlots(OnAbilitySelected);
            _view.BindCancel(OnAbilityCancel);

            BindWeapon(_runtime.Weapon.CurrentWeapon);
            RenderAll();
        }



        // =========================
        // Weapon
        // =========================

        private void BindWeapon(IWeaponWithEvents weapon)
        {
            if (_weapon != null)
            {
                _weapon.OnAmmoChanged -= OnAmmoChanged;
                _weapon.OnDurabilityChanged -= OnDurabilityChanged;
            }

            _weapon = weapon;

            if (_weapon != null)
            {
                _weapon.OnAmmoChanged += OnAmmoChanged;
                _weapon.OnDurabilityChanged += OnDurabilityChanged;
            }
        }



        // =========================
        // Handlers → точечный рендер
        // =========================
        private void OnStatChanged(StatChangedEvent e, bool _)
        {
            if (e.Type == StatId.Health)
                _view.RenderHP(e.Current, e.Max);
        }
        
        private void OnUnitDie()
        {
            _activeQuickIndex = -1;
            _abilityUse.Cancel();
            _view.SwitchToLock();
            Dispose();
        }

        private void OnAmmoChanged(int inClip, int clipSize)
        {
            _view.RenderAmmo(inClip, clipSize, 0);
        }

        private void OnDurabilityChanged(int cur, float cur01)
        {
            _view.RenderDurability(cur, cur01);
        }

        private void OnWeaponChanged(IWeaponWithEvents weapon)
        {
            BindWeapon(weapon);
            RenderWeaponFull(); // смена оружия = полный ребилд секции
        }
        
        private void OnAbilityBusyChanged(bool busy)
        {
            _view.SetAbilityButtonInteractable(!busy);
        }


        // =========================
        // Inventory changed → перерисовать quick slots
        // =========================
        private void OnInventoryChanged()
        {
            RenderAbilities();
        }

        // =========================
        // Render
        // =========================

        /// Первый показ — всё сразу
        private void RenderAll()
        {
            var portraitCache = ServiceLocator.Current.Get<CharacterPortraitCache>();
            _view.RenderUnit(_runtime.DisplayName, portraitCache.GetPortrait(_runtime.ArchetypeId));
            _view.RenderHP(_runtime.Stats.CurrentHP, _runtime.Stats.MaxHP);
            RenderAbilities();
            RenderWeaponFull();
        }
        
        private void RenderAbilities()
        {
            var source = _runtime.InventorySource.Equipment;
            var groups = new Dictionary<RuntimeId, QuickSlotViewDTO>();
            var groupOrder = new List<RuntimeId>();

            for (int i = 0; i < QuickSlotMapping.SlotCount; i++)
            {
                var slot = _runtime.QuickSlot.GetSlot(source, i);
                if (slot == null || slot.IsEmpty) continue;

                var key = slot.Item.Id;

                if (!groups.TryGetValue(key, out var dto))
                {
                    dto = new QuickSlotViewDTO
                    {
                        HasItem = true,
                        Icon = slot.Item.Header.icon,
                        Count = 0,
                        SourceSlotIndices = new List<int>()
                    };
                    groups[key] = dto;
                    groupOrder.Add(key);
                }

                dto.Count += slot.Amount;
                dto.SourceSlotIndices.Add(i);
                groups[key] = dto; // struct — перезаписываем
            }

            var data = new List<QuickSlotViewDTO>();
            foreach (var key in groupOrder)
                data.Add(groups[key]);

            while (data.Count < QuickSlotMapping.SlotCount)
                data.Add(new QuickSlotViewDTO { HasItem = false });

            _view.RenderAbilities(data);
        }

        /// Смена оружия — секция целиком
        private void RenderWeaponFull()
        {
            bool empty = _weapon == null;
            _view.RenderWeaponHeader(
                empty,
                empty ? null : _weapon.Entity.Module.Item.Header.icon
            );

            if (!empty)
            {
                _view.RenderAmmo(_weapon.CurrentAmmo, _weapon.ClipSize, 0);
                _view.RenderDurability(_weapon.Durability, _weapon.Durability01);
            }
        }


        // =========================
        // UI Actions
        // =========================
        private void OnWeaponClick(int viewIndex, string unitId)
        {
            _onInventoryOpen?.Invoke(viewIndex, unitId);
        }

        private void OnAbilityOpen()
        {
            _squadUI.NotifyAbilitySelectOpened();
            _inputRouter.SetFocus(_view);
        }
        
        private void OnOtherCardAbilityOpened()
        {
            // эта карточка не является инициатором — сбрасываем
            _activeQuickIndex = -1;
            _view.SwitchToNormal();
        }
        
        private void OnTargetingStarted(TargetingUIData data)
        {
            _activeQuickIndex = -1;
            _view.Hide(); 
        }

        private void OnTargetingStopped()
        {
            _activeQuickIndex = -1;
            _view.SwitchToNormal();
            _view.Show();
        }

        private void OnAbilitySelected(int quickIndex)
        {
            var source = _runtime.InventorySource.Equipment;
            var dto = _view.GetAbilityDTO(quickIndex);
            if (!dto.HasItem || dto.SourceSlotIndices == null) 
                return;

            // Среди слотов группы — берём с наименьшим количеством
            int bestSlotIndex = -1;
            int bestAmount = int.MaxValue;

            foreach (int srcIdx in dto.SourceSlotIndices)
            {
                var slot = _runtime.QuickSlot.GetSlot(source, srcIdx);
                if (slot == null || slot.IsEmpty) 
                    continue;
                
                if (slot.Amount < bestAmount)
                {
                    bestAmount = slot.Amount;
                    bestSlotIndex = srcIdx;
                }
            }

            if (bestSlotIndex == -1) 
                return;

            var bestSlot = _runtime.QuickSlot.GetSlot(source, bestSlotIndex);
            var behaviour = bestSlot?.Item?.Use?.Behaviour;
            if (behaviour == null) 
                return;

            var repo = ServiceLocator.Current.Get<SurvivorRepository>().TryGet(_runtime.Id);

            var ctx = new ItemUseContext
            {
                User = _runtime,
                SceneUnit = repo.instance?.UnitAdapter,
                InventorySource = _runtime.InventorySource.Equipment,
                UseModule = bestSlot.Item.Use,
                SlotIndex = _runtime.QuickSlot.GetInventoryIndex(bestSlotIndex),
                QuickSlotIndex = bestSlotIndex, // реальный индекс, не визуальный
                SquadMembers = _squadMembers
            };

            switch (behaviour.ActivationType)
            {
                case UseActivationType.Instant:
                    _abilityUse.Use(ctx);
                    _view.SwitchToNormal();
                    break;

                case UseActivationType.Targeting:
                    _view.HighlightSingleSlot(quickIndex);
                    _view.SwitchToAbilityActive();
                    _activeQuickIndex = quickIndex;

                    ctx.OnConfirmed = () =>
                    {
                        _activeQuickIndex = -1;
                        _view.SwitchToNormal();
                    };

                    //_abilityUse.Use(ctx, behaviour);
                    // var instance = _runtime.GetInstance;
                    // if (instance != null)
                    // {
                    //     var cmd = new AbilityCommand(ctx, behaviour);
                    //
                    //     // Targeting — контекст подтверждения пробрасываем через команду,
                    //     // OnConfirmed уже установит UsingAbilityState в OnEnter
                    //     instance.StateMachine.Execute(cmd);
                    // }
                    _abilityUse.Use(ctx);
                    break;
            }
        }

        private void OnAbilityCancel()
        {
            _abilityUse.Cancel();
            _activeQuickIndex = -1;
            _view.SwitchToNormal();
            _inputRouter.ClearFocus();
        }
        

        // =========================
        // Dispose
        // =========================
        public void Dispose()
        {
            _runtime.Stats.OnStatChanged -= OnStatChanged;
            _runtime.Stats.OnDeath -= OnUnitDie;
            _runtime.Weapon.OnWeaponChanged -= OnWeaponChanged;
            _runtime.InventorySource.Equipment.OnChangedPersistent -= OnInventoryChanged;
            _runtime.Status.AbilityBusyChanged -= OnAbilityBusyChanged;
            _squadUI.OnAbilitySelectOpened -= OnOtherCardAbilityOpened;
            _squadUI.OnTargetingStarted -= OnTargetingStarted;
            _squadUI.OnTargetingStopped -= OnTargetingStopped;

            if (_weapon != null)
            {
                _weapon.OnAmmoChanged -= OnAmmoChanged;
                _weapon.OnDurabilityChanged -= OnDurabilityChanged;
            }
        }
    }
}