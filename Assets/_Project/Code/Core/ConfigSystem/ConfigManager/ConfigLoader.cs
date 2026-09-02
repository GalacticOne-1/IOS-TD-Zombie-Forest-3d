using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Security.Cryptography;
using Galactic1;

namespace Galactic1.Configs
{
    public static class ConfigLoader
    {
        private const string RemoteManifestUrl = "https://galactic1games.com/Survivor Squad/Configs/manifest.json";
        private static readonly string LocalPath = Application.persistentDataPath;
        private static readonly string StreamingPath = Path.Combine(Application.streamingAssetsPath, "Configs");

        public static IEnumerator LoadAllConfigs(Action<Dictionary<string, string>> onLoaded)
        {
            // true - загрузка только из streaming assets (for developing)
            bool requiresStreaming = !AppConstants.SERVER_ON;


            EnsureCacheFolder();

            Manifest remoteManifest = null;
            Manifest localManifest = TryLoadManifestFromCache();
            Manifest streamingManifest = TryLoadManifestFromStreaming();

            // 1) Попробовать скачать удалённый манифест
            if (!requiresStreaming)
            {
                yield return TryDownloadTextWithRetry(
                    RemoteManifestUrl,
                    (text) =>
                    {
                        if (!string.IsNullOrEmpty(text))
                            remoteManifest = JsonUtility.FromJson<Manifest>(text);
                    },
                    maxAttempts: 3,
                    delayBetweenAttempts: 2f,
                    timeout: 5);
            }

            Manifest manifestToUse = null;
            bool needDownloadFiles = false;

            if (!requiresStreaming && remoteManifest != null)
            {
                if (localManifest != null && remoteManifest.version <= localManifest.version)
                {
                    DLog.Alert(
                        $"ℹ Using cached configs (local v{localManifest.version}, remote v{remoteManifest.version})");
                    manifestToUse = localManifest;
                }
                else
                {
                    DLog.Alert(
                        $"⬆ New manifest found (local v{localManifest?.version ?? 0} → remote v{remoteManifest.version})");
                    manifestToUse = remoteManifest;
                    needDownloadFiles = true;
                    CacheManifest(remoteManifest); // сохраняем в кеш только для загрузки с сервера
                }
            }
            else if (!requiresStreaming && localManifest != null)
            {
                DLog.Alert($"📂 No internet, using cached manifest (v{localManifest.version})", EDlogColor.ORANGE);
                manifestToUse = localManifest;
            }
            else if (streamingManifest != null)
            {
                DLog.Alert($"📦 No cache, using StreamingAssets manifest (v{streamingManifest.version})",
                    EDlogColor.ORANGE);
                manifestToUse = streamingManifest;
            }
            else
            {
                Debug.LogError("❌ No manifest available at all, fallback to built-in ScriptableObjects.");
                onLoaded?.Invoke(new Dictionary<string, string>());
                yield break;
            }

            var configs = new Dictionary<string, string>();

            foreach (var entry in manifestToUse.files)
            {
                string json = null;
                string cachedPath = Path.Combine(LocalPath, entry.key + ".json");




                // #1 Скачивание, если нужно
                // (загружет если версия выше чем в кеш)
                if (!requiresStreaming && needDownloadFiles)
                {
                    yield return TryDownloadTextWithRetry(
                        entry.url,
                        (text) => json = text,
                        maxAttempts: 3,
                        delayBetweenAttempts: 2f,
                        timeout: 5);
                }


                // #2 Загрузка из кэша
                // (если версия с сервером одинаковая или нет подключения)
                else if (File.Exists(cachedPath))
                {
                    json = File.ReadAllText(cachedPath);
                    Debug.Log($"✅ Cache valid for {entry.key}");
                }




                // #3 StreamingAssets fallback
                // (если ничего не смогли скачать, загружаем внутренний json)
                if (string.IsNullOrEmpty(json))
                {
                    string streamPath = Path.Combine(StreamingPath, entry.key + ".json");

#if UNITY_ANDROID && !UNITY_EDITOR
                using (UnityWebRequest req = UnityWebRequest.Get(streamPath))
                {
                    yield return req.SendWebRequest();
                    if (req.result == UnityWebRequest.Result.Success)
                        json = req.downloadHandler.text;
                }
#else
                    if (File.Exists(streamPath))
                        json = File.ReadAllText(streamPath);
#endif
                }

                // 4) Сохраняем в кэш и словарь
                if (!string.IsNullOrEmpty(json))
                {
                    File.WriteAllText(cachedPath, json);
                    configs[entry.key] = json;
                }
                else
                {
                    Debug.LogWarning($"❌ No data for {entry.key}, using built-in ScriptableObject.");
                }
            }

            onLoaded?.Invoke(configs);
        }

