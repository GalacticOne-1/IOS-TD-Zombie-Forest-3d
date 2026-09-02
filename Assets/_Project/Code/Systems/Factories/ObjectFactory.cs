using UnityEngine;

namespace Galactic1.Core.Systems.Factories
{
    /// <summary>
    /// Responsible for instantiating interactive objects on the scene:
    /// chests, doors, resource nodes, craft stations, traps, etc.
    /// Similar to LDoE BuildObjectFactory.
    /// </summary>
    // [CreateAssetMenu(menuName = "Game Configs/Factories/Object Factory")]
    // public class ObjectFactory : BaseFactory<InteractiveObject>
    // {
    //     public InteractiveObject Create(ObjectConfig config, Vector3 pos)
    //     {
    //         var obj = Instantiate(config.prefab, pos, Quaternion.identity);
    //         obj.Initialize(config);
    //         return obj;
    //     }
    //
    //     public override InteractiveObject Create(Vector3 position, Quaternion rotation)
    //     {
    //         return Instantiate(Resources.Load<InteractiveObject>("DefaultObject"), position, rotation);
    //     }
    // }
}