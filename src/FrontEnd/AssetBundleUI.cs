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

        // ② 加载 Prefab
        var uiPrefab = bundle.LoadAsset<GameObject>("UI"); // Prefab 名字

        if (uiPrefab == null)
        {
            LogManager.LogError("Failed to load UI Prefab!");
            return;
        }

        // ③ 实例化 UI
        var uiObject = GameObject.Instantiate(uiPrefab);

        var canvas = UnityPathManager.FindCanvas();
        if (canvas != null)
        {
            /*var rect = uiObject.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = Vector2.zero;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }*/
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