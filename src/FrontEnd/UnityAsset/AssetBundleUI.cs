using UnityEngine;


namespace GreyAnnouncer;

public static class AssetBundleUI
{
    public static AssetBundle bundle;
    public static string bundlePath = PathManager.GetCurrentPluginPath("ui");
    public static void CreateUI()
    {   
        if (bundle == null)
        {
            bundle = AssetBundle.LoadFromFile(bundlePath);
        }

        // Load Perfab
        var uiPrefab = bundle.LoadAsset<GameObject>("UI");

        if (uiPrefab == null)
        {
            LogManager.LogError("Failed to load UI Prefab!");
            return;
        }

        // Initialize UI
        var uiObject = GameObject.Instantiate(uiPrefab);

        var canvas = UnityPathManager.FindCanvas();
        if (canvas != null)
        {
            uiObject.transform.SetParent(canvas.transform, false);

            uiObject.transform.SetAsLastSibling();

            LogManager.LogInfo("UI Loaded!");
        }
        else
        {
            LogManager.LogWarning("Canvas not found! UI may not display correctly.");
        }

    }
}