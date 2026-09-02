namespace Galactic1
{
    public interface ICommandProcessor
    {
        void RegisterHandle<TCommand>(ICommandHandler<TCommand> handler) where TCommand : ICommand;
        bool Process<TCommand>(TCommand command) where TCommand : ICommand;
    }
}