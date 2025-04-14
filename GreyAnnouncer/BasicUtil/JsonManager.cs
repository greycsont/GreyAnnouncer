using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace greycsont.GreyAnnouncer {
    public class RankSettings
    {
        public string[] ranks { get; set; }
        public string[] audioNames { get; set; }
    }
    public class RootObject
    {
        public RankSettings RankSettings { get; set; }
    }

    public class JsonManager
    {
        private const string JSON_NAME = "JsonSettings.json";
        public static void Initialze()
        {
            TryToFetchJson();
        }

        private static bool CheckDoesJsonExists() {
            if (!File.Exists("ExtensiveSetting.json")) return false;
            return true;
        }

        private static void TryToFetchJson(){
            if (CheckDoesJsonExists() == false){
                CreateJsonFile();
            }
        }

        private static void CreateJsonFile(){
            var rankSettings = new List<RankSettings>
            {
                new RankSettings { ranks = new[] {"D", "C", "B", "A", "S", "SS", "SSS", "U" } },
                new RankSettings { audioNames = new[] {"D", "C", "B", "A", "S", "SS", "SSS", "U" }}
            };
            string json = JsonConvert.SerializeObject(rankSettings, Formatting.Indented);
            File.WriteAllText(JSON_NAME, json);
            Plugin.Log.LogInfo($"Initialzied {JSON_NAME}");
        }

        private static void ReadJson(){
            string loadedJson = File.ReadAllText(JSON_NAME);
            var settings = JsonConvert.DeserializeObject<RootObject>(loadedJson);

            foreach (var rank in settings.RankSettings.ranks)
            {
                Plugin.Log.LogInfo(rank);
            }
        }
    }
}
