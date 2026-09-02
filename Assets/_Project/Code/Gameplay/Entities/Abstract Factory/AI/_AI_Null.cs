using UnityEngine;

namespace Galactic1.AbstractFactory
{
    public class _AI_Null : _AI
    {
        public _AI_Null(_Entity entity) : base(entity) {}

        protected override void LogicUpdate() {}

        protected override bool FindTarget() => false;
    }
}