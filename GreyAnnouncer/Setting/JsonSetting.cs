using System.Collections.Generic;

namespace greycsont.GreyAnnouncer
{
    public class RankSettings
    {
        public string[] ranks { get; set; }
        public string[] audioNames { get; set; }
    }

    public class RootObject
    {
        public RankSettings RankSettings { get; set; }
    }

    public static class JsonSetting
    {
        public static RootObject Settings { get; set; } = new RootObject();
    }
}