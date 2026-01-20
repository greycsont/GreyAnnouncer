using System;
using System.IO;
using Newtonsoft.Json;

namespace GreyAnnouncer.Util;

public static class JsonManager
{
    #region Public Methods
    public static T ReadJson<T>(string jsonName) where T : class
    {
        ValidateJsonName(jsonName);
        
        PathManager.EnsureDirectoryExists(PathManager.GetCurrentPluginPath()); 

        var jsonFilePath = PathManager.GetCurrentPluginPath(jsonName);

        var dir = Path.GetDirectoryName(jsonFilePath);
        if (!Directory.Exists(dir))
        {
            LogManager.LogError($"JSON '{jsonName}' not found at path: {dir}");
            return null;
        }
        
        return DeserializeJson<T>(jsonFilePath, jsonName);
    }

    public static T CreateJson<T>(string jsonName, T data) where T : class 
    { 
        ValidateJsonName(jsonName); 

        PathManager.EnsureDirectoryExists(PathManager.GetCurrentPluginPath()); 

        var jsonFilePath = PathManager.GetCurrentPluginPath(jsonName);

        var dir = Path.GetDirectoryName(jsonFilePath);
        if (Directory.Exists(dir))
        {
            LogManager.LogError($"JSON '{jsonName}' already exists at path: {dir}");
            return null;
        }

        return SerializeAndSaveJson(jsonFilePath, jsonName, data); 
    }

    public static T WriteJson<T>(string jsonName, T data) where T : class
    {
        ValidateJsonName(jsonName);
        var jsonFilePath = PathManager.GetCurrentPluginPath(jsonName);
        
        var dir = Path.GetDirectoryName(jsonFilePath);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        
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
            LogManager.LogError($"Error reading JSON file '{name}': {ex.Message}");
            throw;
        }
    }

    private static T SerializeAndSaveJson<T>(string path, string name, T data) where T : class
    {
        try
        {
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(path, json);
            LogManager.LogInfo($"Writed JSON file '{name}' at path: {path}");
            return data;
        }
        catch (Exception ex)
        {
            LogManager.LogError($"Error writing JSON file '{name}': {ex.Message}");
            throw;
        }
    }
    #endregion
}