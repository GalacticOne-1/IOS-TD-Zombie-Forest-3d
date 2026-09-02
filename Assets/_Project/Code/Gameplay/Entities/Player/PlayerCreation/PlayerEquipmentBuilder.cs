using UnityEngine;
using Galactic1.Gameplay.Player;

namespace Galactic1.Core.Systems.PlayerCreation
{
    public class PlayerEquipmentBuilder
    {
        private readonly PlayerController player;

        public PlayerEquipmentBuilder(PlayerController player)
        {
            this.player = player;
        }

        // public void Apply(EquipmentData equipment)
        // {
        //     if (player == null || equipment == null) return;
        //
        //     // Example: Attach prefabs to sockets on player rig
        //     AttachIfExists(equipment.headItemId, "Socket_Head");
        //     AttachIfExists(equipment.chestItemId, "Socket_Chest");
        //     AttachIfExists(equipment.legsItemId, "Socket_Legs");
        //
        //     // Backpack / inventory items may be loaded but not necessarily attached visually
        //     // TODO: query configs and set defense/stats
        // }

        private void AttachIfExists(string itemId, string socketName)
        {
            if (string.IsNullOrEmpty(itemId)) return;

            // For demo: assume item prefab located at Resources/Items/{itemId}
            var prefab = Resources.Load<GameObject>($"Items/{itemId}");
            if (prefab == null) return;

            var socket = player.transform.Find(socketName);
            if (socket == null)
            {
                Debug.LogWarning($"Socket {socketName} not found on player");
                return;
            }

            GameObject.Instantiate(prefab, socket, false);
        }
    }
}