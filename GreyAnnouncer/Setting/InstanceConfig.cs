using System.Collections.Generic;
using BepInEx.Configuration;

namespace greycsont.GreyAnnouncer{
    
    public static class InstanceConfig
    {
        public static Dictionary<string, ConfigEntry<bool>> RankToggleDict = new();
        public static ConfigEntry<float>                    SharedRankPlayCooldown;    // Range : 0f ~ 10f
        public static ConfigEntry<float>                    IndividualRankPlayCooldown;    // Range : 0f ~ 114514f
        public static ConfigEntry<float>                    AudioSourceVolume; // Range : 0f ~ 1f
        public static ConfigEntry<bool>                     LowPassFilter_Enabled;
        
        public static readonly Dictionary<string, (string section, string name, object defaultValue, string description)> ConfigEntries = new()
        {
            // "Cooldown" section
            { "SharedRankCooldown",     ("Cooldown",      "Shared_rank_play_cooldown",           DEFAULT_SHARED_RANK_COOLDOWN,     "Shared rank play cooldown (in secs)") },
            { "IndividualRankCooldown", ("Cooldown",      "Individual_rank_play_cooldown",       DEFAULT_INDIVIDUAL_RANK_COOLDOWN, "Individual rank play cooldown (in secs)") },

            // "Enabled Style" section
            { "D",                      ("Enabled Style", "Destruction",                         DEFAULT_RANK_TOGGLED,             "Set to true to allow announce at D-rank") },
            { "C",                      ("Enabled Style", "Chaotic",                             DEFAULT_RANK_TOGGLED,             "Set to true to allow announce at C-rank") },
            { "B",                      ("Enabled Style", "Brutal",                              DEFAULT_RANK_TOGGLED,             "Set to true to allow announce at B-rank") },
            { "A",                      ("Enabled Style", "Anarchic",                            DEFAULT_RANK_TOGGLED,             "Set to true to allow announce at A-rank") },
            { "S",                      ("Enabled Style", "Supreme",                             DEFAULT_RANK_TOGGLED,             "Set to true to allow announce at S-rank") },
            { "SS",                     ("Enabled Style", "SSadistic",                           DEFAULT_RANK_TOGGLED,             "Set to true to allow announce at SS-rank") },
            { "SSS",                    ("Enabled Style", "SSShitstorm",                         DEFAULT_RANK_TOGGLED,             "Set to true to allow announce at SSS-rank") },
            { "U",                      ("Enabled Style", "ULTRAKILL",                           DEFAULT_RANK_TOGGLED,             "Set to true to allow announce at U-rank") },

            // "Audio" section
            { "AudioSourceVolume",      ("Audio",         "Audio_source_volume",                 DEFAULT_AUDIO_SOURCE_VOLUME,      "Volume of the Announcer ( Range : 0f ~ 1f )") },
            { "LowPassFilter",          ("Audio",         "Under_water_low_pass_filter_Enabled", DEFAULT_LOW_PASS_FILTER_ENABLED,  "Set to true to enable muffle effect when under water") }
        };
        public static void Initialize(Plugin plugin)
        {
            BindConfigEntryValues(plugin);
        }
        private static void BindConfigEntryValues(Plugin plugin)
        {
            foreach (var entry in ConfigEntries)
            {
                BindConfigEntry(plugin, entry.Key, entry.Value.section, entry.Value.name, entry.Value.defaultValue, entry.Value.description);
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

                if (section.Equals("Enabled Style")) RankToggleDict[key] = configEntry;
                else if (name == "Under_water_low_pass_filter_Enabled") LowPassFilter_Enabled = configEntry;
            }
            else if (defaultValue is float)
            {
                var configEntry = plugin.Config.Bind(
                    section,
                    key,
                    (float)defaultValue,
                    description
                );

                if      (name == "Shared_rank_play_cooldown")     SharedRankPlayCooldown     = configEntry;
                else if (name == "Individual_rank_play_cooldown") IndividualRankPlayCooldown = configEntry;
                else if (name == "Audio_source_volume")           AudioSourceVolume          = configEntry;
                
            }
        }

        public const float DEFAULT_SHARED_RANK_COOLDOWN     = 0f;
        public const float DEFAULT_INDIVIDUAL_RANK_COOLDOWN = 3f;
        public const bool  DEFAULT_RANK_TOGGLED             = true;
        public const float DEFAULT_AUDIO_SOURCE_VOLUME      = 1f;
        public const bool  DEFAULT_LOW_PASS_FILTER_ENABLED  = true;
        
    }

    

}