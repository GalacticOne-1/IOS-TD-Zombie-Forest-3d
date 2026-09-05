using System.Collections.Generic;
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Authoring
{
    /// <summary>
    /// Группа шагов, представляющая геймплейный домен (CAMP, WORLD_MAP, RAID...),
    /// а не Unity-сцену.
    /// </summary>
    [CreateAssetMenu(
        fileName = "TutorialChapter_",
        menuName = "Galactic1/Tutorial/Chapter")]
    public sealed class TutorialChapterDefinition : ScriptableObject
    {
        public TutorialChapterId chapterId;
        public List<TutorialStepDefinition> steps = new();

#if UNITY_EDITOR
        public bool Validate(out string error)
        {
            if (chapterId == null)
            {
                error = $"Chapter asset '{name}': chapterId is empty.";
                return false;
            }

            var seen = new HashSet<TutorialStepId>();
            foreach (var step in steps)
            {
                if (step == null)
                {
                    error = $"Chapter '{chapterId.DebugKey}': contains a null step reference.";
                    return false;
                }

                if (!seen.Add(step.stepId))
                {
                    error = $"Chapter '{chapterId.DebugKey}': duplicate stepId '{step.stepId?.DebugKey}'.";
                    return false;
                }

                if (!step.Validate(out error))
                    return false;
            }

            error = null;
            return true;
        }
#endif
    }
}
