public interface ILog
{
    void LogInfo(object data);
    void LogWarning(object data);
    void LogError(object data);
}