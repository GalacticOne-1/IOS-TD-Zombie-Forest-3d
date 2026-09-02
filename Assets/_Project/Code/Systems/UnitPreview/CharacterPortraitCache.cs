
using System.Collections;
using System.Collections.Generic;
using System;
using Galactic1.Meta.Configs.Recruitment;
using UnityEngine;

namespace Galactic1.UI.CharacterPreview
{
    /// <summary>
    /// Кэш портретов персонажей.
    /// Создаётся один раз при инициализации.
    /// Хранит RenderTexture по archetypeId.
    /// </summary>
    public sealed class CharacterPortraitCache : MonoBehaviour, IGameService
    {
        private readonly Dictionary<string, RenderTexture> portraits = new();
        private readonly Dictionary<string, RenderTexture> fullBodies = new();



        public void Initialize()
        {
            EventBus<SceneServicesResetReusableEvent>.Register(new EventBinding<SceneServicesResetReusableEvent>(Clear));
        }

        // =========================================================
        // PUBLIC API
        // =========================================================

        /// <summary>
        /// Прогрев кэша — рендерит все архетипы из пула.
        /// Вызывать при загрузке сцены.
        /// </summary>
        public void Warmup(
            UnitIdentityPoolConfig pool,
            CharacterPreviewService service,
            List<string> archetypeId,
            Action onComplete = null)
        {
            StartCoroutine(WarmupRoutine(pool, service, archetypeId, onComplete));
        }

        /// <summary>
        /// Рендерит портрет одного архетипа не трогая остальные.
        /// </summary>
        public void Warmup(
            UnitIdentityPoolConfig pool,
            CharacterPreviewService service,
            string archetypeId,
            Action onComplete = null)
        {
            if (string.IsNullOrEmpty(archetypeId)) return;

            // Уже есть — не перерендериваем
            if (HasPortrait(archetypeId))
            {
                onComplete?.Invoke();
                return;
            }

            StartCoroutine(WarmupRoutine(pool, service,
                new List<string> { archetypeId }, onComplete));
        }

        /// <summary>
        /// Возвращает портрет по archetypeId.
        /// Null если ещё не готов.
        /// </summary>
        public RenderTexture GetPortrait(string archetypeId)
        {
            portraits.TryGetValue(archetypeId, out var tex);
            return tex;
        }

        public RenderTexture GetFullBody(string archetypeId)
        {
            fullBodies.TryGetValue(archetypeId, out var tex);
            return tex;
        }

        public bool HasPortrait(string archetypeId)
            => portraits.ContainsKey(archetypeId);

        /// <summary>
        /// Удаляет портрет и полный рост по archetypeId.
        /// Вызывать когда юнит покидает игру.
        /// </summary>
        public void Remove(string archetypeId)
        {
            if (string.IsNullOrEmpty(archetypeId)) return;

            if (portraits.TryGetValue(archetypeId, out var portrait))
            {
                portrait?.Release();
                portraits.Remove(archetypeId);
            }

            if (fullBodies.TryGetValue(archetypeId, out var fullBody))
            {
                fullBody?.Release();
                fullBodies.Remove(archetypeId);
            }
        }

        // =========================================================
        // PRIVATE
        // =========================================================

        private IEnumerator WarmupRoutine(
            UnitIdentityPoolConfig pool,
            CharacterPreviewService service,
            List<string> archetypeId,
            Action onComplete)
        {
            int total = archetypeId.Count;
            int rendered = 0;

            for (int i = 0; i < total; i++)
            {
                var key = archetypeId[i];
                var survEntry = pool.GetSurvivorEntry(key);

                if (HasPortrait(key))
                    continue;

                // Портрет
                bool portraitDone = false;
                service.Request(survEntry, CharacterRenderMode.Portrait, tex =>
                {
                    portraits[key] = tex;
                    portraitDone = true;
                });

                yield return new WaitUntil(() => portraitDone);

                // Полный рост
                bool fullBodyDone = false;
                service.Request(survEntry, CharacterRenderMode.FullBody, tex =>
                {
                    fullBodies[key] = tex;
                    fullBodyDone = true;
                });

                yield return new WaitUntil(() => fullBodyDone);

                rendered++;
            }

            onComplete?.Invoke();
        }

        private void Clear()
        {
            foreach (var tex in portraits.Values)
                tex?.Release();

            foreach (var tex in fullBodies.Values)
                tex?.Release();

            portraits.Clear();
            fullBodies.Clear();
        }

    }
}