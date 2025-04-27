using System.IO;
using Newtonsoft.Json;

namespace greycsont.GreyAnnouncer;

public class JsonManager
{
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
        var rankSettings = new RankSettings
        {
            audioNames = new[] { "D", "C", "B", "A", "S", "SS", "SSS", "U" }
        };

        var rootObject = new RootObject { RankSettings = rankSettings };
        string json    = JsonConvert.SerializeObject(rootObject, Formatting.Indented);
        File.WriteAllText(JSON_FILE_PATH, json);
        Plugin.Log.LogInfo($"Initialized {JSON_NAME}");
    }

    public static void ReadJson()
    {
        string loadedJson    = File.ReadAllText(JSON_FILE_PATH);
        JsonSetting.Settings = JsonConvert.DeserializeObject<RootObject>(loadedJson);
    }
}
