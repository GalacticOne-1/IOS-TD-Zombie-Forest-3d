using Galactic1;

namespace Galactic1
{
    public static class CampViewModelsRegistrations
    {
        public static void Register(DIContainer container)
        {
            //container.RegisterFactory(c => new CampUIManager(container)).AsSingle();
            //container.RegisterFactory(c => new UICampRootViewModel()).AsSingle();
            container.RegisterFactory(c => new CampRootViewModel(
                c.Resolve<StructureService>()
            )).AsSingle();
        }
    }
}