using System;
using Galactic1.Code.Gameplay.Interaction;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Targeting
{
    public interface IWorldPointerService
    {
        UIInputExceptionRegistry UIInputExceptionRegistry { get; }
        event Action<WorldPointerHit, WorldPointerHit> OnPointerDown;
        event Action<WorldPointerHit, WorldPointerHit> OnPointerDrag;
        event Action<WorldPointerHit, WorldPointerHit> OnPointerUp;
        event Action OnCancel;
        bool TryGetWorld(out WorldPointerHit groundHit, out WorldPointerHit anyHit);
    }
}