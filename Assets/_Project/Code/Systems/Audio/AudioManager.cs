using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;

namespace Galactic1.Systems
{

    /// <summary>
    /// Центральный менеджер звука — управляет музыкой, адаптивными треками и SFX.
    /// </summary>
    public class AudioManager : MonoBehaviour, IGameService
    {
        
        [Header("Default Mixer Levels")] [SerializeField]
        private AudioSettings _audioConfig;

        [Header("Audio Mixer")] 
        public AudioMixer masterMixer; // Общий аудио-микшер для громкости SFX/Music
        public AudioMixerGroup musicGroup;
        public AudioMixerGroup sfxGroup;
        
        [Header("Prefabs")] 
        public AudioSource musicSourcePrefab; // Префаб источника для музыки
        public AudioSourcePool sfxPoolPrefab; // Префаб пула для SFX

        [Header("Audio Data")] 
        public List<MusicClipData> musicClips = new List<MusicClipData>(); // Обычные треки
        public List<SFXData> sfxClips = new List<SFXData>(); // Звуковые эффекты
        public List<AdaptiveMusicTrack> adaptiveTracks = new List<AdaptiveMusicTrack>(); // Адаптивные треки

        // --- Внутренние структуры ---
        private AudioSourcePool sfxPoolInstance; // Инстанс пула SFX
        private Dictionary<string, MusicClipData> musicDict = new(); // Быстрый доступ к обычным трекам
        private Dictionary<string, SFXData> sfxDict = new(); // Быстрый доступ к SFX
        private Dictionary<string, List<AudioSource>> activeSFX = new(); // Активные SFX по имени
        private Dictionary<string, AdaptiveMusicTrack> adaptiveTrackDict = new(); // Адаптивные треки
        private Dictionary<string, AudioSource> musicLayerSources = new(); // Источники слоёв адаптивной музыки
        private AudioSource currentMusicSource; // Текущий источник обычной музыки

        
        private const float MIN_VOLUME_DB = -80f;

        private float musicUserVolume = 1f;
        private float sfxUserVolume = 1f;
        
        
        [System.Serializable]
        public class MusicClipData
        {
            public AudioClip clip;
            [Range(0f, 1f)] public float volume = 1f;
        }
        
        
        public void Activate()
        {
            ApplyDefaultMixerLevels();
            Initialize();
        }

        /// <summary>
        /// Инициализация словарей и пула SFX.
        /// </summary>
        private void Initialize()
        {
            sfxPoolInstance = Instantiate(sfxPoolPrefab, transform);

            foreach (var musicData in musicClips)
            {
                if (musicData.clip == null) continue;
                musicDict[musicData.clip.name] = musicData;
            }

            foreach (var sfx in sfxClips)
            {
                sfxDict[sfx.name] = sfx;
                activeSFX[sfx.name] = new List<AudioSource>();
            }

            foreach (var track in adaptiveTracks)
                adaptiveTrackDict[track.trackName] = track;
        }
        
        
        private void ApplyDefaultMixerLevels()
        {
            if (masterMixer == null)
                return;

            masterMixer.SetFloat(
                "MusicVolume",
                _audioConfig.MusicVolumeDb);

            masterMixer.SetFloat(
                "SFXVolume",
                _audioConfig.SFXVolumeDb);
        }

        // =========================================================================

        #region 🎧 Обычная музыка

        /// <summary>
        /// Воспроизводит трек (обычный или адаптивный).
        /// </summary>
        public void PlayMusic(string name, float fadeTime = 1f, bool loop = true)
        {
            // Проверяем: это адаптивная музыка?
            if (adaptiveTrackDict.ContainsKey(name))
            {
                StartCoroutine(PlayAdaptiveMusic(name, fadeTime));
                return;
            }

            // Если это обычный трек
            if (musicDict.ContainsKey(name))
                StartCoroutine(FadeInMusic(musicDict[name], fadeTime, loop));
        }

        /// <summary>
        /// Останавливает текущую музыку (включая адаптивные слои) с плавным затуханием.
        /// </summary>
        public void StopMusic(float fadeTime = 1f)
        {
            // Если активны адаптивные слои — выключаем все
            foreach (var src in musicLayerSources.Values)
                StartCoroutine(FadeLayer(src, 0, fadeTime));

            // Останавливаем обычный трек
            if (currentMusicSource != null)
                StartCoroutine(FadeOutMusic(currentMusicSource, fadeTime));
        }

