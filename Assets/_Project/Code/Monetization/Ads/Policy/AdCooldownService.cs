
using System;
using System.Collections;
using Galactic1.Code.Core.Ads;
using Galactic1.Core;
using UnityEngine;

namespace Galactic1.Code.Systems.Ads
{
    /// <summary>
    /// Управляет кулдаунами по placement.
    /// </summary>
    public class AdCooldownService
    {
        private readonly Coroutines routine;
        
        
        private float cooldown;
        private bool cooldownActive;
        public event Action OnCooldownFinished;

        
        public AdCooldownService(Coroutines routine)
        {
            this.routine = routine;
        }



        public void SetCooldown(float seconds)
        {
            cooldown = UnityEngine.Time.time + seconds;

            if (!cooldownActive)
                routine.StartCoroutine(CooldownWatcher());

            cooldownActive = true;
        }

        private IEnumerator CooldownWatcher()
        {
            while (UnityEngine.Time.time < cooldown)
                yield return null;

            cooldownActive = false;
            OnCooldownFinished?.Invoke();
        }
        
        

        public bool IsOnCooldown(out float remaining)
        {
            remaining = Mathf.Max(0, cooldown - UnityEngine.Time.time);
            return remaining > 0;
        }
        
       
    }
}