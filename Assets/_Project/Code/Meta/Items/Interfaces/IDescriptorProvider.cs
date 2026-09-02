using System.Collections.Generic;
using Galactic1.Code.Items;

namespace Galactic1.Game.Meta.Items
{
    /// <summary>
    /// Allows module to provide UI descriptors.
    /// </summary>
    public interface IDescriptorProvider
    {
        void CollectDescriptors(List<DescriptorDisplayEntry> list);
    }
}