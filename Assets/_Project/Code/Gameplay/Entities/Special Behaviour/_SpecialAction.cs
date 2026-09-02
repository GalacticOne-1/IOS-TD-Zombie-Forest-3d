using UnityEngine;

namespace Gameplay.AbstractFactory
{
    public abstract class _SpecialAction : MonoBehaviour,  ISpecialAction
    {
        public abstract void Register();
    }

    public interface ISpecialAction
    {
        void Register();
    }
}