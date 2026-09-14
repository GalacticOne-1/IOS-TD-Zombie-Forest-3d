using Galactic1.Code.Systems.Tutorial.Authoring;

namespace Galactic1.Code.Systems.Tutorial.Presentation
{
    /// <summary>Живой поиск "какой юнит в текущем видимом списке (InventoryManagementController.
    /// VisibleUnits) подходит под критерии" — в отличие от ITutorialItemSlotTargetProvider
    /// (lookup по ItemId), здесь нет ключа для lookup вообще: юниты идентифицируются только
    /// рантайм-guid'ом (UnitIdentity.Id), authoring не может на него сослаться заранее.
    /// Первое совпадение по порядку списка побеждает — тот же "first match" принцип, что
    /// у AllOf/AnyOf и transitions.</summary>
    public interface ITutorialUnitSlotTargetProvider
    {
        bool TryGetUnitTarget(TutorialUnitSearchCriteria criteria, out ITutorialTarget target);
    }
}