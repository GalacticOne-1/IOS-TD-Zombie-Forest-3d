using Galactic1;
using UnityEngine;

namespace Galactic1
{
    public class Impact_update : Impact, IUpdate
    {
        
        /*
         *  Не протестировано   !!!!
         */
        
        
        protected override void OnEnable()
        {
            t = 0;
            ServiceLocator.Current.Get<MonoBehaviourMaster>().update.Add(this);
            //GameSetup.I.onResetUpdate += IUpdateClear;
        }


        public void IUpdateClear()
        {
            ServiceLocator.Current.Get<MonoBehaviourMaster>().update.Remove(this);
        }


        private float t;
        public void UpdateM()
        {
            t += Time.deltaTime;
            if (t > wait)
            {
                Hide();
                IUpdateClear();
            }
        }
    }
}