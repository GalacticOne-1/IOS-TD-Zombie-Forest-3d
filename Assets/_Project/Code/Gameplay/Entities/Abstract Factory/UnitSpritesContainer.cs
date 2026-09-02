using UnityEngine;

namespace Galactic1.AbstractFactory
{
    public class UnitSpritesContainer : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer head, torso, armLeft, armRight, legs;


        public SpriteRenderer Head => head;

        public SpriteRenderer Torso => torso;

        public SpriteRenderer ArmLeft => armLeft;

        public SpriteRenderer ArmRight => armRight;

        public SpriteRenderer Legs => legs;
    }
}