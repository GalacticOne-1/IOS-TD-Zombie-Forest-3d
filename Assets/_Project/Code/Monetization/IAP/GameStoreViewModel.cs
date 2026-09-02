namespace Galactic1.UI.Shop
{
    public class GameStoreViewModel
    {
        public readonly GameStoreService _gameStoreService;

        public GameStoreViewModel(GameStoreService gameStoreService)
        {
            _gameStoreService = gameStoreService;
        }
    }
}