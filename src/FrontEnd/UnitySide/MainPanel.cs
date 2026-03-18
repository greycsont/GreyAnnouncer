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
    private GameObject _advancedPanelObj;
    private GameObject _creditsPanelObj;

    private Slider _volumeSlider;
    private Dropdown _playDd;
    private Dropdown _loadDd;

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

        _advancedPanelObj = CreateSpecialPanel<AdvancedPanel>("AdvancedPanel");
        _creditsPanelObj  = CreateSpecialPanel<CreditsPanel>("CreditsPanel");

        foreach (var announcer in AnnouncerManager.GetAllAnnouncers())
            AddAnnouncerPanel(announcer);

        AnnouncerManager.OnRegistered += AddAnnouncerPanel;
        Setting.syncUI += SyncUI;
    }

    private void OnDestroy()
    {
        AnnouncerManager.OnRegistered -= AddAnnouncerPanel;
        Setting.syncUI -= SyncUI;
    }

    private void SyncUI()
    {
        _volumeSlider.SetValueWithoutNotify(Setting.audioSourceVolume);
        _playDd.SetValueWithoutNotify(Setting.audioPlayOptions);
        _loadDd.SetValueWithoutNotify(Setting.audioLoadingStrategy);
    }

    // ── Global settings block ────────────────────────────────

    private void BuildGlobalSettings(Transform parent)
    {
        UIBuilder.AddLabel(parent, "Settings", 15, new Color(0.5f, 0.8f, 1f), preferredHeight: 23);

        UIBuilder.AddLabel(parent, "Master Volume", 12, UIBuilder.SubLabelColor, preferredHeight: 20);
        _volumeSlider = UIBuilder.AddSlider(parent, Setting.audioSourceVolume, 0f, 1f);
        _volumeSlider.onValueChanged.AddListener(v => Setting.audioSourceVolume = v);

        UIBuilder.AddLabel(parent, "Play Strategy", 12, UIBuilder.SubLabelColor, preferredHeight: 20);
        _playDd = UIBuilder.AddDropdown(parent,
            [..Enum.GetNames(typeof(AudioPlayOptions)).Select(s => s.Replace('_', ' '))], height: 30);
        _playDd.value = Setting.audioPlayOptions;
        _playDd.onValueChanged.AddListener(v => Setting.audioPlayOptions = v);

        UIBuilder.AddLabel(parent, "Load Strategy", 12, UIBuilder.SubLabelColor, preferredHeight: 20);
        _loadDd = UIBuilder.AddDropdown(parent,
            [..Enum.GetNames(typeof(AudioLoadOptions)).Select(s => s.Replace('_', ' '))], height: 30);
        _loadDd.value = Setting.audioLoadingStrategy;
        _loadDd.onValueChanged.AddListener(v => Setting.audioLoadingStrategy = v);

        var actionRow = UIBuilder.AddRow(parent, "ActionRow", height: 36).transform;

        var reloadBtn = UIBuilder.AddButton(actionRow, "Reload", fontSize: 12);
        reloadBtn.GetComponent<LayoutElement>().flexibleWidth = 1;
        reloadBtn.onClick.AddListener(AnnouncerManager.ReloadAllAnnouncers);

        var advBtn = UIBuilder.AddButton(actionRow, "Advanced", fontSize: 12);
        advBtn.GetComponent<LayoutElement>().flexibleWidth = 1;
        advBtn.onClick.AddListener(ShowAdvanced);

        var creditBtn = UIBuilder.AddButton(actionRow, "Credits", fontSize: 12);
        creditBtn.GetComponent<LayoutElement>().flexibleWidth = 1;
        creditBtn.onClick.AddListener(ShowCredits);

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
        _advancedPanelObj.SetActive(false);
        _creditsPanelObj.SetActive(false);
        for (int i = 0; i < _panels.Count; i++)
        {
            _panels[i].gameObject.SetActive(i == index);
            UIBuilder.StyleButton(_navButtons[i], i == index);
        }
    }

    private void ShowAdvanced()
    {
        foreach (var p in _panels) p.gameObject.SetActive(false);
        foreach (var b in _navButtons) UIBuilder.StyleButton(b, false);
        _advancedPanelObj.SetActive(true);
        _creditsPanelObj.SetActive(false);
    }

    private void ShowCredits()
    {
        foreach (var p in _panels) p.gameObject.SetActive(false);
        foreach (var b in _navButtons) UIBuilder.StyleButton(b, false);
        _advancedPanelObj.SetActive(false);
        _creditsPanelObj.SetActive(true);
    }

    private GameObject CreateSpecialPanel<T>(string name) where T : MonoBehaviour
    {
        var obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(_rightPanelRoot, false);
        UIBuilder.SetFullStretch(obj.GetComponent<RectTransform>());
        obj.AddComponent<T>();
        obj.SetActive(false);
        return obj;
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
