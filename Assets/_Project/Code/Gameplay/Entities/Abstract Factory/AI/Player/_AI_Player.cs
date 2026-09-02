using Galactic1;
using Gameplay;
using UnityEngine;

namespace Galactic1.AbstractFactory
{
    public abstract class _AI_Player : _AI
    {
        protected _AI_Player(_Entity entity) : base(entity)
        {
        }


        protected override bool FindTarget()
        {
            cashTarget = null;
            //cashTarget = new TRGT_Player(Entity.CenterRadius, Entity._feature.GetAttribute(StatId.DetectRange)).FindTarget();
        
            if (cashTarget == null)
            {
                Entity.Log(new CEntityDebugParam()
                {
                    Message = "Player : Target null",
                    Color = EDlogColor.YELLOW
                });
                return false;
            }
            
            Entity.Target.NewTarget(cashTarget);
            return true;
        }
    }
}