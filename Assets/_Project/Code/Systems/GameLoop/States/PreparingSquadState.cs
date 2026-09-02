namespace Galactic1.Code.Systems.GameLoop.States
{
    public sealed class PreparingSquadState : GameLoopStateBase
    {
        public override GameLoopState Id => GameLoopState.PreparingSquad;
        
        public PreparingSquadState(DIContainer container) : base(container) {}

        public override void Enter(GameLoopContext context)
        {
            base.Enter(context);
            //context.CurrentSquad = _service.BuildSquad();
            //_sm.ChangeState(GameLoopState.WorldMap);
        }

        public override void Exit(GameLoopContext context) { }
    }

}