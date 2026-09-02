using Galactic1.Game.UI.Production.DTO;

namespace Galactic1.Game.UI.Production.Presenters
{
    /// <summary>
    /// Presenter для обычной Craft станции.
    /// </summary>
    public sealed class CraftRecipeDetailsPresenter : IRecipeDetailsPresenter
    {
        private readonly CraftDetailsView _view;

        public CraftRecipeDetailsPresenter(CraftDetailsView view)
        {
            _view = view;
        }

        public void Show(RecipeDetailsDto dto)
        {
            _view.ShowDetails(dto);
            _view.gameObject.SetActive(true);
        }

        public void Clear()
        {
            _view.gameObject.SetActive(false);
            _view.Release();
        }
    }
}