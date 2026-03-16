using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GreyAnnouncer.AnnouncerAPI;

namespace GreyAnnouncer.FrontEnd;

public class MainPanel : MonoBehaviour
{
    // Left-panel nav buttons
    private Transform _navContent;

    // Right-panel: one AnnouncerPanel per IAnnouncer
    private Transform _rightPanelRoot;
    private readonly List<AnnouncerPanel> _panels = [];
    private readonly List<GameObject> _navButtons = [];

    public void Awake()
    {
        SetFullStretch(GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>());

        // ── Left panel — 40 % width ──────────────────────────
        var leftPanel = CreatePanel("LeftPanel", 0f, 0.4f, new Color(0.18f, 0.18f, 0.18f, 0.95f));
        _navContent = BuildNavScrollView(leftPanel.transform);

        // ── Right panel — 60 % width ─────────────────────────
        var rightPanel = CreatePanel("RightPanel", 0.4f, 1f, new Color(0.1f, 0.1f, 0.1f, 1f));
        _rightPanelRoot = rightPanel.transform;

        // Populate panels for announcers already registered
        foreach (var announcer in AnnouncerManager.GetAllAnnouncers())
            AddAnnouncerPanel(announcer);

        // Listen for future registrations (announcers created after the UI)
        AnnouncerManager.OnRegistered += AddAnnouncerPanel;
    }

    private void OnDestroy()
    {
        AnnouncerManager.OnRegistered -= AddAnnouncerPanel;
    }

    // ── Panel management ─────────────────────────────────────

    private void AddAnnouncerPanel(IAnnouncer announcer)
    {
        int index = _panels.Count;

        // Nav button (left panel)
        var btnObj = DefaultControls.CreateButton(new DefaultControls.Resources());
        btnObj.name = "NavBtn_" + announcer.title;
        btnObj.transform.SetParent(_navContent, false);
        btnObj.AddComponent<LayoutElement>().preferredHeight = 44;
        SetButtonLabel(btnObj, announcer.title, 14);
        StyleNavButton(btnObj, index == 0);

        // AnnouncerPanel (right panel)
        var panelObj = new GameObject("AnnouncerPanel_" + announcer.title, typeof(RectTransform));
        panelObj.transform.SetParent(_rightPanelRoot, false);
        SetFullStretch(panelObj.GetComponent<RectTransform>());

        var panel = panelObj.AddComponent<AnnouncerPanel>();
        panel.Initialize(announcer);
        panelObj.SetActive(index == 0); // first panel visible by default

        _panels.Add(panel);
        _navButtons.Add(btnObj);

        btnObj.GetComponent<Button>().onClick.AddListener(() => ShowPanel(index));
    }

    private void ShowPanel(int index)
    {
        for (int i = 0; i < _panels.Count; i++)
        {
            _panels[i].gameObject.SetActive(i == index);
            StyleNavButton(_navButtons[i], i == index);
        }
    }

    // ── Left-panel nav scroll view ───────────────────────────

    private Transform BuildNavScrollView(Transform parent)
    {
        var scrollObj = new GameObject("NavScrollView", typeof(RectTransform), typeof(ScrollRect));
        scrollObj.transform.SetParent(parent, false);
        SetFullStretch(scrollObj.GetComponent<RectTransform>());
        var scrollRect = scrollObj.GetComponent<ScrollRect>();

        var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D), typeof(Image));
        viewport.transform.SetParent(scrollObj.transform, false);
        SetFullStretch(viewport.GetComponent<RectTransform>());
        viewport.GetComponent<Image>().color = new Color(0, 0, 0, 0);

        var contentObj = new GameObject("Content", typeof(RectTransform));
        contentObj.transform.SetParent(viewport.transform, false);
        var contentRT = contentObj.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1);
        contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot = new Vector2(0.5f, 1);
        contentRT.offsetMin = Vector2.zero;
        contentRT.offsetMax = Vector2.zero;

        var vlg = contentObj.AddComponent<VerticalLayoutGroup>();
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.spacing = 4;
        vlg.padding = new RectOffset(8, 8, 8, 8);
        contentObj.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.content = contentRT;
        scrollRect.viewport = viewport.GetComponent<RectTransform>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;

        return contentObj.transform;
    }

    // ── UI helpers ───────────────────────────────────────────

    private GameObject CreatePanel(string name, float xMin, float xMax, Color color)
    {
        var obj = new GameObject(name, typeof(RectTransform), typeof(Image));
        obj.transform.SetParent(transform, false);
        var rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(xMin, 0);
        rt.anchorMax = new Vector2(xMax, 1);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        obj.GetComponent<Image>().color = color;
        return obj;
    }

    private static void SetButtonLabel(GameObject btnObj, string text, int size)
    {
        var tmp = btnObj.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null) { tmp.text = text; tmp.fontSize = size; return; }
        if (btnObj.GetComponentInChildren<Text>() is { } legacy) { legacy.text = text; legacy.fontSize = size; }
    }

    private static void StyleNavButton(GameObject btnObj, bool active)
    {
        var img = btnObj.GetComponent<Image>();
        if (img != null)
            img.color = active ? new Color(0.25f, 0.45f, 0.65f) : new Color(0.22f, 0.22f, 0.22f);
    }

    private static void SetFullStretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.localScale = Vector3.one;
    }
}
