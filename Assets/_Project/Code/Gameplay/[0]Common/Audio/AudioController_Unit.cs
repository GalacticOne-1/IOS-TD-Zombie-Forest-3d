using System;
using Galactic1;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Galactic1
{
    // базвовый класс для всех объектов на уровне
    // управление 3D звуком на юнитe
    public abstract class AudioController_Unit : MonoBehaviour
    {
        // id game[]
        [SerializeField] protected int attackID, 
            dieID = -1;
        
        
        public CAudioUnit attack, die, spawn;

        //protected AudioSource source;
        
        
        bool attackActive;

        
        
        

        private void Awake()
        {
            //source = GetComponent<AudioSource>();
            //source.outputAudioMixerGroup = AudioController.I.master.FindMatchingGroups("Game")[0];
            //source.volume *= GameParam.I.gameplaySoundMultiply;
        }


        public void PlaySound(EUnitClip clip)
        {
            switch (clip)
            {
                case EUnitClip.die:
                    if (dieID != -1)
                        Die();
                    break;
                
                case EUnitClip.attack:
                    //AudioController.I.SoundDelay_Attack(attackID);
                    //source.clip = attack.clip;
                    //source.volume = attack.volume;
                    //attackActive = AudioController.I.Sound_Shoot(source, attackActive);
                    return;
                
                case EUnitClip.spawn:
                    Spawn();
                    break;
                
                case EUnitClip.portal:
                    
                    break;
            }
            
           // AudioController.I.Sound_Queue(source);
        }


        protected abstract void Die();
        protected abstract void Spawn();


        public void ResetSoundAttack()
        {
            if (!attackActive) return;
            attackActive = false;
            //AudioController.I.Sound_ShootReset(source);
        }
    }

    [System.Serializable]
    public struct CAudioUnit
    {
        public AudioClip clip;
        public float volume;
    }

    public enum EUnitClip
    {
        die, attack, spawn, portal
    }
}