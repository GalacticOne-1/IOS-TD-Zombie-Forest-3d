namespace Galactic1.Code.Systems.Tutorial.Authoring
{
    /// <summary>Locked-режима нет намеренно — полная блокировка ручной камеры уже
    /// выражается через TutorialCapabilityPolicy.CanControlCamera (см. раздел 5 ТЗ
    /// camera bounds). Этот enum отвечает ТОЛЬКО за пространственное ограничение, когда
    /// камера разрешена.</summary>
    public enum TutorialCameraConstraintMode
    {
        None,
        Bounds
    }
}