        /// <summary>
        /// Плавно включает обычную музыку.
        /// </summary>
        private IEnumerator FadeInMusic(MusicClipData musicData, float fadeTime, bool loop)
        {
            // Если есть активная музыка — затухаем её
            if (currentMusicSource != null && currentMusicSource.isPlaying)
                yield return StartCoroutine(FadeOutMusic(currentMusicSource, fadeTime));

            // Создаём новый источник
            currentMusicSource = Instantiate(musicSourcePrefab, transform);
            currentMusicSource.clip = musicData.clip;
            currentMusicSource.loop = loop;
            currentMusicSource.volume = 0;
            currentMusicSource.outputAudioMixerGroup = musicGroup;
            currentMusicSource.Play();

            // Плавное нарастание громкости до целевого уровня клипа
            float targetVolume = musicData.volume;
            float t = 0;
            while (t < fadeTime)
            {
                t += Time.deltaTime;
                currentMusicSource.volume = Mathf.Lerp(0, targetVolume, t / fadeTime);
                yield return null;
            }

            currentMusicSource.volume = targetVolume;
        }

        /// <summary>
        /// Плавно выключает музыку.
        /// </summary>
        private IEnumerator FadeOutMusic(AudioSource source, float fadeTime)
        {
            float startVolume = source.volume;
            float t = 0;
            while (t < fadeTime)
            {
                t += Time.deltaTime;
                source.volume = Mathf.Lerp(startVolume, 0, t / fadeTime);
                yield return null;
            }

            source.Stop();
            Destroy(source.gameObject);
        }

        #endregion

        // =========================================================================

        #region 🎚 Адаптивная музыка

        /// <summary>
        /// Запускает адаптивный трек (все его слои).
        /// </summary>
        private IEnumerator PlayAdaptiveMusic(string trackName, float fadeTime)
        {
            var track = adaptiveTrackDict[trackName];

            // Проходим по каждому слою трека
            foreach (var layer in track.layers)
            {
                // Если источник ещё не создан — создаём
                if (!musicLayerSources.ContainsKey(layer.name))
                {
                    AudioSource src = Instantiate(musicSourcePrefab, transform);
                    src.clip = layer.clip;
                    src.loop = true;
                    src.volume = 0;
                    src.outputAudioMixerGroup = musicGroup;
                    src.Play();
                    musicLayerSources[layer.name] = src;
                }

                // Включаем базовый слой
                float targetVolume = layer.volume * layer.weight;
                StartCoroutine(FadeLayer(musicLayerSources[layer.name], targetVolume, fadeTime));
            }

            yield return null;
        }

        /// <summary>
        /// Меняет громкость (вес) конкретного слоя адаптивной музыки.
        /// </summary>
        public void SetMusicParameter(string trackName, string layerName, float weight)
        {
            if (!adaptiveTrackDict.ContainsKey(trackName)) return;

            var track = adaptiveTrackDict[trackName];
            var layer = track.layers.Find(l => l.name == layerName);
            if (layer == null) return;

            layer.weight = Mathf.Clamp01(weight);
            UpdateMusicLayer(layer);
        }

        /// <summary>
        /// Обновляет громкость конкретного слоя.
        /// </summary>
        private void UpdateMusicLayer(MusicLayer layer)
        {
            if (!musicLayerSources.ContainsKey(layer.name))
            {
                AudioSource src = Instantiate(musicSourcePrefab, transform);
                src.clip = layer.clip;
                src.loop = true;
                src.volume = 0;
                src.outputAudioMixerGroup = musicGroup;
                src.Play();
                musicLayerSources[layer.name] = src;
            }

            float targetVolume = layer.volume * layer.weight;
            StartCoroutine(FadeLayer(musicLayerSources[layer.name], targetVolume, 0.5f));
        }

        /// <summary>
        /// Плавное изменение громкости слоя.
        /// </summary>
        private IEnumerator FadeLayer(AudioSource source, float targetVolume, float duration)
        {
            float start = source.volume;
            float t = 0;
            while (t < duration)
            {
                t += Time.deltaTime;
                source.volume = Mathf.Lerp(start, targetVolume, t / duration);
                yield return null;
            }

            source.volume = targetVolume;
        }

        #endregion

        // =========================================================================

        #region 🔊 SFX

