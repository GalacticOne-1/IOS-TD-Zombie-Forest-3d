using Galactic1.Code.Systems.Tutorial.Authoring;

namespace Galactic1.Code.Systems.Tutorial.Runtime
{
    /// <summary>Runtime-состояние input-политики тутора. Только данные.</summary>
    public sealed class TutorialInputPolicy
    {
        public TutorialInputMode Mode { get; set; } = TutorialInputMode.Free;
        public TutorialTargetId RequiredTargetId { get; set; }

        public void Reset()
        {
            Mode = TutorialInputMode.Free;
            RequiredTargetId = null;
        }
    }
}
