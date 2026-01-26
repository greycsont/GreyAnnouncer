using System.IO;
using UnityEngine;
using UnityEngine.Video;

using GreyAnnouncer.Util;

namespace GreyAnnouncer.FrontEnd;

public static class AssetBundleUI
{
    public static AssetBundle bundle;
    public static string bundlePath = PathHelper.GetCurrentPluginPath("ui");
    public static void CreateUI()
    {   
        LogHelper.LogInfo("Loading");
        if (bundle == null)
        {
            bundle = AssetBundle.LoadFromFile(bundlePath);
        }

        // Load Perfab
        var uiPrefab = bundle.LoadAsset<GameObject>("UI");

        if (uiPrefab == null)
        {
            LogHelper.LogError("Failed to load UI Prefab!");
            return;
        }

        // Initialize UI
        var uiObject = GameObject.Instantiate(uiPrefab);

        var canvas = UnityPathManager.FindCanvas();
        if (canvas != null)
        {
            uiObject.transform.SetParent(canvas.transform, false);

            uiObject.transform.SetAsLastSibling();

            LogHelper.LogInfo("UI Loaded!");

            Transform videoPlayerTransform = uiObject.transform.Find("Mask/VideoPlayer");

            if (videoPlayerTransform != null)
            {
                VideoPlayer vp = videoPlayerTransform.GetComponent<VideoPlayer>(); 

                if (vp == null)
                {
                    LogHelper.LogInfo("Fuck VideoPlayer");
                    return;
                }
                vp.source = VideoSource.Url;
                vp.url = PathHelper.GetCurrentPluginPath("output_h264.mp4");
                vp.Play();
            }       
        }
        else
        {
            LogHelper.LogWarning("Canvas not found! UI may not display correctly.");
        }

    }
}