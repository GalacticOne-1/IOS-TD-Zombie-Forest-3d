using System.Collections.Generic;

namespace Galactic1.Code.Gameplay.Interaction
{
    public class UIInputExceptionRegistry
    {
        
        public readonly HashSet<string> UiExceptionTags = new();
        
        
        public void AddUIInputExceptionTag(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                return;

            UiExceptionTags.Add(tag);
        }

        public void RemoveUIInputExceptionTag(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                return;

            UiExceptionTags.Remove(tag);
        }
        
        public void ClearUIExceptionTags()
        {
            UiExceptionTags.Clear();
        }
    }
}