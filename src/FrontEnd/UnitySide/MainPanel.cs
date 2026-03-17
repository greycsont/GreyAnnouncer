using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using GreyAnnouncer.AnnouncerAPI;
using GreyAnnouncer.Config;

namespace GreyAnnouncer.FrontEnd;

public class MainPanel : MonoBehaviour
{
    private Transform _navContent;
    private Transform _rightPanelRoot;
    private readonly List<AnnouncerPanel> _panels = [];
    private readonly List<GameObject> _navButtons = [];

    public void Awake()
    {
        UIBuilder.SetFullStretch(GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>());

        var leftPanel = CreatePanel("LeftPanel", 0f, 0.4f, new Color(0.18f, 0.18f, 0.18f, 0.95f));
        var (_, navContent) = UIBuilder.BuildScrollView(leftPanel.transform,
            padding: new RectOffset(8, 8, 8, 8), addScrollbar: false);
        _navContent = navContent;
        BuildGlobalSettings(_navContent);

        var rightPanel = CreatePanel("RightPanel", 0.4f, 1f, new Color(0.1f, 0.1f, 0.1f, 1f));
        _rightPanelRoot = rightPanel.transform;

        foreach (var announcer in AnnouncerManager.GetAllAnnouncers())
            AddAnnouncerPanel(announcer);

        AnnouncerManager.OnRegistered += AddAnnouncerPanel;
    }

    private void OnDestroy()
    {
        AnnouncerManager.OnRegistered -= AddAnnouncerPanel;
    }

    // ── Global settings block ────────────────────────────────

    private void BuildGlobalSettings(Transform parent)
    {
        UIBuilder.AddLabel(parent, "Settings", 15, new Color(0.5f, 0.8f, 1f), preferredHeight: 23);

        UIBuilder.AddLabel(parent, "Master Volume", 12, UIBuilder.SubLabelColor, preferredHeight: 20);
        UIBuilder.AddSlider(parent, Setting.audioSourceVolume, 0f, 1f)
            .onValueChanged.AddListener(v => Setting.audioSourceVolume = v);

        UIBuilder.AddLabel(parent, "Play Strategy", 12, UIBuilder.SubLabelColor, preferredHeight: 20);
        var playDd = UIBuilder.AddDropdown(parent,
            [..Enum.GetNames(typeof(AudioPlayOptions)).Select(s => s.Replace('_', ' '))], height: 30);
        playDd.value = Setting.audioPlayOptions;
        playDd.onValueChanged.AddListener(v => Setting.audioPlayOptions = v);

        UIBuilder.AddLabel(parent, "Load Strategy", 12, UIBuilder.SubLabelColor, preferredHeight: 20);
        var loadDd = UIBuilder.AddDropdown(parent,
            [..Enum.GetNames(typeof(AudioLoadOptions)).Select(s => s.Replace('_', ' '))], height: 30);
        loadDd.value = Setting.audioLoadingStrategy;
        loadDd.onValueChanged.AddListener(v => Setting.audioLoadingStrategy = v);

        UIBuilder.AddButton(parent, "Reload", fontSize: 13)
            .onClick.AddListener(AnnouncerManager.ReloadAllAnnouncers);

        UIBuilder.AddSeparator(parent);
        UIBuilder.AddLabel(parent, "Announcers", 13, new Color(1f, 0.7f, 0.3f), preferredHeight: 21);
    }

    // ── Panel management ─────────────────────────────────────

    private void AddAnnouncerPanel(IAnnouncer announcer)
    {
        int index = _panels.Count;

        var btn = UIBuilder.AddButton(_navContent, announcer.title, fontSize: 14, height: 44);
        UIBuilder.StyleButton(btn.gameObject, index == 0);
        btn.onClick.AddListener(() => ShowPanel(index));

        var panelObj = new GameObject("AnnouncerPanel_" + announcer.title, typeof(RectTransform));
        panelObj.transform.SetParent(_rightPanelRoot, false);
        UIBuilder.SetFullStretch(panelObj.GetComponent<RectTransform>());

        var panel = panelObj.AddComponent<AnnouncerPanel>();
        panel.Initialize(announcer);
        panelObj.SetActive(index == 0);

        _panels.Add(panel);
        _navButtons.Add(btn.gameObject);
    }

    private void ShowPanel(int index)
    {
        for (int i = 0; i < _panels.Count; i++)
        {
            _panels[i].gameObject.SetActive(i == index);
            UIBuilder.StyleButton(_navButtons[i], i == index);
        }
    }

    // ── Helpers ───────────────────────────────────────────────

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
}
