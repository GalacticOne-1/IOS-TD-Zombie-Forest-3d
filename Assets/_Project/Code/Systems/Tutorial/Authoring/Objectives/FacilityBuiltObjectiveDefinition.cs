using UnityEngine;
using Galactic1.Code.GameDatabase.Registries;

namespace Galactic1.Code.Systems.Tutorial.Authoring.Objectives
{
    /// <summary>
    /// "Построй здание".
    /// itemId задан: если такое здание УЖЕ построено к старту шага — шаг завершается сразу
    /// (здания в основном одиночные, второй раз построить нельзя).
    /// itemId пуст: засчитывается только постройка ЛЮБОГО здания за время шага (event-семантика).
    /// </summary>
    [CreateAssetMenu(fileName = "Objective_FacilityBuilt",
        menuName = "Game Configs/Tutorial/Objectives/Facility Built")]
    public sealed class FacilityBuiltObjectiveDefinition : TutorialObjectiveDefinition
    {
        public override string ObjectiveTypeId => "FacilityBuilt";

        [Tooltip("Задан = завершается сразу, если это здание уже построено, иначе ждёт постройки " +
                 "(совпадает с FacilityModule.Item.Id). Пусто = засчитывается постройка ЛЮБОГО " +
                 "здания, но только за время шага.")]
        public ItemId itemId;
    }
}