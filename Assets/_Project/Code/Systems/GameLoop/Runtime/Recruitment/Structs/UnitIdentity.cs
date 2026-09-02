
namespace Galactic1.Code.Systems.Runtime
{
    /// <summary>
    /// Уникальная идентичность юнита.
    /// Value-object.
    /// </summary>
    [System.Serializable]
    public sealed class UnitIdentity
    {
        public string Id { get; }
        public string DisplayName { get; }
        public string ArchetypeId { get; }
        public long CreatedAtTicks { get; }

        public UnitIdentity(string id, string displayName, string archetypeId, long createdAtTicks)
        {
            Id = id;
            DisplayName = displayName;
            ArchetypeId = archetypeId;
            CreatedAtTicks = createdAtTicks;
        }

        public override string ToString()
        {
            return $"{DisplayName} ({Id})";
        }
    }
}