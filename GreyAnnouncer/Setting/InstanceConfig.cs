using System.Collections.Generic;
using BepInEx.Configuration;

namespace greycsont.GreyAnnouncer;

public static class InstanceConfig
{
    public static          ConfigEntry<float>                    sharedPlayCooldown;    // Range : 0f ~ 10f
    public static          ConfigEntry<float>                    individualPlayCooldown;    // Range : 0f ~ 114514f
    public static          ConfigEntry<float>                    audioSourceVolume; // Range : 0f ~ 1f
    public static          ConfigEntry<bool>                     isLowPassFilterEnabled;
    public static          ConfigEntry<string>                   audioFolderPath;
    public static          ConfigEntry<int>                      audioPlayOptions;
    public static          ConfigEntry<int>                      audioLoadingOptions;

    public static readonly Dictionary<string, (string section, string name, object defaultValue, string description)> ConfigEntries = new()
    {
        // "Cooldown" section
        { "SharedRankCooldown",     ("Cooldown",      "Shared_rank_play_cooldown",           DEFAULT_SHARED_PLAY_COOLDOWN,     "Shared rank play cooldown (in secs)") },
        { "IndividualRankCooldown", ("Cooldown",      "Individual_rank_play_cooldown",       DEFAULT_INDIVIDUAL_PLAY_COOLDOWN, "Individual rank play cooldown (in secs)") },

        // "Audio" section
        { "AudioSourceVolume",      ("Audio",         "Audio_source_volume",                 DEFAULT_AUDIO_SOURCE_VOLUME,      "Volume of the Announcer ( Range : 0f ~ 1f )") },
        { "LowPassFilter",          ("Audio",         "Under_water_low_pass_filter_Enabled", DEFAULT_LOW_PASS_FILTER_ENABLED,  "Set to true to enable muffle effect when under water") },
        { "AudioFolderPath",        ("Audio",         "Audio_folder_path",                   DEFAULT_AUDIO_FOLDER_PATH,        "Path to the audio folder") },
        { "AudioPlayOptions",       ("Audio",         "Audio_Play_Option",                   DEFAULT_AUDIO_PLAY_OPTIONS,       "0 : new audio will override the old one, 1 : audio will not effect each other") },
        { "AudioLoadingOption",     ("Audio",         "Audio_Loading_Option",                DEFAULT_AUDIO_LOADING_OPTIONS,     "0 : load clip from file (less RAM more latency), 1 : preload clip to games (less latency more RAM)") }
    };
    
    public static void Initialize(Plugin plugin)
    {
        DEFAULT_AUDIO_FOLDER_PATH = PathManager.GetCurrentPluginPath("Audio");
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

    private static void BindConfigEntry(Plugin plugin, string key, string section, string name, object defaultValue, string description)
    {
        if (defaultValue is bool)
        {
            var configEntry = plugin.Config.Bind(
                section,
                name,
                (bool)defaultValue,
                description
            );
            if (name == "Under_water_low_pass_filter_Enabled") isLowPassFilterEnabled = configEntry;
        }
        else if (defaultValue is float)
        {
            var configEntry = plugin.Config.Bind(
                section,
                name,
                (float)defaultValue,
                description
            );

            if      (name == "Shared_rank_play_cooldown")     sharedPlayCooldown     = configEntry;
            else if (name == "Individual_rank_play_cooldown") individualPlayCooldown = configEntry;
            else if (name == "Audio_source_volume")           audioSourceVolume          = configEntry;
            
        }
        else if (defaultValue is string)
        {
            var configEntry = plugin.Config.Bind(
                section,
                name,
                (string)defaultValue,
                description
            );

            if (name == "Audio_folder_path") audioFolderPath = configEntry;
        }
        else if (defaultValue is int)
        {
            var configEntry = plugin.Config.Bind(
                section,
                name,
                (int)defaultValue,
                description
            );
            if (name == "Audio_Play_Option") audioPlayOptions = configEntry;
            else if (name == "Audio_Loading_Option") audioLoadingOptions = configEntry;
        }
        else
        {
            Plugin.log.LogError($"Unsupported type for config entry: {key}, Type: {defaultValue?.GetType()?.FullName ?? "null"}");
        }
    }
                           
    private static float m_defaultSharedPlayCooldown = 0f;
    public static float DEFAULT_SHARED_PLAY_COOLDOWN 
    {
        get => m_defaultSharedPlayCooldown;
        private set => m_defaultSharedPlayCooldown = value;
    }

    private static float m_defaultIndividualPlayCooldown = 3f;
    public static float DEFAULT_INDIVIDUAL_PLAY_COOLDOWN 
    {
        get => m_defaultIndividualPlayCooldown;
        private set => m_defaultIndividualPlayCooldown = value;
    }

    private static float m_defaultAudioSourceVolume = 1f;
    public static float DEFAULT_AUDIO_SOURCE_VOLUME 
    {
        get => m_defaultAudioSourceVolume;
        private set => m_defaultAudioSourceVolume = value;
    }

    private static bool m_defaultLowPassFilterEnabled = true;
    public static bool DEFAULT_LOW_PASS_FILTER_ENABLED 
    {
        get => m_defaultLowPassFilterEnabled;
        private set => m_defaultLowPassFilterEnabled = value;
    }

    private static string m_audioFolderPath;
    public static string DEFAULT_AUDIO_FOLDER_PATH
    {
        get => m_audioFolderPath ?? PathManager.GetCurrentPluginPath("Audio");
        private set => m_audioFolderPath = value;
    }

    private static int m_defaultAudioPlayOptions = 0;
    public static int DEFAULT_AUDIO_PLAY_OPTIONS
    {
        get => m_defaultAudioPlayOptions < 2 ? m_defaultAudioPlayOptions : 0;
        private set => m_defaultAudioPlayOptions = value;
    }

    private static int m_defaultAudioLoadingOptions = 0;
    public static int DEFAULT_AUDIO_LOADING_OPTIONS
    {
        get => m_defaultAudioLoadingOptions < 2 ? m_defaultAudioLoadingOptions : 0;
        private set => m_defaultAudioLoadingOptions = value;
    }
}

    
