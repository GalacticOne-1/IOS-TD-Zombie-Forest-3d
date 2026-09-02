using System.Collections;
using System.Collections.Generic;
using Galactic1.Core;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Galactic1
{
    public class SceneService
    {
        public Coroutines Coroutines;


        private AsyncOperationHandle<SceneInstance> handle;
        
        private List<SceneInstance> _allLoadedScenes = new();

        public SceneService(Coroutines coroutines)
        {
            Coroutines = coroutines;
        }


        /// <summary>
        /// Загрузка с выгрузкой несовместимых сцен
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="setActiveScene"></param>
        public Coroutine LoadScene(string sceneName, bool setActiveScene)
        {
            return Coroutines.StartCoroutine(process(sceneName, setActiveScene));
        }

        IEnumerator process(string sceneName, bool setActiveScene)
        {
            // выгружаем несовместимые сцены c новой
            yield return Coroutines.StartCoroutine(FindAndUnloadScene(sceneName));
            // загрузка новой сцены
            yield return Coroutines.StartCoroutine(LoadSceneAsync(sceneName, setActiveScene));
            
            // даем время корутинам завершится (A* делает рескан не сразу, а то объекты сцены пропускаются)
#if !UNITY_EDITOR
            yield return new WaitForSeconds(2);
#endif
        }

        IEnumerator FindAndUnloadScene(string newScene)
        {
            // несoвместимые сцены с новой
            string[] notCompatible = Scenes.NotCompatibleGroups(newScene);
            
            
            while (true)
            {
                bool newCheck = false;
                int sceneCount = SceneManager.sceneCount;
                //Debug.Log($"Number of loaded scenes: {sceneCount}");
                
                for (int i = 0; i < notCompatible.Length; i++)
                {
                    bool success = false;
                    for (int j = 0; j < sceneCount; j++)
                    {
                        Scene scene = SceneManager.GetSceneAt(j);
                        //Debug.Log($"Scene {j}: {scene.name} (Path: {scene.path})");
                        
                        // нашли несовместимую сцену 
                        if (notCompatible[i] == scene.name)
                        {
                            // #1 выгружаем сцену если есть ее ссылка
                            foreach (var sceneInstance in _allLoadedScenes)
                            {
                                if(sceneInstance.Scene.name == scene.name)
                                {
                                    yield return Addressables.UnloadSceneAsync(sceneInstance);
                                    _allLoadedScenes.Remove(sceneInstance);
                                    success = true;
                                    break;
                                }
                            }

                            // #2 выгружаем сцену без ссылки
                            if (!success)
                            {
                                yield return SceneManager.UnloadSceneAsync(scene.name);
                                success = true;
                            }

                            if (success)
                            {
                                newCheck = true;
                                break;
                            }
                        }
                    }

                    // *** все несовместимые сцены проверены, останавливаем корутину
                    if (i == notCompatible.Length - 1) yield break; 
                    
                    // что бы по новой запустить проверку сцен после удаления,
                    // иначе будет ошибка связанная с удалением в массиве внутри цикла
                    if(newCheck) break;
                }
            }
        }
        
        
        
        /// <summary>
        /// Для загрузки сцены
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="setActiveScene"></param>
        /// <returns></returns>
        public IEnumerator LoadSceneAsync(string sceneName, bool setActiveScene)
        {
            handle = Addressables.LoadSceneAsync(sceneName+".unity", LoadSceneMode.Additive);

            yield return handle;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                _allLoadedScenes.Add(handle.Result);        // *** saving each loaded scene
                
                if (setActiveScene)
                    SceneManager.SetActiveScene(SceneManager.GetSceneByName(handle.Result.Scene.name));
            }
            else
            {
                Addressables.Release(handle);
            }
        }
        
        /// <summary>
        /// Выгружает сцену, если она загружена.
        /// Сначала пытается использовать Addressables handle,
        /// иначе выгружает через SceneManager.
        /// </summary>
        public Coroutine UnloadScene(string sceneName)
        {
            return Coroutines.StartCoroutine(UnloadSceneAsync(sceneName));
        }

        private IEnumerator UnloadSceneAsync(string sceneName)
        {
            // Сначала ищем сохранённый Addressables handle
            for (int i = _allLoadedScenes.Count - 1; i >= 0; i--)
            {
                var sceneInstance = _allLoadedScenes[i];

                if (sceneInstance.Scene.name != sceneName)
                    continue;

                yield return Addressables.UnloadSceneAsync(sceneInstance);

                _allLoadedScenes.RemoveAt(i);
                yield break;
            }

            // Если сцена была загружена не через Addressables
            if (SceneManager.GetSceneByName(sceneName).isLoaded)
            {
                yield return SceneManager.UnloadSceneAsync(sceneName);
            }
        }
        
        
        
        /// <summary>
        /// true - сцена активна 
        /// </summary>
        /// <param name="sceneName"></param>
        /// <returns></returns>
        public bool SceneExist(string sceneName)
        {
            int sceneCount = SceneManager.sceneCount;
            //Debug.Log($"Number of loaded scenes: {sceneCount}");

            for (int i = 0; i < sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                //Debug.Log($"Scene {i}: {scene.name} (Path: {scene.path})");
                if (scene.name == sceneName) return true;
            }

            return false;
        }
        
        
        
    }
    
}