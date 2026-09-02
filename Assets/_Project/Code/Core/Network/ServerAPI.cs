
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;


namespace Galactic1
{
    [System.Serializable]
    public class TaskData
    {
        public string taskId;
        public double finishTime;
    }

    [System.Serializable]
    public class TaskList
    {
        public List<TaskData> tasks = new List<TaskData>();
    }

    public class ServerAPI : IServerAPI
    {
        public string serverUrl = "https://galactic1games.com/server"; // без слэша на конце

        
        
        [System.Serializable]
        private class TimeResponse
        {
            public double server_time;
        }

        [System.Serializable]
        private class StartTaskResponse
        {
            public double finish_time;
        }

        // Пинг сервера
        public IEnumerator PingServer(System.Action<bool> callback)
        {
            using (UnityWebRequest req = UnityWebRequest.Get(serverUrl + "/ping.php"))
            {
                req.timeout = 5; // таймаут 5 секунд
                yield return req.SendWebRequest();
                callback(req.result == UnityWebRequest.Result.Success);
            }
        }

        // Получение серверного времени
        public IEnumerator GetServerTime(System.Action<double> callback)
        {
            using (UnityWebRequest req = UnityWebRequest.Get(serverUrl + "/time.php"))
            {
                yield return req.SendWebRequest();
                if (req.result == UnityWebRequest.Result.Success)
                {
                    var json = JsonUtility.FromJson<TimeResponse>(req.downloadHandler.text);
                    callback(json.server_time);
                }
                else callback(-1);
            }
        }

        // Создание задания
        public IEnumerator StartTask(string playerId, string taskId, double duration, System.Action<double> onResult)
        {
            string url = $"{serverUrl}/start_task.php?player={playerId}&task={taskId}&duration={duration}";
            UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Ошибка соединения: " + www.error);
                onResult?.Invoke(-1);
                yield break;
            }

            var json = www.downloadHandler.text;
            var response = JsonUtility.FromJson<StartTaskResponse>(json);
            onResult?.Invoke(response.finish_time);
        }

        // Сохранение активных задач
        public IEnumerator SaveTasks(string playerId, TaskList data)
        {
            string json = JsonUtility.ToJson(data);
            WWWForm form = new WWWForm();
            form.AddField("player", playerId);
            form.AddField("data", json);

            UnityWebRequest www = UnityWebRequest.Post($"{serverUrl}/save_tasks.php", form);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
                Debug.LogError("Ошибка сохранения: " + www.error);
            else
                DLog.Alert("Задания сохранены на сервере");
        }

        // Загрузка заданий
        public IEnumerator LoadTasks(string playerId, System.Action<TaskList> onResult)
        {
            string url = $"{serverUrl}/get_tasks.php?player={playerId}";
            UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Ошибка загрузки: " + www.error);
                onResult?.Invoke(new TaskList());
                yield break;
            }

            string json = www.downloadHandler.text;
            try
            {
                TaskList tasks = JsonUtility.FromJson<TaskList>(json);
                onResult?.Invoke(tasks);
            }
            catch
            {
                onResult?.Invoke(new TaskList());
            }
        }
    }
}