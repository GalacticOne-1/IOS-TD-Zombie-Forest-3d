using UnityEngine;

namespace Galactic1.Core.Systems.Factories
{
    /// <summary>
    /// Spawns NPC characters using NpcConfig.
    /// Used for traders, quest givers, townsfolk.
    /// </summary>
    // [CreateAssetMenu(menuName = "Game Configs/Factories/Npc Factory")]
    // public class NpcFactory : BaseFactory<NpcController>
    // {
    //     [SerializeField] private NpcController npcPrefab;
    //
    //     public NpcController Create(NpcConfig config, Vector3 pos)
    //     {
    //         var npc = Instantiate(npcPrefab, pos, Quaternion.identity);
    //         npc.ApplyConfig(config);
    //         return npc;
    //     }
    //
    //     public override NpcController Create(Vector3 position, Quaternion rotation)
    //     {
    //         return Instantiate(npcPrefab, position, rotation);
    //     }
    // }
}