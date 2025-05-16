
namespace greycsont.GreyAnnouncer;

public static class LogManager
{
    public static void LogInfo(object data)    => Plugin.log.LogInfo(data);
    public static void LogWarning(object data) => Plugin.log.LogWarning(data);
    public static void LogError(object data)   => Plugin.log.LogError(data);
}