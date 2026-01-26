using System;
using System.Reflection;

namespace GreyAnnouncer.Util;

public static class ReflectionManager
{   
    public static void LoadByReflection(string assemblyName, string methodName, object[] parameter = null)
    {
        try
        {
            Assembly assembly     = Assembly.GetExecutingAssembly();
            Type configuratorType = assembly.GetType(assemblyName);
            MethodInfo initialize = configuratorType.GetMethod( methodName, BindingFlags.Public | BindingFlags.Static);
            initialize?.Invoke(null, parameter);
        }
        catch (Exception ex)
        {
            LogHelper.LogError($"Failed to load {assemblyName}'s {methodName} by : {ex}");
        }
    }
}
