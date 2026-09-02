using Galactic1.Code.UI.Buildings;
using Galactic1.Game.UI.Buildings.DTO;
using Galactic1.Code.Systems.Inbox;
using Galactic1.Code.Systems.Runtime.Building;
using Galactic1.UI.Core;
using UnityEngine;

namespace Galactic1.Game.UI.Inbox
{
    /// <summary>
    /// Панель входящих предметов (Inbox).
    /// </summary>
    public class InboxPanelModule : FacilityPanelModule
    {
        [SerializeField] private InboxListView listView;

        private InboxSceneAdapter _adapter;

        bool opened = false;
        
        
        public override bool IsSupported(FacilityDTO dto)
            => dto.Details.Type == FacilityType.MainContainer;

        public override void Bind(FacilityDTO dto, object sceneAdapter, FacilityUpgradeSceneAdapter upgradeAdapter)
        {
            base.Bind(dto, sceneAdapter, upgradeAdapter);

            _adapter = sceneAdapter as InboxSceneAdapter;

            listView.OnTakeClicked += HandleTake;

            opened = true;
            Rebind(dto);
        }

        public override void Unbind()
        {
            base.Unbind();

            if (_adapter != null)
            {
                listView.OnTakeClicked -= HandleTake;
                listView.Clear();
            }
        }

        public override void Rebind(FacilityDTO dto)
        {
            var details = dto.Details as InboxModuleDetailsDTO;
            listView.Build(details.Slots, opened);
            opened = false;
        }

        private void HandleTake(string slotId)
        {
            var success = _adapter.TryClaimSlot(slotId);
            
            if (!success)
            {
                // инвентарь полон — показать уведомление
                ServiceLocator.Current.Get<UIManager>().OpenPopup(
                    UIScreenId.AdAlertToast,
                    "Not enough space in storage!");
            }
        }
    }
}