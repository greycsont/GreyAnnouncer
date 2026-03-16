using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GreyAnnouncer.AnnouncerAPI;
using GreyAnnouncer.Config;

namespace GreyAnnouncer.FrontEnd;

/// <summary>
/// Unity-side equivalent of RegistedAnnouncerPage.
/// Call Initialize(IAnnouncer) immediately after AddComponent.
/// </summary>
public class AnnouncerPanel : MonoBehaviour
{
    private IAnnouncer _announcer;

    private Dropdown _announcerDropdown;
    private Toggle _randomizeToggle;
    private readonly Dictionary<string, Category> _categories = new();

    // ── public API ──────────────────────────────────────────

    public void Initialize(IAnnouncer announcer)
    {
        _announcer = announcer;
        _announcer.announcerConfig.PropertyChanged += (_, __) => Refresh();
        Build();
    }

    /// <summary>Push live config values back into the UI controls.</summary>
    public void Refresh()
    {
        if (_announcer == null) return;
        var cfg = _announcer.announcerConfig;

        if (_randomizeToggle != null)
            _randomizeToggle.SetIsOnWithoutNotify(cfg.RandomizeAudioOnPlay);

        foreach (var kv in _categories)
        {
            if (!cfg.CategorySetting.TryGetValue(kv.Key, out var data)) continue;
            kv.Value.enabledToggle.SetIsOnWithoutNotify(data.Enabled);
            kv.Value.SetVolume(data.VolumeMultiplier);
            kv.Value.SetCooldown(data.Cooldown);
        }
    }

    // ── build ────────────────────────────────────────────────

    private void Build()
    {
        // Fill parent
        SetFullStretch(GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>());

        // ── Scroll view ──────────────────────────────────────
        var (scrollRect, content) = BuildScrollView(transform);

        // ── Title ────────────────────────────────────────────
        AddHeader(content, _announcer.title, 26, Color.white);
        AddSpace(content, 3);

        // ── Announcer selector ───────────────────────────────
        AddSectionLabel(content, "Selected Announcer");
        _announcerDropdown = AddDropdown(content, "AnnouncerDropdown", AnnouncerIndex.GetAnnouncers());
        _announcerDropdown.onValueChanged.AddListener(idx =>
        {
            var selected = _announcerDropdown.options[idx].text;
            _announcer.announcerPath = Path.Combine(Setting.announcersPath, selected);
        });
        AddSpace(content, 3);

        // ── Open folder button ───────────────────────────────
        var openBtn = AddButton(content, "Open Current Announcer Folder");
        openBtn.onClick.AddListener(() => _announcer.EditExternally());
        AddSpace(content, 3);

        // ── Randomize toggle ─────────────────────────────────
        var randomizeRow = AddRow(content, "RandomizeRow");
        AddRowLabel(randomizeRow, "Randomize Audio On Play");
        _randomizeToggle = AddToggleToRow(randomizeRow, "RandomizeToggle");
        _randomizeToggle.isOn = _announcer.announcerConfig.RandomizeAudioOnPlay;
        _randomizeToggle.onValueChanged.AddListener(v =>
            _announcer.announcerConfig.RandomizeAudioOnPlay = v);
        AddSpace(content, 3);

        // ── Per-category configuration ───────────────────────
        AddHeader(content, "Configuration", 20, new Color(0.85f, 0.85f, 0.85f));

        foreach (var kv in _announcer.announcerConfig.CategorySetting)
        {
            string key = kv.Key;

            var catObj = new GameObject("Category_" + key, typeof(RectTransform));
            catObj.transform.SetParent(content, false);
            var cat = catObj.AddComponent<Category>(); // Awake fires here
            cat.SetName(key);

            // Wire up
            cat.enabledToggle.isOn = kv.Value.Enabled;
            cat.enabledToggle.onValueChanged.AddListener(v =>
            {
                if (_announcer.announcerConfig.CategorySetting.TryGetValue(key, out var d))
                    d.Enabled = v;
            });

            cat.SetVolume(kv.Value.VolumeMultiplier);
            cat.volumeSlider.onValueChanged.AddListener(v =>
            {
                if (_announcer.announcerConfig.CategorySetting.TryGetValue(key, out var d))
                    d.VolumeMultiplier = v;
            });

            cat.SetCooldown(kv.Value.Cooldown);
            cat.cooldownSlider.onValueChanged.AddListener(v =>
            {
                if (_announcer.announcerConfig.CategorySetting.TryGetValue(key, out var d))
                    d.Cooldown = v;
            });
            _categories[key] = cat;

            AddSpace(content, 3);
        }
    }

    // ── scroll view ──────────────────────────────────────────

