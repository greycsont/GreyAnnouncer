using System.Collections.Generic;

namespace greycsont.GreyAnnouncer;

public class RootObject
{
    public string[] AudioNames { get; set; }
}

public class JsonSetting
{
    public Dictionary<string, string[]> AudioNames { get; set; }
}