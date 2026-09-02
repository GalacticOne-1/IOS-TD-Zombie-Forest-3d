using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Galactic1.Core;
using Newtonsoft.Json;
using UnityEngine;

namespace Galactic1
{
    public delegate void Requestd(byte[] data);
    public class DataSaver: Singleton<DataSaver>
    {
        
        /*
         *     File.WriteAllBytes - создает файл и записывает байты, если файл уже есть просто перезаписывает
         *     File.Exists - проверяет существование файла
         */
        
        // если нужно получить файл из StreamingAssets, то нужно загружать через корутину (LoadData)
        // иначе, например для сохранения как обычно ()
        
        public Requestd onContinue;



        #region StreamingAssets for Pc & Mobile
        
        public void LoadData(string fileName)
        {
            string path = StreamingAssetsPath.StreamingAssetPathForWWW()+fileName; 
            StartCoroutine(local(path));
        }
        IEnumerator local(string path)
        {
            WWW request = new WWW(path);
            while (!request.isDone)
            {
                yield return null;
            }
            var data = request.bytes;
            onContinue?.Invoke(data);
        }
        

        #endregion





        #region File for PC & mobile

        // Работает на пк и мобилах
        public static void saveData<T>(T dataToSave, string fileName)
        {
            
            string tempPath = Application.persistentDataPath+"/"+fileName;// Path.Combine(Application.persistentDataPath, "Data");
            //tempPath = Path.Combine(tempPath, fileName + ".txt");
            //Debug.Log("try save "+tempPath);


            var jsonConverters = new List<JsonConverter>();
            jsonConverters.Add(new Vector2Converter());
            JsonConvert.DefaultSettings = () => new ()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                Converters = jsonConverters
            };
            
            //Convert To Json then to bytes
            string jsonData = JsonConvert.SerializeObject(dataToSave, Formatting.Indented);
            byte[] jsonByte = Encoding.ASCII.GetBytes(jsonData);
            Debug.Log("Saved Json Data: " + jsonData);

            //Create Directory if it does not exist
            /*if (!Directory.Exists(Path.GetDirectoryName(tempPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(tempPath));
            }*/
            //Debug.Log(path);

            try
            {
                File.WriteAllBytes(tempPath, jsonByte);
                Debug.Log("Saved Data to: " + tempPath.Replace("/", "\\"));
            }
            catch (Exception e)
            {
                Debug.LogWarning("Failed To PlayerInfo Data to: " + tempPath.Replace("/", "\\"));
                Debug.LogWarning("Error: " + e.Message);
            }
        }

        //Load Data
        public static T loadData<T>(string fileName)
        {
            string tempPath = Application.persistentDataPath+"/"+fileName;
            //tempPath = Path.Combine(tempPath, fileName + ".txt");

            //Exit if Directory or File does not exist
            /*if (!Directory.Exists(Path.GetDirectoryName(tempPath)))
            {
                Debug.LogWarning("Directory does not exist");
                return default(T);
            }
            */

            if (!File.Exists(tempPath))
            {
                Debug.Log("File does not exist");
                return default(T);
            }

            //Load saved Json
            byte[] jsonByte = null;
            try
            {
                jsonByte = File.ReadAllBytes(tempPath);
                Debug.Log("Loaded Data from: " + tempPath.Replace("/", "\\"));
            }
            catch (Exception e)
            {
                Debug.LogWarning("Failed To Load Data from: " + tempPath.Replace("/", "\\"));
                Debug.LogWarning("Error: " + e.Message);
            }

            //Convert to json string
            string jsonData = Encoding.ASCII.GetString(jsonByte);
            Debug.Log("Loaded Json Data: " + jsonData);
            
            var jsonConverters = new List<JsonConverter>();
            jsonConverters.Add(new Vector2Converter());
            JsonConvert.DefaultSettings = () => new ()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                Converters = jsonConverters
            };

            //Convert to Object
            object resultValue = JsonConvert.DeserializeObject<T>(jsonData);
            return (T) Convert.ChangeType(resultValue, typeof(T));
        }

        public static bool deleteData(string dataFileName)
        {
            bool success = false;

            //Load Data
            string tempPath = Path.Combine(Application.persistentDataPath, "Data");
            tempPath = Path.Combine(tempPath, dataFileName + ".txt");

            //Exit if Directory or File does not exist
            if (!Directory.Exists(Path.GetDirectoryName(tempPath)))
            {
                Debug.LogWarning("Directory does not exist");
                return false;
            }

            if (!File.Exists(tempPath))
            {
                Debug.Log("File does not exist");
                return false;
            }

            try
            {
                File.Delete(tempPath);
                Debug.Log("Data deleted from: " + tempPath.Replace("/", "\\"));
                success = true;
            }
            catch (Exception e)
            {
                Debug.LogWarning("Failed To Delete Data: " + e.Message);
            }

            return success;
        }
        
        public T ConvertData<T>(byte[] data)
        {
            string jsonData = Encoding.ASCII.GetString(data);
            
            var jsonConverters = new List<JsonConverter>();
            jsonConverters.Add(new Vector2Converter());
            JsonConvert.DefaultSettings = () => new ()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                Converters = jsonConverters
            };

            //Convert to Object
            object resultValue = JsonConvert.DeserializeObject<T>(jsonData);
            return (T) Convert.ChangeType(resultValue, typeof(T));
        }
        
        

        #endregion
    }
}