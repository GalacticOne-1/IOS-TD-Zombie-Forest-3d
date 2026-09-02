using UnityEngine;

namespace Galactic1.PoolObject
{
    public class SfxPlayer : MonoBehaviour
    {
        public void Play(AudioClip clip, Vector3 position)
        {
            AudioSource.PlayClipAtPoint(clip, position); // Можно расширить с пулом источников
        }
    }

}