        // =================== HELPERS ===================

        private static void EnsureCacheFolder()
        {
            try
            {
                if (!Directory.Exists(LocalPath)) Directory.CreateDirectory(LocalPath);
            }
            catch
            {
            }
        }

        private static void CacheManifest(Manifest m)
        {
            try
            {
                File.WriteAllText(Path.Combine(LocalPath, "manifest.json"), JsonUtility.ToJson(m));
            }
            catch
            {
            }
        }

        private static Manifest TryLoadManifestFromCache()
        {
            string path = Path.Combine(LocalPath, "manifest.json");
            if (!File.Exists(path)) return null;
            try
            {
                return JsonUtility.FromJson<Manifest>(File.ReadAllText(path));
            }
            catch
            {
                return null;
            }
        }

        private static Manifest TryLoadManifestFromStreaming()
        {
            string path = Path.Combine(StreamingPath, "manifest.json");

#if UNITY_ANDROID && !UNITY_EDITOR
        using (UnityWebRequest req = UnityWebRequest.Get(path))
        {
            req.timeout = 5;
            req.SendWebRequest();
            while (!req.isDone) { }
            if (req.result == UnityWebRequest.Result.Success)
                return JsonUtility.FromJson<Manifest>(req.downloadHandler.text);
        }
        return null;
#else
            if (!File.Exists(path)) return null;
            try
            {
                return JsonUtility.FromJson<Manifest>(File.ReadAllText(path));
            }
            catch
            {
                return null;
            }
#endif
        }

        // ===== MD5 =====
        private static string ComputeMD5(string path)
        {
            if (!File.Exists(path)) return null;

            using (var md5 = MD5.Create())
            using (var stream = File.OpenRead(path))
            {
                var hash = md5.ComputeHash(stream);
                return System.BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        // ===== Download with Retry =====
        public static IEnumerator TryDownloadTextWithRetry(
            string url,
            Action<string> onDone,
            int maxAttempts = 3,
            float delayBetweenAttempts = 1f,
            int timeout = 5)
        {
            int attempt = 0;
            string result = null;





            while (attempt < maxAttempts && string.IsNullOrEmpty(result))
            {
                attempt++;
                using (UnityWebRequest req = UnityWebRequest.Get(url))
                {
                    req.timeout = timeout;
                    yield return req.SendWebRequest();

                    if (req.result == UnityWebRequest.Result.Success)
                    {
                        result = req.downloadHandler.text;
#if UNITY_EDITOR
                        DLog.Alert($"✅ Download successful: {url} (attempt {attempt})",
                            EDlogColor.YELLOW,
                            AppConstants.show_log_core);
#endif
                    }
                    else
                    {
#if UNITY_EDITOR
                        DLog.Alert($"⚠ Attempt {attempt} failed for {url}: {req.error}",
                            EDlogColor.ORANGE,
                            AppConstants.show_log_core);
#endif
                        if (attempt < maxAttempts)
                            yield return new WaitForSeconds(delayBetweenAttempts);
                    }
                }
            }

            if (string.IsNullOrEmpty(result))
            {
#if UNITY_EDITOR
                DLog.Alert($"❌ All {maxAttempts} attempts failed for {url}",
                    EDlogColor.RED,
                    AppConstants.show_log_core);
#endif
            }

            onDone?.Invoke(result);
        }
    }
}
