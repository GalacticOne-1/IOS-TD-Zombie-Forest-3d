namespace Galactic1.Code.Systems.Tutorial.Runtime
{
    /// <summary>
    /// Composable tutorial input capability policy — раздел 10.1 ТЗ Chapter 2.
    ///
    /// Существующий TutorialInputMode (Free/Restricted/RequiredAction/Blocked) слишком
    /// coarse-grained для Chapter 2: во время Molotov-шага нужно ОДНОВРЕМЕННО заблокировать
    /// движение и ручную камеру, но оставить доступными interact/abilities с разной
    /// гранулярностью, чего единый enum-режим не выражает. Это ОТДЕЛЬНЫЙ, параллельный
    /// TutorialInputMode механизм — не замена, не расширение InteractionPolicyService
    /// (тот остаётся политикой именно world-interactions: facilities/loot/NPC/vehicles).
    ///
    /// Единственный источник истины — TutorialInputPolicyService.ApplyCapabilities(...),
    /// вызываемый из TutorialService.ActivateStep. WorldInputDispatcher/CameraController
    /// читают этот объект lazy через ServiceLocator (тот же паттерн, что уже используют
    /// TutorialUnitSlotTargetProvider/ConstructionFacilitySlotTargetProvider для резолва
    /// scene-сервисов) — без прямой DI-зависимости gameplay-классов от Tutorial-системы.
    /// </summary>
    public sealed class TutorialCapabilityPolicy : IGameService
    {
        public bool CanMove { get; private set; } = true;
        public bool CanControlCamera { get; private set; } = true;
        public bool CanInteract { get; private set; } = true;
        public bool CanUseAbilities { get; private set; } = true;

        public void Set(bool canMove, bool canControlCamera, bool canInteract, bool canUseAbilities)
        {
            CanMove = canMove;
            CanControlCamera = canControlCamera;
            CanInteract = canInteract;
            CanUseAbilities = canUseAbilities;
        }

        public void Reset() => Set(true, true, true, true);
    }
}
