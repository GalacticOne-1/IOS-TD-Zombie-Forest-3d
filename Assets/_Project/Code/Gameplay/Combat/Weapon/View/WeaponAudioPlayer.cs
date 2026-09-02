
using Galactic1.Code.Gameplay.Weapons.Logic;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Weapons.View
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class WeaponAudioPlayer : MonoBehaviour
    {
        private AudioSource _source;
        private WeaponEntity _entity;
        private WeaponDefinitionData _definition;

        // public void Bind(WeaponEntity entity, WeaponDefinitionData definitionData)
        // {
        //     _entity = entity;
        //     _definition = definitionData;
        //     _source ??= GetComponent<AudioSource>();
        //
        //     entity.OnFired += _ => Play(_definition.FireSfx);
        //     entity.OnReloadStarted += () => Play(_definition.ReloadSfx);
        //     entity.OnOverheated += () => Play(_definition.OverheatSfx);
        // }
        //
        // public void Unbind()
        // {
        //     if (_entity == null) return;
        //
        //     _entity.OnFired -= _ => Play(_definition.FireSfx);
        //     _entity.OnReloadStarted -= () => Play(_definition.ReloadSfx);
        //     _entity.OnOverheated -= () => Play(_definition.OverheatSfx);
        // }
        //
        // public void PlayEmpty() => Play(_definition.EmptySfx);
        //
        // private void Play(AudioClip clip)
        // {
        //     if (clip != null)
        //         _source.PlayOneShot(clip);
        // }
    }
}