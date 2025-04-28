namespace greycsont.GreyAnnouncer;

public class RootObject
{
    public string[] AudioNames { get; set; }
}

public static class JsonSetting
{
    public static RootObject Settings { get; set; } = new RootObject();
}

public class JsonSetting_v2
{
    public string[] AudioNames { get; set; }
}