using System.Collections.Generic;
using Galactic1.Meta.Configs.Recruitment;

namespace Galactic1.Configs
{
    [System.Serializable]
    public struct RecruitAccess
    {
        public int tier;
        public int weight;
        public List<RecruitCategory> allowedCategories;
        
    }
}