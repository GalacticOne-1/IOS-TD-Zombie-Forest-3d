namespace Galactic1.Code.Systems.Interaction
{
    /// <summary>
    /// Центральный runtime-сервис политики взаимодействий.
    ///
    /// Не принимает решений.
    /// Не знает про сценарии.
    /// Не знает про UI.
    ///
    /// Хранит текущее состояние доступности различных
    /// игровых взаимодействий.
    /// </summary>
    public sealed class InteractionPolicyService : IGameService
    {
        private readonly InteractionPolicy _policy = new();

        /// <summary>
        /// Текущая runtime-политика.
        /// </summary>
        public InteractionPolicy Policy => _policy;

        public bool CanInteractWithFacilities =>
            _policy.CanInteractWithFacilities;

        public bool CanInteractWithLoot =>
            _policy.CanInteractWithLoot;

        public bool CanInteractWithNpc =>
            _policy.CanInteractWithNpc;

        public bool CanInteractWithVehicles =>
            _policy.CanInteractWithVehicles;

        public bool CanInteractWithWorldObjects =>
            _policy.CanInteractWithWorldObjects;

        /// <summary>
        /// Возвращает политику к состоянию по умолчанию.
        /// </summary>
        public void Reset()
        {
            _policy.Reset();
        }

        /// <summary>
        /// Полностью запрещает любые взаимодействия.
        /// </summary>
        public void DisableAll()
        {
            _policy.DisableAll();
        }
    }
}