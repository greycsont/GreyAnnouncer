using BepInEx;
using System;

namespace GreyAnnouncer;

internal static class PluginDependencies
{
    public const string PLUGINCONFIGURATOR_GUID = "com.eternalUnion.pluginConfigurator";
    public static void LoadIfPluginExists(string guid, string moduleName, Action loader)
    {
        if (!BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(guid))
        {
            LogManager.LogWarning($"Plugin {guid} not loaded, skipping {moduleName}");
            return;
        }

        try
        {
            loader?.Invoke();
            LogManager.LogInfo($"Loaded optional module: {moduleName}");
        }
        catch (Exception ex)
        {
            LogManager.LogError($"Exception while loading {moduleName}: {ex.Message}");
        }
    }
}