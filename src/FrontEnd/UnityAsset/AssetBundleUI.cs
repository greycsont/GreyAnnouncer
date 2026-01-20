using System.IO;
using UnityEngine;
using UnityEngine.Video;

using GreyAnnouncer.Util;

namespace GreyAnnouncer.FrontEnd;

public static class AssetBundleUI
{
    public static AssetBundle bundle;
    public static string bundlePath = PathManager.GetCurrentPluginPath("ui");
    public static void CreateUI()
    {   
        LogManager.LogInfo("Loading");
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

            Transform videoPlayerTransform = uiObject.transform.Find("Mask/VideoPlayer");

            if (videoPlayerTransform != null)
            {
                VideoPlayer vp = videoPlayerTransform.GetComponent<VideoPlayer>(); 

                if (vp == null)
                {
                    LogManager.LogInfo("Fuck VideoPlayer");
                    return;
                }
                vp.source = VideoSource.Url;
                vp.url = PathManager.GetCurrentPluginPath("output_h264.mp4");
                vp.Play();
            }       
        }
        else
        {
            LogManager.LogWarning("Canvas not found! UI may not display correctly.");
        }

    }
}