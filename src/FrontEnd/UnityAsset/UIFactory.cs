using System.IO;
using UnityEngine;
using UnityEngine.Video;

using GreyAnnouncer.Util;

namespace GreyAnnouncer.FrontEnd;

public static class UIFactory
{
    public static void CreateUI()
    {   
        LogHelper.LogInfo("Loading UI...");

        var canvas = UnityPathManager.FindCanvas();
        if (canvas == null) return;

        GameObject go = new GameObject("MainPanelHost");
        
        go.transform.SetParent(canvas.transform, false);

        go.AddComponent<MainPanel>();

        var rt = go.GetComponent<RectTransform>() ?? go.AddComponent<RectTransform>();
        
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;

        rt.anchoredPosition = Vector2.zero;

        rt.offsetMax = new Vector2(-20, -20); // 右下角留出20像素的边距
        rt.offsetMin = new Vector2(20, 20);   // 左上角

        LogHelper.LogInfo("UI Initialized at full size with margins."); 
    }
}