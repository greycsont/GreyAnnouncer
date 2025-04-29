using System;
using System.IO;
using Newtonsoft.Json;

namespace greycsont.GreyAnnouncer;

public class JsonManager
{
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
