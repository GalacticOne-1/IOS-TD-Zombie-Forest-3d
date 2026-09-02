using UnityEngine;

namespace Galactic1.Gameplay.Interaction.Objects
{
    public enum ContainerType
    {
        InstantOpen,
        TimedOpen,
        CodeLocked,
        Corpse
    }

    [System.Serializable]
    public class ContainerConfig
    {
        public ContainerType type;

        public float openTime;
        public bool requiresProgressBar;

        public string correctCode;
    }
}