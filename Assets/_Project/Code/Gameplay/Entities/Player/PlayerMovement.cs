using UnityEngine;

namespace Galactic1.Gameplay.Player
{
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private float speed = 3f;
        private Vector2 moveVector;

        public void SetMoveVector(Vector2 vector)
        {
            moveVector = vector;
        }

        void Update()
        {
            var dir = new Vector3(moveVector.x, 0, moveVector.y);
            transform.position += dir * speed * Time.deltaTime;
        }
    }
}