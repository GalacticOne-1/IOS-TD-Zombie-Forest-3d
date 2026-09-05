using UnityEngine;

namespace Galactic1.Systems
{
    
    /// <summary>
    /// Фасад над AudioManager. Управляет музыкой и звуковыми эффектами через категориальные команды.
    /// </summary>
    public static class AudioService
    {
        // ===================== Категории звуков =====================
        public static class UI
        {
            public enum Command
            {
                ButtonClick,
                ButtonHover,
                UIOpen,
                UIClose,
                Notification
            }
        }

        public static class Player
        {
            public enum Command
            {
                Jump,
                Land,
                Attack,
                Hurt,
                Death,
                WalkStep,
                RunStep_1,
                RunStep_2,
                ClingClimb,
                DragonFly,
                
                WeaponReloadClip,
                WeaponUnloadClip
            }
        }

        public static class Enemy
        {
            public enum Command
            {
                Attack,
                Hurt,
                Death,
                Spawn
            }
        }

        public static class Environment
        {
            public enum Command
            {
                DoorOpen,
                DoorClose,
                ChestOpen,
                ChestClose,
                ItemPickup,
                ItemDrop,
                Explosion,
                WaterSplash,
                FireIgnite,
                FireExtinguish,
                FootstepGravel,
                FootstepWood,
                FootstepMetal
            }
        }

        public static class Special
        {
            public enum Command
            {
                MagicCast,
                MagicHit,
                MagicImpact,
                TrapTrigger,
                Alarm,
                Heartbeat
            }
        }

        // ===================== Методы воспроизведения =====================
        
        public static void PlaySFXAtPosition(AudioClip clip, Vector3 position, float volume, float pitch)
        {
            ServiceLocator.Current.Get<AudioManager>()?.PlaySFXAtPosition(clip, position, volume, pitch);
        }
        
        public static void PlaySFX(AudioClip clip, float volume, float pitch)
        {
            ServiceLocator.Current.Get<AudioManager>()?.PlaySFX(clip, volume, pitch);
        }
        
        /// <summary>
        /// Проигрывает 2D-звук UI, игрока, врага, окружения или спецэффекта.
        /// </summary>
        public static void Play<T>(T command) where T : System.Enum
        {
            if (ServiceLocator.Current.Get<AudioManager>() == null) return;

            string clipName = command.ToString();

            // Для простых 2D звуков
            ServiceLocator.Current.Get<AudioManager>().PlaySFX(clipName);
        }

        /// <summary>
        /// Проигрывает 3D-звук в позиции.
        /// </summary>
        public static void PlayAt<T>(T command, Vector3 position) where T : System.Enum
        {
            if (ServiceLocator.Current.Get<AudioManager>() == null) return;

            string clipName = command.ToString();

            ServiceLocator.Current.Get<AudioManager>().PlaySFXAtPosition(clipName, position);
        }

        /// <summary>
        /// Включает обычную музыку (одиночный трек или базовый слой адаптивного трека).
        /// </summary>
        public static void PlayMusic(string trackName)
        {
            ServiceLocator.Current.Get<AudioManager>()?.PlayMusic(trackName);
        }

        /// <summary>
        /// Останавливает музыку.
        /// </summary>
        public static void StopMusic()
        {
            ServiceLocator.Current.Get<AudioManager>()?.StopMusic();
        }

        /// <summary>
        /// Меняет громкость слоя адаптивного трека.
        /// </summary>
        public static void SetMusicLayer(string trackName, string layerName, float weight)
        {
            ServiceLocator.Current.Get<AudioManager>()?.SetMusicParameter(trackName, layerName, weight);
        }
    }

}