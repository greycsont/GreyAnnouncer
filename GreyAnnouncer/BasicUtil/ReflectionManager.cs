using System;
using System.Reflection;

namespace greycsont.GreyAnnouncer;

public class ReflectionManager
{   
    public static void LoadByReflection(string assemblyName, string methodName)
    {
        try
        {
            Assembly assembly     = Assembly.GetExecutingAssembly();
            Type configuratorType = assembly.GetType(assemblyName);
            MethodInfo initialize = configuratorType.GetMethod( methodName, BindingFlags.Public | BindingFlags.Static);
            initialize?.Invoke(null, null);
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"Failed to load optional module by : {ex}");
        }
    }
}
