using System;
using UnityEngine;

namespace Galactic1
{
    public class RagdollColliderIgnore : MonoBehaviour
    {
        private void Start()
        {
            var colliders = GetComponentsInChildren<Collider2D>();
            var l = colliders.Length;
            for (int i = 0; i < l; i++)
            {
                for (int j = i+1; j < l; j++)
                {
                    Physics2D.IgnoreCollision(colliders[i], colliders[j]);
                }
            }
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            Physics2D.IgnoreCollision(gameObject.GetComponent<Collider2D>(), other.gameObject.GetComponent<Collider2D>());
        }
    }
}