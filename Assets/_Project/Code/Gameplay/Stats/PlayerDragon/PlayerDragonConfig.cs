using UnityEngine;

namespace Galactic1
{
    [CreateAssetMenu(fileName = "PlayerDragonConfig", menuName = "Game Configs/Player/Player Dragon Config")]
    public class PlayerDragonConfig : ScriptableObject
    {
        [field:SerializeField] public GameObject Prefab {get; private set;}


    }
}