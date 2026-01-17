using System.Collections.Generic;
using BepInEx.Configuration;

namespace GreyAnnouncer.Config;

public static class BepInExConfig
{
    public static ConfigEntry<float> audioSourceVolume; // Range : 0f ~ 1f

    public static ConfigEntry<bool> isLowPassFilterEnabled;

    public static ConfigEntry<int> audioPlayOptions;

    public static ConfigEntry<int> audioLoadingOptions;

    public static ConfigEntry<bool> isFFmpegSupportEnabled;
    

    public static readonly Dictionary<string, (string section, string name, object defaultValue, string description)> ConfigEntries = new()
    {
        // "Audio" section 
        { "AudioSourceVolume",      ("Audio",    "Audio_source_volume",                 DEFAULT_AUDIO_SOURCE_VOLUME,         "Volume of the Announcer ( Range : 0f ~ 1f )") },
        { "LowPassFilter",          ("Audio",    "Under_water_low_pass_filter_Enabled", DEFAULT_LOW_PASS_FILTER_ENABLED,     "Set to true to enable muffle effect when under water") },
        { "AudioPlayOptions",       ("Audio",    "Audio_Play_Option",                   DEFAULT_AUDIO_PLAY_OPTIONS,          "0 : new audio will override the old one, 1 : audio will not effect each other") },
        { "AudioLoadingOption",     ("Audio",    "Audio_Loading_Option",                DEFAULT_AUDIO_LOADING_OPTIONS,       "0 : load clip from file (less RAM more latency), 1 : preload clip to games (less latency more RAM)") },

        // "Advanced" section
        { "FFmpegSupport",          ("Advance",   "FFmpeg_Support",                     false,                               "Set to true to enable FFmpeg support for loading non-unity supported audios")}
    };
    
    public static void Initialize(Plugin plugin)
    {
        BindConfigEntryValues(plugin);
    }

    
    private static void BindConfigEntryValues(Plugin plugin)
    {
        foreach (var entry in ConfigEntries)
        {
            BindConfigEntry(
                plugin, 
                entry.Key, 
                entry.Value.section, 
                entry.Value.name, 
                entry.Value.defaultValue, 
                entry.Value.description
            );
        }
    }

    private static void BindConfigEntry(Plugin plugin,
                                        string key,
                                        string section,
                                        string name,
                                        object defaultValue,
                                        string description)
    {
        if (defaultValue is bool)
        {
            var configEntry = plugin.Config.Bind(
                section,
                name,
                (bool)defaultValue,
                description
            );
            if      (name == "Under_water_low_pass_filter_Enabled") isLowPassFilterEnabled      = configEntry;
            else if (name == "FFmpeg_Support")                      isFFmpegSupportEnabled      = configEntry;
        }
        else if (defaultValue is float)
        {
            var configEntry = plugin.Config.Bind(
                section,
                name,
                (float)defaultValue,
                description
            );
            if (name == "Audio_source_volume")           audioSourceVolume      = configEntry;
            
        }
        else if (defaultValue is string)
        {
            var configEntry = plugin.Config.Bind(
                section,
                name,
                (string)defaultValue,
                description
            );
        }
        else if (defaultValue is int)
        {
            var configEntry = plugin.Config.Bind(
                section,
                name,
                (int)defaultValue,
                description
            );
            if      (name == "Audio_Play_Option")    audioPlayOptions    = configEntry;
            else if (name == "Audio_Loading_Option") audioLoadingOptions = configEntry;
        }
        else
        {
            LogManager.LogError($"Unsupported type for config entry: {key}, Type: {defaultValue?.GetType()?.FullName ?? "null"}");
        }
    }
                           
    private static float _defaultAudioSourceVolume = 1f;
    public static float DEFAULT_AUDIO_SOURCE_VOLUME 
    {
        get => _defaultAudioSourceVolume;
        private set => _defaultAudioSourceVolume = value;
    }

    private static bool _defaultLowPassFilterEnabled = true;
    public static bool DEFAULT_LOW_PASS_FILTER_ENABLED 
    {
        get => _defaultLowPassFilterEnabled;
        private set => _defaultLowPassFilterEnabled = value;
    }

    private static int _defaultAudioPlayOptions = 0;
    public static int DEFAULT_AUDIO_PLAY_OPTIONS
    {
        get => _defaultAudioPlayOptions < 2 ? _defaultAudioPlayOptions : 0;
        private set => _defaultAudioPlayOptions = value;
    }

    private static int _defaultAudioLoadingOptions = 0;
    public static int DEFAULT_AUDIO_LOADING_OPTIONS
    {
        get => _defaultAudioLoadingOptions < 2 ? _defaultAudioLoadingOptions : 0;
        private set => _defaultAudioLoadingOptions = value;
    }
}

    
