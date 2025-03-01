using UnityEngine;

namespace greycsont.GreyAnnouncer
{
    public static class DebugLogger
    {
        private static bool isDebugEnabled = true;
        public static void Log(string message)
        {
            if (isDebugEnabled)
            {
                Debug.Log($"{message}");
            }
        }
        public static void LogWarning(string message)
        {
            if (isDebugEnabled)
            {
                Debug.LogWarning($"{message}");
            }
        }
        public static void LogError(string message)
        {
            if (isDebugEnabled)
            {
                Debug.LogError($"{message}");
            }
        }
    }
}
