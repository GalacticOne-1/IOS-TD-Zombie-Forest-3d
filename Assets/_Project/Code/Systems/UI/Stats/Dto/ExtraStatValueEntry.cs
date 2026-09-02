namespace Galactic1.Game.UI.Stats.DTO
{
    public struct ExtraStatValueEntry
    {
        public StatId StatId;
        public float Value;

        public ExtraStatValueEntry(StatId statId, float value)
        {
            StatId = statId;
            Value = value;
        }
    }
}