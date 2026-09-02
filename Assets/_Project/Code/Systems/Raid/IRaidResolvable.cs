namespace Galactic1.Code.Systems.Raid
{
    /// <summary>
    /// Контракт объекта, который может быть
    /// разрешён (resolved) после рейда обратно в meta-runtime.
    /// </summary>
    public interface IRaidResolvable
    {
        string Id { get; }

        void ApplyToMeta(object metaRuntime);
    }
}