using System;
using System.Reflection;

namespace GreyAnnouncer;

[EntryPoint]
internal static class PluginDependencies
{
    public const string PLUGINCONFIGURATOR_GUID = "com.eternalUnion.pluginConfigurator";
    public static void LoadIfPluginExists(string guid, string moduleName, Action loader)
    {
        if (!BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(guid))
        {
            LogHelper.LogWarning($"Plugin {guid} not loaded, skipping {moduleName}");
            return;
        }

        try
        {
            loader?.Invoke();
            LogHelper.LogInfo($"Loaded optional module: {moduleName}");
        }
        catch (Exception ex)
        {
            LogHelper.LogError($"Exception while loading {moduleName}: {ex.Message}");
        }
    }

    [EntryPoint]
    public static void CheckPluginLoaded()
    {
        PluginDependencies.LoadIfPluginExists(
            PluginDependencies.PLUGINCONFIGURATOR_GUID,
            "PluginConfiguratorEntry",
            () => LoadByReflection("GreyAnnouncer.FrontEnd.PluginConfiguratorEntry", "Initialize")
        );
    }

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