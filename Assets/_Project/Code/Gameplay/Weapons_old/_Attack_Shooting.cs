
namespace Galactic1.AbstractFactory
{
    public abstract class _Attack_Shooting : _Attack
    {
        
        
        
        
        protected void Shot_Regular(byte layer)
        {
            // cashBullet = Pool_OLD.I.GetBullet(modeShell.bullet);
            //
            // if (cashBullet)
            // {
            //     cashBullet.transform.position = modeShell.bar.transform.position;
            //
            //     direction = _attack.Entity._target.ITarget.HitCoord() - modeShell.bar.transform.position;
            //     /*cashBullet.transform.rotation = use_accuracy
            //         ? Accuracy()
            //         : Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f);*/
            //     cashBullet.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f);
            //
            //     //if (buff != null)
            //         //cashBullet.GetComponent<Granade>().param.buff = buff.Copy();
            //
            //     cashBullet.GetComponent<BulletABS>().CMD_ACTIVATE(new CBulletData()
            //     {
            //         layer = layer,
            //         status = EStatusDamage.bullet,
            //         damage = usedDamage,
            //         owner = _attack.Entity.gameObject,
            //         targetCoord = _attack.Entity._target.ITarget.HitCoord(),
            //         target = _attack.Entity._target.ITarget.Obj
            //     });
            // }
        }
        
        protected void Shot_Regular(byte layer, float rotateOffset)
        {
            // cashBullet = Pool_OLD.I.GetBullet(modeShell.bullet);
            //
            // if (cashBullet)
            // {
            //     cashBullet.transform.position = modeShell.bar.transform.position;
            //
            //     direction = _attack.Entity._target.ITarget.HitCoord() - modeShell.bar.transform.position;
            //     cashBullet.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f + rotateOffset);
            //     DLog.Alert("Rotate offset : "+rotateOffset);
            //     
            //     //if (buff != null)
            //     //cashBullet.GetComponent<Granade>().param.buff = buff.Copy();
            //
            //     cashBullet.GetComponent<BulletABS>().CMD_ACTIVATE(new CBulletData()
            //     {
            //         layer = layer,
            //         status = EStatusDamage.bullet,
            //         damage = usedDamage,
            //         owner = _attack.Entity.gameObject,
            //         targetCoord = _attack.Entity._target.ITarget.HitCoord(),
            //         target = _attack.Entity._target.ITarget.Obj
            //     });
            // }
        }
        
        
        protected void Shot_AoE(byte layer)
        {
            // cashBullet = Pool_OLD.I.GetBullet(modeShell.bullet);
            //
            // if (cashBullet)
            // {
            //     
            // }
        }
    }
}