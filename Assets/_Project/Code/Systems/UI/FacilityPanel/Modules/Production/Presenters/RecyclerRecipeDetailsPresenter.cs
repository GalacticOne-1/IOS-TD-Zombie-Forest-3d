using Galactic1.Game.UI.Production.DTO;

namespace Galactic1.Game.UI.Production.Presenters
{
    /// <summary>
    /// Presenter для Recycler станции.
    /// Отличается отображением Main Info (несколько output ресурсов).
    /// </summary>
    public sealed class RecyclerRecipeDetailsPresenter : IRecipeDetailsPresenter
    {
        private readonly RecyclerDetailsView _view;

        public RecyclerRecipeDetailsPresenter(RecyclerDetailsView view)
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
        }
    }
}