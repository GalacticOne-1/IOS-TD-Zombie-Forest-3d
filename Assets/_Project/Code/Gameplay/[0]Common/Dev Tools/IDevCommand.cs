namespace Galactic1.Dev
{
    public interface IDevCommand
    {
        string Name { get; }
        string Description { get; }
        void Execute(string[] args);
    }

}