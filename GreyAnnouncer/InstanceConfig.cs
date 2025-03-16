using BepInEx.Configuration;

namespace greycsont.GreyAnnouncer{
    public static class InstanceConfig
    {
        public static ConfigEntry<float> AnnounceCooldown;
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
            AnnounceCooldown = plugin.Config.Bind(
                "General", 
                "Cooldown", 
                0f, 
                "Cooldown timer (in sec)"
            );
            
            RankD_Enabled = plugin.Config.Bind(
                "Enabled Style", 
                "Destruction", 
                true, 
                "Set to true to allow announce at D-rank"
            );

            RankC_Enabled = plugin.Config.Bind(
                "Enabled Style", 
                "Chaotic", 
                true, 
                "Set to true to allow announce at C-rank"
            );

            RankB_Enabled = plugin.Config.Bind(
                "Enabled Style", 
                "Brutal", 
                true, 
                "Set to true to allow announce at B-rank"
            );

            RankA_Enabled = plugin.Config.Bind(
                "Enabled Style", 
                "Anarchic", 
                true, 
                "Set to true to allow announce at A-rank"
            );

            RankS_Enabled = plugin.Config.Bind(
                "Enabled Style", 
                "Supreme", 
                true, 
                "Set to true to allow announce at S-rank"
            );
            RankSS_Enabled = plugin.Config.Bind(
                "Enabled Style", 
                "SSadistic", 
                true, 
                "Set to true to allow announce at SS-rank"
            );

            RankSSS_Enabled = plugin.Config.Bind(
                "Enabled Style", 
                "SSShitstorm", 
                true, 
                "Set to true to allow announce at SSS-rank"
            );

            RankU_Enabled = plugin.Config.Bind(
                "Enabled Style", 
                "ULTRAKILL", 
                true, 
                "Set to true to allow announce at U-rank"
            );
        }
    }
}