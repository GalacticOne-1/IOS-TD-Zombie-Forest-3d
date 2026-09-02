
using Galactic1.Code.Gameplay.Enemies;

namespace DEV
{
    public class DevZombieTargetInfo : ZombieTargetInfo
    {
        private DevZombieInstance instance;
        public override bool IsDead => instance.Hp <= 0;

        protected void Awake()
        {
            instance = GetComponent<DevZombieInstance>();
        }

        
    }
}