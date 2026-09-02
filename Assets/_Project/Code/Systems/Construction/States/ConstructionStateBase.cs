using UnityEngine;

namespace Galactic1.Code.Gameplay.Construction.States
{
    /// <summary>
    /// Базовый класс состояния режима строительства
    /// </summary>
    public abstract class ConstructionStateBase : IConstructionState
    {
        protected readonly ConstructionModeController controller;
        protected readonly ConstructionPlacementController placement;

        protected ConstructionStateBase(
            ConstructionModeController controller,
            ConstructionPlacementController placement)
        {
            this.controller = controller;
            this.placement = placement;
        }

        public virtual void Enter() { }

        public virtual void Exit()
        {
            controller.HideObjectMenu();
        }

        public virtual void OnCellClicked(Vector2Int cell) { }
        public virtual void OnObjectClicked(BuildableObject obj) { }

        public virtual void OnConfirm() { }
        public virtual void OnCancel() { }
        public virtual void OnMove() { }
        public virtual void OnRotation() { }
        public virtual void OnDelete() { }
        public virtual void OnRepair() { }
    }
}