    private (ScrollRect, Transform content) BuildScrollView(Transform parent)
    {
        var scrollObj = new GameObject("ScrollView", typeof(RectTransform), typeof(ScrollRect));
        scrollObj.transform.SetParent(parent, false);
        SetFullStretch(scrollObj.GetComponent<RectTransform>());
        var scrollRect = scrollObj.GetComponent<ScrollRect>();

        var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D), typeof(Image));
        viewport.transform.SetParent(scrollObj.transform, false);
        var viewRT = viewport.GetComponent<RectTransform>();
        SetFullStretch(viewRT);
        viewRT.offsetMax = new Vector2(-20, 0);
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
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.spacing = 8;
        vlg.padding = new RectOffset(12, 12, 12, 12);
        contentObj.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Scrollbar
        var sbObj = new GameObject("Scrollbar", typeof(RectTransform), typeof(Scrollbar), typeof(Image));
        sbObj.transform.SetParent(scrollObj.transform, false);
        var sbRT = sbObj.GetComponent<RectTransform>();
        sbRT.anchorMin = new Vector2(1, 0);
        sbRT.anchorMax = new Vector2(1, 1);
        sbRT.offsetMin = new Vector2(-20, 0);
        sbRT.offsetMax = Vector2.zero;
        sbObj.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.6f);

        var sb = sbObj.GetComponent<Scrollbar>();
        sb.direction = Scrollbar.Direction.BottomToTop;

        var handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        handle.transform.SetParent(sbObj.transform, false);
        SetFullStretch(handle.GetComponent<RectTransform>());
        handle.GetComponent<Image>().color = new Color(0.6f, 0.6f, 0.6f);

        scrollRect.content = contentRT;
        scrollRect.viewport = viewRT;
        scrollRect.verticalScrollbar = sb;
        sb.handleRect = handle.GetComponent<RectTransform>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;

        return (scrollRect, contentObj.transform);
    }

    // ── widget helpers ───────────────────────────────────────

    private void AddHeader(Transform parent, string text, int size, Color color)
    {
        var obj = new GameObject("Header_" + text, typeof(RectTransform), typeof(TextMeshProUGUI));
        obj.transform.SetParent(parent, false);
        obj.AddComponent<LayoutElement>().preferredHeight = size + 10;
        var tmp = obj.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Left;
    }

    private void AddSectionLabel(Transform parent, string text)
    {
        var obj = new GameObject("SectionLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
        obj.transform.SetParent(parent, false);
        obj.AddComponent<LayoutElement>().preferredHeight = 22;
        var tmp = obj.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 13;
        tmp.color = new Color(0.7f, 0.7f, 0.7f);
    }

    private Dropdown AddDropdown(Transform parent, string name, List<string> options)
    {
        var obj = DefaultControls.CreateDropdown(new DefaultControls.Resources());
        obj.name = name;
        obj.transform.SetParent(parent, false);
        obj.AddComponent<LayoutElement>().preferredHeight = 36;
        var dd = obj.GetComponent<Dropdown>();
        dd.ClearOptions();
        dd.AddOptions(options);
        return dd;
    }

    private Button AddButton(Transform parent, string label)
    {
        var obj = DefaultControls.CreateButton(new DefaultControls.Resources());
        obj.name = "Btn_" + label;
        obj.transform.SetParent(parent, false);
        obj.AddComponent<LayoutElement>().preferredHeight = 36;
        var tmp = obj.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null) tmp.text = label;
        else
        {
            var legacy = obj.GetComponentInChildren<Text>();
            if (legacy != null) legacy.text = label;
        }
        return obj.GetComponent<Button>();
    }

    private GameObject AddRow(Transform parent, string name)
    {
        var row = new GameObject(name, typeof(RectTransform));
        row.transform.SetParent(parent, false);
        var hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.spacing = 8;
        row.AddComponent<LayoutElement>().preferredHeight = 30;
        return row;
    }

    private void AddRowLabel(GameObject row, string text)
    {
        var obj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        obj.transform.SetParent(row.transform, false);
        var le = obj.AddComponent<LayoutElement>();
        le.preferredWidth = 200;
        le.flexibleWidth = 1;
        var tmp = obj.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 14;
        tmp.alignment = TextAlignmentOptions.Left;
    }

    private Toggle AddToggleToRow(GameObject row, string name)
    {
        var obj = DefaultControls.CreateToggle(new DefaultControls.Resources());
        obj.name = name;
        obj.transform.SetParent(row.transform, false);
        var le = obj.AddComponent<LayoutElement>();
        le.preferredWidth = 30;
        le.flexibleWidth = 0;
        return obj.GetComponent<Toggle>();
    }

    private void AddSpace(Transform parent, float height)
    {
        var obj = new GameObject("Space", typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        obj.AddComponent<LayoutElement>().preferredHeight = height;
    }

    private void SetFullStretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.localScale = Vector3.one;
    }
}
