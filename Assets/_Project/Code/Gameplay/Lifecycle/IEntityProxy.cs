namespace Galactic1.Code.Core.Lifecycle
{
    public interface IEntityProxy
    {
        int Priority { get; }
        void Register();
        void Unregister();
    }
}