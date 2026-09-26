using Galactic1.Code.Systems.Tutorial.Authoring;
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Presentation
{
    /// <summary>Scene bridge — прямоугольная XZ-область для ограничения ручной камеры на
    /// конкретном tutorial-шаге. Тот же OnEnable/OnDisable регистрационный lifecycle, что
    /// TutorialTargetBehaviour/WorldTutorialTargetBehaviour уже используют для
    /// TutorialTargetRegistry — здесь только другой registry (TutorialCameraBoundsRegistry).
    /// Дизайнер ставит объект в сцену, растягивает Size в инспекторе/через Gizmos.</summary>
    public sealed class TutorialColliderStepBound : WorldTutorialTargetBehaviour
    {


#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.94f, 0.3f, .35f, 0.3f);
            Gizmos.DrawCube(transform.position, transform.localScale);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.94f, 0.3f, .35f, 0.9f);
            Gizmos.DrawWireCube(transform.position, transform.localScale);
        }
#endif
    }
}