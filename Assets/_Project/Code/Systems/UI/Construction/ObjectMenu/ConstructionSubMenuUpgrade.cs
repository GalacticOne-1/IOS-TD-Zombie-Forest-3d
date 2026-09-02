using Galactic1.Code.UI.Construction.Repair;
using UnityEngine;

namespace Galactic1.Code.UI.Construction
{
    public class ConstructionSubMenuUpgrade : MonoBehaviour
    {
        public GameObject switchMoveMenuButton;
        public GameObject deleteButton;
        
        [Header("Repair")]
        public GameObject repairButton;
        public RepairRequirementListView repairRequirementList;
    }
}