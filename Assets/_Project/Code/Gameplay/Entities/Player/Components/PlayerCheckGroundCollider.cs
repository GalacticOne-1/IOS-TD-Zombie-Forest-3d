
using UnityEngine;

namespace Galactic1.Gameplay.Player
{
    public class PlayerCheckGroundCollider : MonoBehaviour
    {
        private int grounds;
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            grounds++;
            ServiceLocator.Current.Get<HeroStateMachine>().Current.IsGrounded();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            grounds--; 
            if (grounds <= 0)
            {
                grounds = 0;
                ServiceLocator.Current.Get<HeroStateMachine>().Current.IsFalling();
            }
        }
    }
}