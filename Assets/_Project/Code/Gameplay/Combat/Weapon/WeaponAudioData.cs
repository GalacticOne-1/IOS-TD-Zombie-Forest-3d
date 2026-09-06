namespace Galactic1.Code.Gameplay.Audio.Weapons
{
    public sealed class WeaponAudioData
    {
        public readonly AudioCueData Fire;
        public readonly AudioCueData ReloadStart;
        public readonly AudioCueData ReloadComplete;
        public readonly AudioCueData Empty;
        public readonly AudioCueData Overheat;
        public readonly AudioCueData Broken;

        public readonly int Priority;

        public WeaponAudioData(
            AudioCueData fire,
            AudioCueData reloadStart,
            AudioCueData reloadComplete,
            AudioCueData empty,
            AudioCueData overheat,
            AudioCueData broken,
            int priority)
        {
            Fire = fire;
            ReloadStart = reloadStart;
            ReloadComplete = reloadComplete;
            Empty = empty;
            Overheat = overheat;
            Broken = broken;

            Priority = priority;
        }
    }
}