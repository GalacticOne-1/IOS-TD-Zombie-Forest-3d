namespace Galactic1
{
    public static class GameplayViewModelsRegistrations
    {
        public static void Register(DIContainer container)
        {
            container.RegisterFactory(c => new UIGameplayRootViewModel(/*c.Resolve<CheatsService>()*/)).AsSingle();
        }
    }
}