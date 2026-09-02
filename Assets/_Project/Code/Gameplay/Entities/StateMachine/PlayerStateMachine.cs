using Galactic1.AbstractFactory;
using UnityEngine;
using Galactic1.Gameplay.Interaction;

namespace Galactic1.Gameplay.Player.StateMachine
{
    /// <summary>
    /// Центральная машина состояний игрока.
    /// Хранит ссылки, переключает состояния, взаимодействует с PlayerActionController и InputLock.
    /// </summary>
    public class PlayerStateMachine : MonoBehaviour
    {
        public GameObject PlayerGameObject => gameObject;

        // Состояния
        private PlayerState _current;
        private PlayerIdleState _idle;
        private PlayerMoveState _move;
        private PlayerJumpState _jump;
        private PlayerWallSlideState _wallSlide;
        private PlayerInteractState _interact;
        private PlayerGatherState _gather;
        private PlayerAttackState _attack;
        private PlayerDeathState _death;

        // Внешние контроллеры
        public PlayerActionController ActionController { get; private set; }
        public InputLock InputLock { get; private set; }

        private void Awake()
        {
            // инициализация зависимостей
            ActionController = GetComponent<PlayerActionController>();
            if (ActionController == null)
                ActionController = gameObject.AddComponent<PlayerActionController>();

            InputLock = new InputLock();

            // создаём состояния
            _idle = new PlayerIdleState(this);
            _move = new PlayerMoveState(this);
            _jump = new PlayerJumpState(this);
            _wallSlide = new PlayerWallSlideState(this);
            _interact = new PlayerInteractState(this);
            _gather = new PlayerGatherState(this);
            _attack = new PlayerAttackState(this);
            _death = new PlayerDeathState(this);
            
            
            // *** для отмены действия после получения урона
            gameObject.GetComponent<PlayerController>().OnDamage += _ =>
            {
                ActionController.CancelCurrentJob();
            };
        }

        private void OnEnable()
        {
            // подписка на внешние события (например, WorldInputDispatcher)
            ServiceLocator.Current.Get<InteractionSystem>().OnCurrentChanged += OnInteractionChanged;
        }

        private void OnDisable()
        {
            ServiceLocator.Current.Get<InteractionSystem>().OnCurrentChanged -= OnInteractionChanged;
        }

        private void Start()
        {
            ChangeState(_idle);
        }

        private void Update()
        {
            _current?.Update();
        }

        private void FixedUpdate()
        {
            _current?.FixedUpdate();
        }

        public void ChangeState(PlayerState next)
        {
            if (_current == next || (_current != null && _current.BlocksInput)) return;
            

            // выход из предыдущего
            _current?.Exit();

            // снять/поставить lock в InputLock по флагу состояния
            if (_current != null && _current.BlocksInput)
                InputLock.Release();

            _current = next;

            // если новое состояние блокирует ввод — ставим блокировку
            if (_current != null && _current.BlocksInput)
                InputLock.Acquire();

            _current?.Enter();
        }


        
        public void ClearState()
        {
            _current = null;
            InputLock.Reset();
        }

        public PlayerState GetCurrentState() => _current;
        public PlayerIdleState GetIdleState() => _idle;
        public PlayerMoveState GetMoveState() => _move;
        public PlayerJumpState GetJumpState() => _jump;
        public PlayerWallSlideState GetWallSlideState() => _wallSlide;
        public PlayerInteractState GetInteractState() => _interact;
        public PlayerState GetGatheringState() => _gather;
        public PlayerAttackState GetAttackState() => _attack;
        public PlayerDeathState GetDeathState() => _death;

        public bool IsInputBlocked() => InputLock.IsLocked;

        /// <summary>
        /// Внешний призыв: начать интеракцию с текущим интерактом (обычно из UI).
        /// </summary>
        public void RequestInteract(Transform interactor = null)
        {
            var current = ServiceLocator.Current.Get<InteractionSystem>().GetCurrent();
            if (current == null) return;

            // использовать ActionController для старта Job
            ActionController.StartActionForInteractable(current, interactor);
        }

        /// <summary>
        /// Внешний призыв: выполнить атаку по текущему интеракту (из кнопки атаки).
        /// </summary>
        public void RequestAttack(Transform attacker = null)
        {
            var current = ServiceLocator.Current.Get<InteractionSystem>().GetCurrent();
            if (current == null) return;

            // если цель поддерживает IAttackable — выполняем
            if (current is ITargetable targetable)
            {
                ActionController.StartAttack(targetable, attacker);
            }
        }

        private void OnInteractionChanged(IInteractable interactable)
        {
            // при смене интеракта можно автоматически переключать состояние на Idle/Move и т.д.
            // но мы оставляем реакцию на ввод пользователю.
        }

        /// <summary>
        /// Устанавливает состояние смерти.
        /// </summary>
        public void Die()
        {
            ChangeState(_death);
        }

        
    }
}
