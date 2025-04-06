using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace greycsont.GreyAnnouncer{
    public class JsonReader{
        public static int MaxRank { get; private set;} = 7;

        private class ConfigData
        {
            [JsonPropertyName("maxRank")]
            private int maxRank { get; set; } = 10;

            public int MaxRank => maxRank;  // 通过公共属性提供访问
        }
        
        public static void LoadJson(string configPath)
        {
            try
            {
                if (!File.Exists(configPath))
                {
                    Plugin.Log.LogInfo($"Config file not found: {configPath}");
                    return;
                }

                string json = File.ReadAllText(configPath);
                var config = JsonSerializer.Deserialize<ConfigData>(json);

                MaxRank = config?.MaxRank ?? 10;
                Plugin.Log.LogInfo($"MaxRank set to {MaxRank}");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogInfo($"Failed to load config: {ex.Message}");
            }
        }
    }
}