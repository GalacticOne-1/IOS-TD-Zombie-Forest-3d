
using System;
using Galactic1.Core.UI;
using Galactic1.Gameplay.Control;
using Galactic1.Gameplay.Player.StateMachine;
using Galactic1.Gameplay.UI;
using Galactic1.UI.Core;
using Game.Gameplay.Interaction;
using UnityEngine;

namespace Galactic1.Gameplay.Interaction
{
    /// <summary>
    /// Главная система взаимодействия. Поддерживает подсветку, кнопку действия, HP панель.
    /// </summary>
    public class InteractionSystem : MonoBehaviour, IGameService
    {
        [Header("BasicSettings")]
        [Tooltip("Как часто (в секундах) пересчитывать ближайший интерактор по реестру")]
        [SerializeField]
        private float registryScanInterval = 0.15f;

        private PlayerStateMachine _machine;
        private ActionRules _actionRules;
        public InteractionHighlightController _interactionHighlight { get; private set; }

        private IInteractable _current;
        private float _nextScanTime;
        private ITargetable currentTarget;

        public event Action<IInteractable> OnCurrentChanged;



        public void Initialize(
            UIButtonAction actionButton, 
            UIButtonAttack attackButton, 
            TargetHPBarUI targetHpBar)
        {
            OnCurrentChanged = null;
            _current = null;
            currentTarget = null;
            _actionRules = new ActionRules(actionButton, attackButton, targetHpBar);
            
            
            // создаем подсветку для объектов
            _interactionHighlight = "Prefabs/UI/Gameplay/InteractionHighlight"
                .CreateGO(ServiceLocator.Current.Get<UIManager>().TransformRoot.hudRoot)
                .GetComponent<InteractionHighlightController>();
            
            EventBus<SceneClearEvent>.Register(new EventBinding<SceneClearEvent>(() =>
            {
                _interactionHighlight?.gameObject.DestroyGO();
            }));
        }

        public void Setup(PlayerStateMachine machine) => _machine = machine;

        
        
        private void Update()
        {
            // Периодический перерасчёт ближайшего интеракта по реестру.
            if (Time.time >= _nextScanTime)
            {
                _nextScanTime = Time.time + registryScanInterval;
                TryFindNearestFromRegistry();
            }
        }

        /// <summary>
        /// Внешний вызов из InteractionDetector (который использует Physics2D)
        /// или других подсистем, чтобы установить текущий интеракт.
        /// </summary>
        public void SetCurrentInteractable(IInteractable interactable)
        {
            if (_current == interactable) return;

            _current?.OnFocusLost();
            _current = interactable;
            _current?.OnFocus();
            OnCurrentChanged?.Invoke(_current);
        }

        public IInteractable GetCurrent() => _current;

        /// <summary>
        /// Возвращает ближайший интеракт из реестра, сортируя по дистанции и фильтруя недоступные.
        /// Полезно когда detector выключен или нужен альтернативный поиск.
        /// </summary>
        public IInteractable TryFindNearestFromRegistry(Transform reference = null, float maxDistance = 999f)
        {
            if (InteractablesRegistry.All.Count == 0) return null;
            var pos = reference != null ? (Vector2)reference.position : Vector2.zero;

            IInteractable best = null;
            float bestDist = float.MaxValue;

            foreach (var it in InteractablesRegistry.All)
            {
                if (it == null) continue;
                if (!it.IsAvailable) continue;

                var dist = reference != null
                    ? Vector2.Distance(pos, it.WorldPosition)
                    : 0f;

                if (dist > maxDistance) continue;

                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = it;
                }
            }

            return best;
        }


        public void ButtonUp()
        {
            _machine.ActionController.OnActionButtonUp();
        }

        /// <summary>
        /// Вспомогательный API: интерактор может запросить "выполнить текущее".
        /// </summary>
        public void InteractCurrent(Transform interactor)
        {
            if (!_machine.IsInputBlocked() &&
                _current != null && 
                _current.IsAvailable && 
                _current.CanInteract(interactor, ControllableSwitcher.IsDragon))
            {
                _machine.RequestInteract(_machine.PlayerGameObject.transform);
                //_current?.Interact(interactor);
            }
        }
        
        public void AttackCurrent(Transform attacker)
        {
            if (!_machine.IsInputBlocked() &&
                _current is ITargetable targetable && targetable.IsAlive)
            {
                _machine.RequestAttack(_machine.PlayerGameObject.transform);
                //targetable.ReceiveAttack(attacker);
            }
        }

    }
}