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
        Instance = JsonManager.ReadJson<PluginSettings>(PathHelper.GetCurrentPluginPath(FileName));
        if (Instance == null) { Instance = new PluginSettings(); Save(); }
    }

    public static void Save() => JsonManager.WriteJson(PathHelper.GetCurrentPluginPath(FileName), Instance);
}
