using Galactic1.AbstractFactory;
using Galactic1.Code.Gameplay.World.Repositories;

namespace Galactic1.Code.Gameplay.World
{
    public abstract class WorldInstance : _Entity
    {
        protected override void OnEnable()
        {
            ServiceLocator.Current.Get<WorldObjectRepository>().Register(UniqueId,this);
        }

        protected override void OnDisable()
        {
            ServiceLocator.Current.Get<WorldObjectRepository>().Unregister(UniqueId,this);
        }
    }
}