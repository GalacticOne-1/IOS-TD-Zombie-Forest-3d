
namespace Galactic1.RaidLoot.Runtime
{
    /// <summary>
    /// Запись об истощении контейнера.
    /// Хранит количество открытий — не бинарный флаг.
    /// </summary>
    public sealed class ContainerDepletionRecord
    {
        public string RuntimeId { get; }
        public int OpenCount { get; private set; }
        public int DayFirstLooted { get; }

        public ContainerDepletionRecord(string id, int day)
        {
            RuntimeId = id;
            DayFirstLooted = day;
            OpenCount = 0;
        }

        public void RegisterOpen() => OpenCount++;
    }
}