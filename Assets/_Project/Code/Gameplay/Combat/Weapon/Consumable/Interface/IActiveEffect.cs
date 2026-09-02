namespace Galactic1.Code.Gameplay.Effect
{
    public interface IActiveEffect
    {
        bool IsFinished { get; }
        void Tick(float dt);
    }
}