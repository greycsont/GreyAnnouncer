using System;
using System.IO;
using Newtonsoft.Json;

namespace greycsont.GreyAnnouncer;

public class JsonManager
{
    #region Obslete
    private const string  JSON_NAME      = "greyannouncer.json";
    private static string JSON_FILE_PATH = PathManager.GetCurrentPluginPath(JSON_NAME);

    public static void Initialize()
    {
        TryToFetchJson();
    }

    private static void TryToFetchJson()
    {
        if (!CheckDoesJsonExists()) CreateJsonFile();
        ReadJson();
    }

    private static bool CheckDoesJsonExists()
    {
        return File.Exists(JSON_FILE_PATH);
    }

    private static void CreateJsonFile()
    {
        var audioNames = new string[] { "D", "C", "B", "A", "S", "SS", "SSS", "U" };

        var rootObject = new RootObject { AudioNames = audioNames };
        string json    = JsonConvert.SerializeObject(rootObject, Formatting.Indented);
        File.WriteAllText(JSON_FILE_PATH, json);
        Plugin.Log.LogInfo($"Initialized {JSON_NAME}");
    }

    public static void ReadJson()
    {
        string loadedJson    = File.ReadAllText(JSON_FILE_PATH);
        JsonSetting.Settings = JsonConvert.DeserializeObject<RootObject>(loadedJson);
    }
    #endregion

    public static T ReadJson<T>(string jsonName) where T : class
    {
        try
        {
            string jsonFilePath = PathManager.GetCurrentPluginPath(jsonName);
            if (!File.Exists(jsonFilePath))
            {
                throw new FileNotFoundException($"JSON file '{jsonName}' not found at path: {jsonFilePath}");
            }

            string loadedJson = File.ReadAllText(jsonFilePath);
            T deserializedObject = JsonConvert.DeserializeObject<T>(loadedJson);

            if (deserializedObject == null)
            {
                throw new InvalidOperationException($"Failed to deserialize JSON file '{jsonName}' into type {typeof(T).Name}");
            }

            return deserializedObject;
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"Error reading JSON file '{jsonName}': {ex.Message}");
            throw;
        }
    }

    public static T CreateJson<T>(string jsonName, T data) where T : class
    {
        try
        {
            string jsonFilePath = PathManager.GetCurrentPluginPath(jsonName);
            if (File.Exists(jsonFilePath))
            {
                throw new InvalidOperationException($"JSON file '{jsonName}' already exists at path: {jsonFilePath}");
            }
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(jsonFilePath, json);
            Plugin.Log.LogInfo($"Created JSON file '{jsonName}' at path: {jsonFilePath}");
            return data;
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"Error creating JSON file '{jsonName}': {ex.Message}");
            throw;
        }
    }
}
