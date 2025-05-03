using System.Collections.Generic;
using BepInEx.Configuration;

namespace greycsont.GreyAnnouncer;

public static class InstanceConfig
{
    public static          ConfigEntry<float>                    SharedRankPlayCooldown;    // Range : 0f ~ 10f
    public static          ConfigEntry<float>                    IndividualRankPlayCooldown;    // Range : 0f ~ 114514f
    public static          ConfigEntry<float>                    AudioSourceVolume; // Range : 0f ~ 1f
    public static          ConfigEntry<bool>                     LowPassFilter_Enabled;
    public static          ConfigEntry<string>                   AudioFolderPath;
    public static          ConfigEntry<int>                      AudioPlayOptions;

    public static readonly Dictionary<string, (string section, string name, object defaultValue, string description)> ConfigEntries = new()
    {
        // "Cooldown" section
        { "SharedRankCooldown",     ("Cooldown",      "Shared_rank_play_cooldown",           DEFAULT_SHARED_RANK_COOLDOWN,     "Shared rank play cooldown (in secs)") },
        { "IndividualRankCooldown", ("Cooldown",      "Individual_rank_play_cooldown",       DEFAULT_INDIVIDUAL_RANK_COOLDOWN, "Individual rank play cooldown (in secs)") },

        // "Audio" section
        { "AudioSourceVolume",      ("Audio",         "Audio_source_volume",                 DEFAULT_AUDIO_SOURCE_VOLUME,      "Volume of the Announcer ( Range : 0f ~ 1f )") },
        { "LowPassFilter",          ("Audio",         "Under_water_low_pass_filter_Enabled", DEFAULT_LOW_PASS_FILTER_ENABLED,  "Set to true to enable muffle effect when under water") },
        { "AudioFolderPath",        ("Audio",         "Audio_folder_path",                   DEFAULT_AUDIO_FOLDER_PATH,        "Path to the audio folder") },
        { "AudioPlayOptions",       ("Audio",         "Audio_Play_Option",                   DEFAULT_AUDIO_PLAY_OPTIONS,       "0 : new audio will override the old one, 1 : audio will not effect each other") }
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
            if (name == "Under_water_low_pass_filter_Enabled") LowPassFilter_Enabled = configEntry;
        }
        else if (defaultValue is float)
        {
            var configEntry = plugin.Config.Bind(
                section,
                name,
                (float)defaultValue,
                description
            );

            if      (name == "Shared_rank_play_cooldown")     SharedRankPlayCooldown     = configEntry;
            else if (name == "Individual_rank_play_cooldown") IndividualRankPlayCooldown = configEntry;
            else if (name == "Audio_source_volume")           AudioSourceVolume          = configEntry;
            
        }
        else if (defaultValue is string)
        {
            var configEntry = plugin.Config.Bind(
                section,
                name,
                (string)defaultValue,
                description
            );

            if (name == "Audio_folder_path") AudioFolderPath = configEntry;
        }
        else if (defaultValue is int)
        {
            var configEntry = plugin.Config.Bind(
                section,
                name,
                (int)defaultValue,
                description
            );
            if (name == "Audio_Play_Option") AudioPlayOptions = configEntry;
        }
        else
        {
            Plugin.Log.LogError($"Unsupported type for config entry: {key}, Type: {defaultValue?.GetType()?.FullName ?? "null"}");
        }
    }
                           
    private static float _defaultSharedRankCooldown = 0f;
    public static float DEFAULT_SHARED_RANK_COOLDOWN 
    {
        get => _defaultSharedRankCooldown;
        private set => _defaultSharedRankCooldown = value;
    }

    private static float _defaultIndividualRankCooldown = 3f;
    public static float DEFAULT_INDIVIDUAL_RANK_COOLDOWN 
    {
        get => _defaultIndividualRankCooldown;
        private set => _defaultIndividualRankCooldown = value;
    }

    private static bool _defaultRankToggled = true;
    public static bool DEFAULT_RANK_TOGGLED 
    {
        get => _defaultRankToggled;
        private set => _defaultRankToggled = value;
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

    private static string _audioFolderPath;
    public static string DEFAULT_AUDIO_FOLDER_PATH
    {
        get => _audioFolderPath ?? PathManager.GetCurrentPluginPath("Audio");
        private set => _audioFolderPath = value;
    }

    private static int _defaultAudioPlayOptions = 0;
    public static int DEFAULT_AUDIO_PLAY_OPTIONS
    {
        get => _defaultAudioPlayOptions < 2 ? _defaultAudioPlayOptions : 0;
        private set => _defaultAudioPlayOptions = value;
    }
}

    
