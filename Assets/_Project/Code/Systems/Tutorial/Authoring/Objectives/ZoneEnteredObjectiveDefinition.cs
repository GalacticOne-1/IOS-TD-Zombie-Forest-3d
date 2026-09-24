
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Authoring.Objectives
{
    /// <summary>Двухфазный объектив: 1) игрок входит в зону targetId, 2) запускается
    /// вложенный thenObjective, его завершение завершает этот объектив.</summary>
    [CreateAssetMenu(fileName = "Objective_ZoneEntered",
        menuName = "Game Configs/Tutorial/Objectives/Zone Entered")]
    public sealed class ZoneEnteredObjectiveDefinition : TutorialObjectiveDefinition
    {
        public override string ObjectiveTypeId => "ZoneEntered";

        [Tooltip("Совпадает с targetId на TutorialZoneTrigger в сцене.")]
        public TutorialTargetId targetId;

        [Tooltip("Реальное задание, стартует ПОСЛЕ входа в зону. Пусто = объектив " +
                 "завершается самим входом в зону.")]
        public TutorialObjectiveDefinition thenObjective;

#if UNITY_EDITOR
        public override bool Validate(out string error)
        {
            if (targetId == null)
            {
                error = "ZoneEnteredObjectiveDefinition: targetId is empty.";
                return false;
            }

            // thenObjective == null допустим: вход в зону = выполнено.
            if (thenObjective == null)
            {
                error = null;
                return true;
            }

            // защита от циклов: A -> ... -> A
            var cur = thenObjective;
            int guard = 0;
            while (cur is ZoneEnteredObjectiveDefinition z)
            {
                if (z == this || ++guard > 16)
                {
                    error = "ZoneEnteredObjectiveDefinition: cyclic nesting of thenObjective.";
                    return false;
                }

                cur = z.thenObjective;
                if (cur == null)
                {
                    error = null; // вложенная цепочка заканчивается пустым звеном — это допустимо
                    return true;
                }
            }

            return thenObjective.Validate(out error);
        }
#endif
    }
}