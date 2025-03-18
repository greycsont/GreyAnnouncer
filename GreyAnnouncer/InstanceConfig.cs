using BepInEx.Configuration;

namespace greycsont.GreyAnnouncer{
    public static class InstanceConfig
    {
        public const float DEFAULT_SHARED_RANK_COOLDOWN = 0f;
        public const float DEFAULT_INDIVIDUAL_RANK_COOLDOWN = 3f;
        public const bool DEFAULT_RANK_FILTER_ENABLED = true;
        public static ConfigEntry<float> SharedRankPlayCooldown;
        public static ConfigEntry<float> IndividualRankPlayCooldown;
        public static ConfigEntry<bool> RankD_Enabled;
        public static ConfigEntry<bool> RankC_Enabled;
        public static ConfigEntry<bool> RankB_Enabled;
        public static ConfigEntry<bool> RankA_Enabled;
        public static ConfigEntry<bool> RankS_Enabled;
        public static ConfigEntry<bool> RankSS_Enabled;
        public static ConfigEntry<bool> RankSSS_Enabled;
        public static ConfigEntry<bool> RankU_Enabled;
        public static void Initialize(Plugin plugin)
        {
            BindConfigEntryValues(plugin);
        }

        private static void BindConfigEntryValues(Plugin plugin){
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
            
            RankD_Enabled = plugin.Config.Bind(
                "Enabled Style", 
                "Destruction", 
                DEFAULT_RANK_FILTER_ENABLED, 
                "Set to true to allow announce at D-rank"
            );

            RankC_Enabled = plugin.Config.Bind(
                "Enabled Style", 
                "Chaotic", 
                DEFAULT_RANK_FILTER_ENABLED, 
                "Set to true to allow announce at C-rank"
            );

            RankB_Enabled = plugin.Config.Bind(
                "Enabled Style", 
                "Brutal", 
                DEFAULT_RANK_FILTER_ENABLED, 
                "Set to true to allow announce at B-rank"
            );

            RankA_Enabled = plugin.Config.Bind(
                "Enabled Style", 
                "Anarchic", 
                DEFAULT_RANK_FILTER_ENABLED, 
                "Set to true to allow announce at A-rank"
            );

            RankS_Enabled = plugin.Config.Bind(
                "Enabled Style", 
                "Supreme", 
                DEFAULT_RANK_FILTER_ENABLED, 
                "Set to true to allow announce at S-rank"
            );
            RankSS_Enabled = plugin.Config.Bind(
                "Enabled Style", 
                "SSadistic", 
                DEFAULT_RANK_FILTER_ENABLED, 
                "Set to true to allow announce at SS-rank"
            );

            RankSSS_Enabled = plugin.Config.Bind(
                "Enabled Style", 
                "SSShitstorm", 
                DEFAULT_RANK_FILTER_ENABLED, 
                "Set to true to allow announce at SSS-rank"
            );

            RankU_Enabled = plugin.Config.Bind(
                "Enabled Style", 
                "ULTRAKILL", 
                DEFAULT_RANK_FILTER_ENABLED, 
                "Set to true to allow announce at U-rank"
            );
        }
    }
}