using UnityEngine;
using UnityEngine.InputSystem;

using GreyAnnouncer.Util;

namespace GreyAnnouncer.FrontEnd;

public class UIFactory : MonoBehaviour
{
    public GameObject mainPanel;

    public InputAction _toggleUIAction;

    public bool isAllowTrigger = false;

    public void Start()
    {
        LogHelper.LogDebug("Register UIFactory Actions");
        _toggleUIAction = new InputAction(binding: "<Keyboard>/backslash", interactions: "hold(duration=0.5)");
        _toggleUIAction.performed += _ => isAllowTrigger = true;
        _toggleUIAction.canceled += _ => TryOpenUI();

        _toggleUIAction.Enable();
    }
    
    public void TryOpenUI()
    {   
        LogHelper.LogDebug("TryOpenUI Called");
        if (isAllowTrigger == false) return;

        if (mainPanel == null)
            CreateUI();
        else
            mainPanel.SetActive(!mainPanel.activeSelf);

        isAllowTrigger = false;
    }

    public void CreateUI()
    {
        LogHelper.LogInfo("Loading UI...");

        var canvas = UnityPathManager.FindCanvas();
        if (canvas == null) return;

        mainPanel = new GameObject("MainPanelHost");
        
        mainPanel.transform.SetParent(canvas.transform, false);

        mainPanel.AddComponent<MainPanel>();

        var rt = mainPanel.GetComponent<RectTransform>() ?? mainPanel.AddComponent<RectTransform>();
        
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;

        rt.anchoredPosition = Vector2.zero;

        rt.offsetMax = new Vector2(-20, -20); // 右下角留出20像素的边距
        rt.offsetMin = new Vector2(20, 20);   // 左上角

        LogHelper.LogInfo("UI Initialized at full size with margins."); 
    }

}