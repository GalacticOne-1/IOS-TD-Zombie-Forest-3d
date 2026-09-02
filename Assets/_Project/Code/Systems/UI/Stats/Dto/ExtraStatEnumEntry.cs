namespace Galactic1.Game.UI.Stats.DTO
{
    public struct ExtraStatEnumEntry
    {
        public StatId StatId;
        public object RawEnum;

        public ExtraStatEnumEntry(StatId statId, object e)
        {
            StatId = statId;
            RawEnum = e;
        }
    }
}