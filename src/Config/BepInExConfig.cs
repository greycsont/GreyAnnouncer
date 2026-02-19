using BepInEx.Configuration;
using System;
using System.Reflection;
using System.Linq;
using GreyAnnouncer.Util;

namespace GreyAnnouncer.Config;


public static class BepInExConfig
{
    [ConfigInfo("Audio", "Audio_source_volume", "Volume of the Announcer (Range: 0f ~ 1f)")]
    public static ConfigEntry<float> audioSourceVolume;

    [ConfigInfo("Audio", "Under_water_low_pass_filter_Enabled", "Set to true to enable muffle effect when under water")]
    public static ConfigEntry<bool> isLowPassFilterEnabled;

    [ConfigInfo("Audio", "Audio_Play_Option", "0: new audio will override the old one, 1: audio will not effect each other")]
    public static ConfigEntry<int> audioPlayOptions;

    [ConfigInfo("Audio", "Audio_Loading_Option", "0: load from file (less RAM), 1: preload (less latency)")]
    public static ConfigEntry<int> audioLoadingStrategy;

    [ConfigInfo("Audio", "Announcers_Path", "The folder path where announcer packs are stored")]
    public static ConfigEntry<string> announcersPath;

    [ConfigInfo("Advanced", "FFmpeg_Support", "Set to true to enable FFmpeg support for non-Unity audio formats")]
    public static ConfigEntry<bool> isFFmpegSupportEnabled;


    public static void Initialize(Plugin plugin)
    {
        var fields = typeof(BepInExConfig)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.GetCustomAttribute<ConfigInfoAttribute>() != null);

        foreach (var field in fields)
        {
            try
            {
                var attr = field.GetCustomAttribute<ConfigInfoAttribute>();
                object defaultValue = GetDefaultValueForField(field.Name);
                Type settingType = field.FieldType.GetGenericArguments()[0];

                var description = new ConfigDescription(attr.Description);

                var bindMethod = plugin.Config.GetType().GetMethods()
                    .First(m => m.Name == "Bind" && 
                                m.GetParameters().Length == 4 && 
                                m.GetParameters()[3].ParameterType == typeof(ConfigDescription))
                    .MakeGenericMethod(settingType);

                var result = bindMethod.Invoke(plugin.Config, new[] { 
                    attr.Section, 
                    attr.Name, 
                    defaultValue, 
                    description
                });

                field.SetValue(null, result);
            }
            catch (Exception ex)
            {
                LogHelper.LogError($"Failed to bind config field {field.Name}: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        LogHelper.LogInfo("BepInExConfig initialized successfully.");
    }

    private static object GetDefaultValueForField(string fieldName) => fieldName switch
    {
        nameof(audioSourceVolume)       => 1f,
        nameof(isLowPassFilterEnabled)  => true,
        nameof(audioPlayOptions)        => 0,
        nameof(audioLoadingStrategy)    => 0,
        nameof(isFFmpegSupportEnabled)  => false,
        nameof(announcersPath)          => PathHelper.GetCurrentPluginPath("announcers"),
        _ => throw new ArgumentException($"[Config] No default value defined for: {fieldName}")
    };
}