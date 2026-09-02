using Galactic1;
using Gameplay;
using UnityEngine;

namespace Galactic1.AbstractFactory
{
    public abstract class _AI_Enemy : _AI
    {
        public _AI_Enemy(_Entity entity) : base(entity)
        {
        }


        


        protected override bool FindTarget()
        {
            cashTarget = null;
            //cashTarget = new TRGT_Enemy(Entity.tr, Entity._feature.GetAttribute(StatId.DetectRange)).FindTarget();
        
            if (cashTarget == null)
            {
                Entity.Log(new CEntityDebugParam()
                {
                    Message = "Enemy : Target null",
                    Color = EDlogColor.YELLOW
                });
                return false;
            }
            
            Entity.Target.NewTarget(cashTarget);
            return true;
        }
    }
}