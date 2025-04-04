using System.Collections.Generic;
using BepInEx.Configuration;

namespace greycsont.GreyAnnouncer{
    
    public static class InstanceConfig
    {
        public static Dictionary<string, ConfigEntry<bool>> RankToggleDict = new();
        public static ConfigEntry<float> SharedRankPlayCooldown;    // Range : 0f ~ 10f
        public static ConfigEntry<float> IndividualRankPlayCooldown;    // Range : 0f ~ 114514f
        public static ConfigEntry<float> AudioSourceVolume; // Range : 0f ~ 1f
        public static ConfigEntry<bool> LowPassFilter_Enabled;
        public static void Initialize(Plugin plugin)
        {
            BindConfigEntryValues(plugin);
        }

        private static void BindConfigEntryValues(Plugin plugin){
            BindCooldownConfigEntryValues(plugin);
            BindRankEnabledConfigEntryValues(plugin);
            BindAudioRelatedConfigEntryValues(plugin);
        }
        private static void BindCooldownConfigEntryValues(Plugin plugin){
            var cooldownNames = new Dictionary<string, string>
            {
                { "Shared", "Shared"},
                { "Individual", "Individual"}
            };
            SharedRankPlayCooldown = plugin.Config.Bind(
                "Cooldown", 
                "Shared_rank_play_cooldown", 
                DEFAULT_SHARED_RANK_COOLDOWN, 
                "Shared rank play cooldown(in secs)"
            );

            IndividualRankPlayCooldown = plugin.Config.Bind(
                "Cooldown",
                "Individual_rank_play_cooldown",
                DEFAULT_INDIVIDUAL_RANK_COOLDOWN,
                "Individual rank play cooldown(in secs)"
            );
        }
        private static void BindRankEnabledConfigEntryValues(Plugin plugin){
            var rankNames = new Dictionary<string, string>
            {
                { "D", "Destruction" },
                { "C", "Chaotic" },
                { "B", "Brutal" },
                { "A", "Anarchic" },
                { "S", "Supreme" },
                { "SS", "SSadistic" },
                { "SSS", "SSShitstorm" },
                { "U", "ULTRAKILL" }
            };

            foreach (var rank in rankNames)
            {
                var configEntry = plugin.Config.Bind(
                    "Enabled Style",
                    rank.Value,
                    DEFAULT_RANK_TOGGLED,
                    $"Set to true to allow announce at {rank.Key}-rank"
                );

                RankToggleDict[rank.Key] = configEntry;
            }
        }
        private static void BindAudioRelatedConfigEntryValues(Plugin plugin){
            AudioSourceVolume = plugin.Config.Bind(
                "Audio",
                "Audio_source_volume",
                DEFAULT_AUDIO_SOURCE_VOLUME,
                "Volume of the Announcer ( Range : 0f ~ 1f )"
            );

            LowPassFilter_Enabled = plugin.Config.Bind(
                "Audio",
                "Under_water_low_pass_filter_Enabled",
                DEFAULT_LOW_PASS_FILTER_ENABLED,
                "Set to true to enable muffle effect when under water"
            );
        }

        public const float DEFAULT_SHARED_RANK_COOLDOWN = 0f;
        public const float DEFAULT_INDIVIDUAL_RANK_COOLDOWN = 3f;
        public const bool DEFAULT_RANK_TOGGLED = true;
        public const float DEFAULT_AUDIO_SOURCE_VOLUME = 1f;
        public const bool DEFAULT_LOW_PASS_FILTER_ENABLED = true;
        
    }
}