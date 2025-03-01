using System;
using System.IO;
using System.Collections.Generic;

namespace greycsont.GreyAnnouncer
{
    public class ConfiginiAnalyzer{
        private Dictionary<string, Dictionary<string, string>> configData;

        public ConfiginiAnalyzer(string fileName)
        {
            configData = LoadIniFile(PathManager.GetCurrentPluginPath(fileName));
        }

        public Dictionary<string, Dictionary<string, string>> LoadIniFile(string filePath)
        {
            var config = new Dictionary<string, Dictionary<string, string>>();
            string currentSection = "";
            foreach (var line in File.ReadLines(filePath))
            {
                string trimmedLine = line.Trim();
                if (trimmedLine.StartsWith(";") || string.IsNullOrWhiteSpace(trimmedLine))
                    continue;
                if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                {
                    // get title of section
                    currentSection = trimmedLine.Substring(1, trimmedLine.Length - 2);
                    config[currentSection] = new Dictionary<string, string>();
                }
                else if (trimmedLine.Contains("="))
                {
                    var keyValue = trimmedLine.Split(new char[] { '=' }, 2);
                    var key = keyValue[0].Trim();
                    var value = keyValue[1].Trim();
                    if (!string.IsNullOrEmpty(currentSection))
                    {
                        config[currentSection][key] = value;
                    }
                }
            }
            return config;
        }

        public float GetCooldownDuration(){
            if (TryGetConfigValue("Timer", "cooldownDuration", out float cooldown) && cooldown >=0)
            {
                return cooldown;
            }
            
            DebugLogger.Log("Invalid cooldown value in INI file. Returning default value.");
            return 0.75f;
        }
        
        private bool TryGetConfigValue<T>(string section, string key, out T result){
            result = default;

            if (configData.ContainsKey(section) && configData[section].ContainsKey(key))
            {
                string valueStr = configData[section][key];

                try
                {
                    result = (T)Convert.ChangeType(valueStr, typeof(T));
                    return true;
                }
                catch (Exception ex)
                {
                    DebugLogger.LogError($"Failed to parse '{key}' in section [{section}]: {ex.Message}");
                }
            }

            return false;
        }

    }
}