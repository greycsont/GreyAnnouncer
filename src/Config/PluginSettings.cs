using System.IO;
using GreyAnnouncer.Util;

namespace GreyAnnouncer.Config;

public class PluginSettings
{
    private const string FileName = "settings.json";

    public static PluginSettings Instance { get; private set; }

    public float AudioSourceVolume { get; set; } = 1f;
    public bool IsLowPassFilterEnabled { get; set; } = true;
    public int AudioPlayOptions { get; set; } = 0;
    public int AudioLoadingStrategy { get; set; } = 0;
    public bool IsFFmpegSupportEnabled { get; set; } = false;

    public static void Initialize()
    {
        try { Instance = JsonManager.ReadJson<PluginSettings>(FileName); }
        catch (FileNotFoundException) { Instance = new PluginSettings(); Save(); }
    }

    public static void Save() => JsonManager.WriteJson(FileName, Instance);
}
