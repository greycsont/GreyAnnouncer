using System;

namespace GreyAnnouncer;

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
}