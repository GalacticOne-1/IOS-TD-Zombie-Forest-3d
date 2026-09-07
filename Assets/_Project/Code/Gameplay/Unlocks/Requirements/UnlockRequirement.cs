using System;

namespace Galactic1.Code.Systems.Progression
{
    /// <summary>
    /// Polymorphic unlock requirement. Serialized via [SerializeReference] on
    /// ProgressionUnlockDefinition entries — same polymorphism pattern the
    /// project already uses for ItemModule (see ItemConfig.modules).
    ///
    /// Soft Launch ships exactly one concrete type: ProgressionLevelRequirement.
    /// Adding a new requirement type later means adding a new subclass here —
    /// UnlockService and ProgressionUnlockService never need to change.
    /// </summary>
    [Serializable]
    public abstract class UnlockRequirement
    {
        public abstract bool IsMet(IUnlockContext context);
    }
}
