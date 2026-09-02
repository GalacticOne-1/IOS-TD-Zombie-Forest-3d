namespace Galactic1.Code.Systems.GameLoop.States
{
    public abstract class GameLoopStateBase : IGameLoopState
    {
        public abstract GameLoopState Id { get; }

        
        protected DIContainer _container;
        protected GameLoopContext _context;


        
        protected GameLoopStateBase(DIContainer container)
        {
            _container = container;
        }

        public virtual void Enter(GameLoopContext context)
        {
            _context = context;
        }

        public abstract void Exit(GameLoopContext context);
    }
}