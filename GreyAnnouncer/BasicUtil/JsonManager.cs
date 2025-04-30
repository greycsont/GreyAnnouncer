using System;
using System.IO;
using Newtonsoft.Json;

namespace greycsont.GreyAnnouncer;

public class JsonManager
{
    #region Public Methods
    public static T ReadJson<T>(string jsonName) where T : class
    {
        ValidateJsonName(jsonName);
        var jsonFilePath = PathManager.GetCurrentPluginPath(jsonName);
        ValidateFileExists(jsonFilePath, jsonName);
        
        return DeserializeJson<T>(jsonFilePath, jsonName);
    }

    public static T CreateJson<T>(string jsonName, T data) where T : class
    {
        ValidateJsonName(jsonName);
        var jsonFilePath = PathManager.GetCurrentPluginPath(jsonName);
        ValidateFileDoesNotExist(jsonFilePath, jsonName);
        
        return SerializeAndSaveJson(jsonFilePath, jsonName, data);
    }
    #endregion


    #region Private Methods
    private static void ValidateJsonName(string jsonName)
    {
        if (string.IsNullOrEmpty(jsonName))
        {
            throw new ArgumentException("JSON file name cannot be null or empty");
        }
    }

    private static void ValidateFileExists(string path, string name)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"JSON file '{name}' not found at path: {path}");
        }
    }

    private static void ValidateFileDoesNotExist(string path, string name)
    {
        if (File.Exists(path))
        {
            throw new InvalidOperationException($"JSON file '{name}' already exists at path: {path}");
        }
    }

    private static T DeserializeJson<T>(string path, string name) where T : class
    {
        try
        {
            string json = File.ReadAllText(path);
            var result = JsonConvert.DeserializeObject<T>(json);

            if (result == null)
            {
                throw new InvalidOperationException($"Failed to deserialize JSON file '{name}' into type {typeof(T).Name}");
            }

            return result;
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"Error reading JSON file '{name}': {ex.Message}");
            throw;
        }
    }

    private static T SerializeAndSaveJson<T>(string path, string name, T data) where T : class
    {
        try
        {
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(path, json);
            Plugin.Log.LogInfo($"Created JSON file '{name}' at path: {path}");
            return data;
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"Error creating JSON file '{name}': {ex.Message}");
            throw;
        }
    }
    #endregion
}