        /// <summary>
        /// Воспроизводит 2D-звук (интерфейс, UI, короткие эффекты).
        /// </summary>
        public void PlaySFX(
            string name, 
            float? volume = null, 
            float pitchMin = 1f, 
            float pitchMax = 1f)
        {
            if (!sfxDict.ContainsKey(name)) return;
            var data = sfxDict[name];

            // Ограничение количества одновременно проигрываемых копий
            if (activeSFX[name].Count >= data.maxInstances) return;

            AudioSource src = sfxPoolInstance.GetAudioSource();
            src.clip = data.clip;

            // Если громкость передана — используем её, иначе берем базовую
            src.volume = volume.HasValue ? volume.Value : data.volume;

            src.pitch = Random.Range(pitchMin, pitchMax);
            src.spatialBlend = 0f; // 2D
            src.outputAudioMixerGroup = sfxGroup;
            src.Play();

            activeSFX[name].Add(src);
            StartCoroutine(ReleaseAfterPlay(name, src));
        }

        /// <summary>
        /// Воспроизводит 3D-звук в заданной позиции.
        /// </summary>
        public void PlaySFXAtPosition(
            string name, 
            Vector3 position, 
            float? volume = null, 
            float pitchMin = 1f, 
            float pitchMax = 1f)
        {
            if (!sfxDict.ContainsKey(name)) return;
            var data = sfxDict[name];

            if (activeSFX[name].Count >= data.maxInstances) return;

            AudioSource src = sfxPoolInstance.GetAudioSource();
            src.transform.position = position;
            src.clip = data.clip;
            src.volume = volume.HasValue ? volume.Value : data.volume;
            src.pitch = Random.Range(pitchMin, pitchMax);
            src.spatialBlend = 1f; // 3D
            src.outputAudioMixerGroup = sfxGroup;
            src.Play();

            activeSFX[name].Add(src);
            StartCoroutine(ReleaseAfterPlay(name, src));
        }
        
        /// <summary>
        /// Проигрывает 3D-звук по прямой ссылке на AudioClip, а не по имени
        /// из sfxDict. Нужен там, где клип приходит из внешнего authoring-ассета
        /// (например WeaponAudioDefinition), а не заведён отдельной записью
        /// в общем списке sfxClips.
        ///
        /// Переиспользует тот же sfxPoolInstance, что и именованные SFX —
        /// вторая пул-система не создаётся.
        ///
        /// НЕ учитывается в activeSFX/maxInstances — этот лимит существует
        /// только для именованных SFX. Голосовой бюджет для выстрелов пока
        /// не реализован (CombatAudioPrioritySystem в проекте отсутствует).
        /// </summary>
        public void PlaySFXAtPosition(AudioClip clip, Vector3 position, float volume, float pitch)
        {
            if (clip == null || sfxPoolInstance == null) 
                return;

#if UNITY_EDITOR
            DLog.Alert($"{clip.name}; volume = {volume}; pitch = {pitch}", EDlogColor.YELLOW);
#endif

            
            AudioSource src = sfxPoolInstance.GetAudioSource();
            src.transform.position = position;
            src.clip = clip;
            src.volume = volume;
            src.pitch = pitch;
            src.spatialBlend = 1f; // 3D
            src.outputAudioMixerGroup = sfxGroup;
            src.Play();

            StartCoroutine(ReleaseRawAfterPlay(src));
        }

        /// <summary>
        /// Возвращает источник в пул после проигрывания.
        /// </summary>
        private IEnumerator ReleaseAfterPlay(string name, AudioSource src)
        {
            yield return new WaitForSeconds(src.clip.length);
            activeSFX[name].Remove(src);
            sfxPoolInstance.ReleaseAudioSource(src);
        }
        
        
        /// <summary>
        /// Возвращает источник в пул после проигрывания клипа,
        /// переданного напрямую (не из sfxDict).
        /// </summary>
        private IEnumerator ReleaseRawAfterPlay(AudioSource src)
        {
            yield return new WaitForSeconds(src.clip.length);
            sfxPoolInstance.ReleaseAudioSource(src);
        }


        #endregion

        // =========================================================================

        #region 🎚 Контроль громкости

        public void SetSFXVolume(float volume)
        {
            sfxUserVolume = Mathf.Clamp01(volume);

            float db = CalculateVolumeDb(
                _audioConfig.SFXVolumeDb,
                sfxUserVolume);

            masterMixer.SetFloat("SFXVolume", db);
        }

        public void SetMusicVolume(float volume)
        {
            musicUserVolume = Mathf.Clamp01(volume);

            float db = CalculateVolumeDb(
                _audioConfig.MusicVolumeDb,
                musicUserVolume);

            masterMixer.SetFloat("MusicVolume", db);
        }

        private float CalculateVolumeDb(float baseDb, float userVolume)
        {
            if (userVolume <= 0.0001f)
                return MIN_VOLUME_DB;

            return baseDb + Mathf.Log10(userVolume) * 20f;
        }

        #endregion
    }
}