using System;
using System.IO;
using System.Text.Json;

namespace greycsont.GreyAnnouncer
{
    public class JsonWriter
    {
        // 写入 JSON 配置文件的方法
        public static void Write(string configPath, int maxRank)
        {
            try
            {
                // 创建一个对象来存储需要序列化的数据
                var config = new ConfigData
                {
                    MaxRank = maxRank
                };

                // 使用 JsonSerializer 将对象序列化为 JSON 字符串
                string json = JsonSerializer.Serialize(config, new JsonSerializerOptions
                {
                    WriteIndented = true  // 使 JSON 格式更加可读（缩进）
                });

                // 将 JSON 字符串写入文件
                File.WriteAllText(configPath, json);

                Console.WriteLine($"[GreyAnnouncer] Config saved to {configPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GreyAnnouncer] Failed to write config: {ex.Message}");
            }
        }

        // 内部配置数据类
        private class ConfigData
        {
            public int MaxRank { get; set; }
        }
    }
}
