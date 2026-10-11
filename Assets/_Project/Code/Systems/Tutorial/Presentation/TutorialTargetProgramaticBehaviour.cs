using Galactic1.Code.Systems.Tutorial.Authoring;

namespace Galactic1.Code.Systems.Tutorial.Presentation
{
    public class TutorialTargetProgramaticBehaviour : TutorialTargetBehaviour
    {

        public void Initialize(TutorialTargetId id)
        {
            targetId = id;
            ServiceLocator.Current.Get<TutorialTargetRegistry>().Register(this);

            EventBus<SceneServicesClearEvent>.Register(new EventBinding<SceneServicesClearEvent>(() =>
                ServiceLocator.Current.Get<TutorialTargetRegistry>().Unregister(this)));
        }


        protected override void OnEnable() {}
    }
}