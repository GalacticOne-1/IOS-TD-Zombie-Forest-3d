using UnityEngine;

namespace Galactic1.Code.Gameplay.Construction.States
{
    /// <summary>
    /// Базовый интерфейс состояния режима строительства
    /// </summary>
    public interface IConstructionState
    {
        void Enter();
        void Exit();

        void OnCellClicked(Vector2Int cell);
        void OnObjectClicked(BuildableObject obj);

        void OnConfirm();
        void OnCancel();
        void OnMove();
        void OnDelete();
        void OnRotation();
        void OnRepair();
    }
}