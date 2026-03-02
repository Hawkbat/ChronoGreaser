using System;
using UnityEngine;

[Serializable]
public class Save : ScriptableObject
{
    static Save instance;

    public float masterVolume = 1f;
    public float musicVolume = 1f;
    public float sfxVolume = 1f;
    public float cameraSensitivityX = 2f;
    public float cameraSensitivityY = 2f;
    public bool didTimeStopEnding = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Init()
    {
        instance = null;
    }

    public static Save Instance
    {
        get
        {
            if (instance == null)
            {
                instance = CreateInstance<Save>();
            }
            return instance;
        }
    }

    public static string SavePath => Application.persistentDataPath + "/save.json";

    public static void SaveFile()
    {
        var json = JsonUtility.ToJson(Instance, true);
        System.IO.File.WriteAllText(SavePath, json);
    }

    public static void LoadFile()
    {
        try
        {
            if (System.IO.File.Exists(SavePath))
            {
                var json = System.IO.File.ReadAllText(SavePath);
                JsonUtility.FromJsonOverwrite(json, Instance);
            }
            else
            {
                Debug.Log("No save file found, using default settings.");
                instance = CreateInstance<Save>();
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to load save file: " + e.Message);
            instance = CreateInstance<Save>();
        }
    }
}
