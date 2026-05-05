using System;
using System.IO;
using GreyAnnouncer.Util;

namespace GreyAnnouncer.Config;

public class PluginSettings
{
    private const string FileName = "settings.json";

    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "GreyAnnouncer",
        FileName
    );

    public static PluginSettings Instance { get; private set; }

    public float AudioSourceVolume { get; set; } = 1f;
    public bool IsLowPassFilterEnabled { get; set; } = true;
    public int AudioPlayOptions { get; set; } = 0;
    public int AudioLoadingStrategy { get; set; } = 0;
    public bool IsFFmpegSupportEnabled { get; set; } = false;
    public string AnnouncersPath { get; set; } = "./announcers";
    public float SpatialBlend { get; set; } = 0f;

    public static void Initialize()
    {
        Instance = JsonManager.ReadJson<PluginSettings>(SettingsPath);
        if (Instance == null) { Instance = new PluginSettings(); Save(); }
    }

    public static void Save() => JsonManager.WriteJson(SettingsPath, Instance);
}
