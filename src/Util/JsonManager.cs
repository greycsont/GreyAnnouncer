using System;
using System.IO;
using Newtonsoft.Json;

namespace GreyAnnouncer.Util;

public static class JsonManager
{
    #region Public Methods
    public static T ReadJson<T>(string path) where T : class
    {
        if (!File.Exists(path))
            return null;

        return DeserializeJson<T>(path);
    }

    public static T WriteJson<T>(string path, T data) where T : class
    {
        var dir = Path.GetDirectoryName(path);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        return SerializeAndSaveJson(path, data);
    }
    #endregion


    #region Private Methods
    private static T DeserializeJson<T>(string path) where T : class
    {
        try
        {
            string json = File.ReadAllText(path);
            var result = JsonConvert.DeserializeObject<T>(json);

            if (result == null)
                throw new InvalidOperationException($"Failed to deserialize JSON at '{path}' into type {typeof(T).Name}");

            return result;
        }
        catch (FileNotFoundException)
        {
            return null;
        }
        catch (Exception ex)
        {
            LogHelper.LogError($"Error reading JSON '{path}': {ex.Message}");
            throw;
        }
    }

    private static T SerializeAndSaveJson<T>(string path, T data) where T : class
    {
        try
        {
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(path, json);
            return data;
        }
        catch (Exception ex)
        {
            LogHelper.LogError($"Error writing JSON '{path}': {ex.Message}");
            throw;
        }
    }
    #endregion
}