using TMPro;
using UnityEngine;

namespace Galactic1.Game.UI.Production
{
    public class CraftDetailsStorageView : MonoBehaviour
    {
        [SerializeField] private TMP_Text campText;
        [SerializeField] private TMP_Text transportText;
        [SerializeField] private TMP_Text inboxText;
        [SerializeField] private TMP_Text ordersText;


        public TMP_Text CampText => campText;

        public TMP_Text TransportText => transportText;

        public TMP_Text InboxText => inboxText;

        public TMP_Text OrdersText => ordersText;
    }
}