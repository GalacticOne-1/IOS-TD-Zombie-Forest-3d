using Galactic1.Game.UI.Production.DTO;

namespace Galactic1.Game.UI.Production.Presenters
{
    /// <summary>
    /// Strategy для отображения блока деталей рецепта.
    /// Позволяет использовать разные представления (Craft / Recycler)
    /// без дублирования панели.
    /// </summary>
    public interface IRecipeDetailsPresenter
    {
        void Show(RecipeDetailsDto dto);
        void Clear();
    }
}