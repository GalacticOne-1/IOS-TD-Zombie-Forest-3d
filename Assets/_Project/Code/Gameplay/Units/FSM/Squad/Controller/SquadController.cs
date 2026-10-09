using Galactic1.Code.Gameplay.Audio;
using Galactic1.Code.Gameplay.Combat.Events;
using Galactic1.Code.Gameplay.Interaction;
using Galactic1.Configs;
using Galactic1.UI.Core;
using UnityEngine;

namespace Galactic1.Code.Systems.Squad
{
    /// <summary>
    /// Тонкий координатор. Принимает команды, передаёт в MovementSystem.
    /// Не вызывает pathService.Tick() — сервис пассивен.
    /// </summary>
    public sealed class SquadController : MonoBehaviour, IGameService, IUpdate
    {
        [SerializeField] private SquadPathService pathService;
        public SquadPathService PathService => pathService;

        public SquadSceneRuntime Squad { get; private set; }
        
        private SimpleAudioConfig config;

        private WorldInputDispatcher _worldInput;
        private SquadMovementSystem _movementSystem;
        public SquadMovementSystem MovementSystem => _movementSystem;

        private bool _initialized;

        public void Initialize(SquadSceneRuntime squad, WorldInputDispatcher worldInput)
        {
            _initialized = true;
            Squad = squad;
            _worldInput = worldInput;

            _movementSystem = new SquadMovementSystem(
                squad,
                GetComponent<SquadTrailRenderer>(),
                pathService);
            
            config = ServiceLocator.Current.Get<ConfigProvider>()
                .Get<UIAudioDatabase>()
                .Get<SimpleAudioConfig>("audio_cue_squad_move_command");

            // CenterProvider: MovementSystem.Center — путь запрашивается
            // через pathService.SetTarget(from, to), from берётся из Center.
            // SquadPathService больше не нужен BindCenterProvider.

            _worldInput.OnMoveCommandIssued += OnMoveCommand;

            ServiceLocator.Current
                .Get<MonoBehaviourMaster>()
                .update
                .Add(this);

            EventBus<SceneServicesClearEvent>.Register(
                new EventBinding<SceneServicesClearEvent>(() =>
                {
                    ServiceLocator.Current
                        .Get<MonoBehaviourMaster>()
                        .update
                        .Remove(this);

                    _worldInput.OnMoveCommandIssued -= OnMoveCommand;
                    _movementSystem.Dispose();
                }));
        }

        public void IUpdateClear(){}

        public void UpdateM()
        {
            if (!_initialized) return;
            
            Squad.Commands.Execute();
            _movementSystem.Tick(); // единственный вызов
        }

        public void OnMoveCommand(Vector3 targetPoint, WorldInputDispatcher.MoveMode mode)
        {
            EventBus<AudioUIEvent>.Raise(new AudioUIEvent(config.ToData()));
            _movementSystem.IssueMove(targetPoint, WorldInputDispatcher.MoveMode.Run); // всегда бегут (было mode)
        }
    }
}