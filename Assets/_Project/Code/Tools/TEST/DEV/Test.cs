using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine;

public class Test : MonoBehaviour
{
    public TextMeshProUGUI t;
    private int val;

    private string file = "save_data_test";
    
    public class Saving
    {
        public int val;
    }

    void OnGUI()
    {



        if (GUI.Button(new Rect(20, Screen.height / 2, 50, 30), "Add"))
        {
            val += 5;
            t.text = val.ToString();
        }
        
        if (GUI.Button(new Rect(75, Screen.height / 2, 50, 30), "Minus"))
        {
            val -= 5;
            t.text = val.ToString();
        }
        
        if (GUI.Button(new Rect(20, Screen.height / 2+50, 80, 30), "Save"))
            Save();
        
        if (GUI.Button(new Rect(120, Screen.height / 2+50, 80, 30), "Load"))
            Load();
    }



    void Save()
    {
        string tempPath = Application.persistentDataPath+"/"+file;
        Debug.Log("try save "+tempPath);
        //Convert To Json then to bytes
        string jsonData = JsonUtility.ToJson(new Saving()
        {
            val = val
        }, true);
        byte[] jsonByte = Encoding.ASCII.GetBytes(jsonData);

        
        File.WriteAllBytes(tempPath, jsonByte);
        //Debug.Log("Saved Data to: " + tempPath.Replace("/", "\\"));
        
    }

    void Load()
    {
        string tempPath = Application.persistentDataPath + "/" + file;
        if (!File.Exists(tempPath))
        {
            Debug.Log("File does not exist");
            return;
        }
        byte[] jsonByte = File.ReadAllBytes(tempPath);
        var d = ConvertData<Saving>(jsonByte);
        val = d.val;
        t.text = val.ToString();

        //StartCoroutine(local(Application.persistentDataPath+"/"+file));
    }
    
    IEnumerator local(string path)
    {
        Debug.Log("WWW 1");
        WWW request = new WWW(path);
        Debug.Log("WWW 2");
        while (!request.isDone)
        {
            yield return null;
        }
        
        var data = request.bytes;
        Debug.Log("WWW 3 "+data);
        var d = ConvertData<Saving>(data);
        val = d.val;
        t.text = val.ToString();
    }
    
    public T ConvertData<T>(byte[] data)
    {
        string jsonData = Encoding.ASCII.GetString(data);

        //Convert to Object
        object resultValue = JsonUtility.FromJson<T>(jsonData);
        return (T) Convert.ChangeType(resultValue, typeof(T));
    }
}
