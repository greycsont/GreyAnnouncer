using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GreyAnnouncer.AnnouncerCore;
using GreyAnnouncer.Config;

namespace GreyAnnouncer.FrontEnd;

/// <summary>
/// Unity-side equivalent of RegistedAnnouncerPage.
/// Call Initialize(IAnnouncer) immediately after AddComponent.
/// </summary>
public class AnnouncerPanel : MonoBehaviour
{
    private IAnnouncer _announcer;
    private Transform _content;
    private Dropdown _announcerDropdown;
    private Toggle _randomizeToggle;
    private TextMeshProUGUI _mismatchLabel;
    private readonly Dictionary<string, Category> _categories = new();

    // ── public API ──────────────────────────────────────────

    public void Initialize(IAnnouncer announcer)
    {
        _announcer = announcer;
        _announcer.syncUI += Refresh;
        Build();
    }

    /// <summary>Push live config values back into the UI controls.</summary>
    public void Refresh()
    {
        if (_announcer == null) return;

        var currentPack = Path.GetFileName(_announcer.announcerPath);
        var idx = _announcerDropdown.options.FindIndex(o => o.text == currentPack);
        if (idx >= 0) _announcerDropdown.SetValueWithoutNotify(idx);

        if (_announcer.isConfigLoaded == false)
        {
            _mismatchLabel.text = string.IsNullOrEmpty(_announcer.configMismatchInfo)
                ? "Config not loaded."
                : $"Config mismatch — {_announcer.configMismatchInfo}";
            return;
        }
        _mismatchLabel.text = "";

        var cfg = _announcer.announcerConfig;

        if (_randomizeToggle == null)
            BuildConfigSection(cfg);

        _randomizeToggle?.SetIsOnWithoutNotify(cfg.RandomizeAudioOnPlay);

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
        UIBuilder.SetFullStretch(GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>());

        var (_, content) = UIBuilder.BuildScrollView(transform);
        _content = content;

        // Title
        UIBuilder.AddLabel(content, _announcer.title, 26, Color.white, preferredHeight: 36);
        UIBuilder.AddSpace(content, 3);

        // Announcer selector
        UIBuilder.AddLabel(content, "Selected Announcer", 13, UIBuilder.SubLabelColor, preferredHeight: 22);
        var announcerOptions = AudioAnnouncer.GetAvailablePacks(_announcer.title);
        _announcerDropdown = UIBuilder.AddDropdown(content, announcerOptions);
        var currentPack = Path.GetFileName(_announcer.announcerPath);
        var initIdx = announcerOptions.IndexOf(currentPack);
        if (initIdx >= 0) _announcerDropdown.SetValueWithoutNotify(initIdx);
        _announcerDropdown.onValueChanged.AddListener(idx =>
        {
            var selected = _announcerDropdown.options[idx].text;
            _announcer.announcerPath = Path.Combine(Setting.announcersPath, _announcer.title, selected);
        });
        UIBuilder.AddSpace(content, 3);

        // Open folder
        UIBuilder.AddButton(content, "Open Current Announcer Folder")
            .onClick.AddListener(() => _announcer.EditExternally());
        UIBuilder.AddSpace(content, 3);

        // Mismatch label (updated by Refresh)
        _mismatchLabel = UIBuilder.AddLabel(content, "", 14, new Color(1f, 0.4f, 0.4f), preferredHeight: 22);

        if (_announcer.isConfigLoaded)
            BuildConfigSection(_announcer.announcerConfig);
    }

    private void BuildConfigSection(PackConfig cfg)
    {
        // Randomize toggle row
        var randomizeRow = UIBuilder.AddRow(_content, "RandomizeRow").transform;
        UIBuilder.AddLabel(randomizeRow, "Randomize Audio On Play", 14, Color.white,
            preferredWidth: 200, flexibleWidth: 1);
        _randomizeToggle = UIBuilder.AddToggle(randomizeRow, preferredWidth: 30);
        _randomizeToggle.isOn = cfg.RandomizeAudioOnPlay;
        _randomizeToggle.onValueChanged.AddListener(v =>
            _announcer.announcerConfig.RandomizeAudioOnPlay = v);
        UIBuilder.AddSpace(_content, 3);

        // Per-category configuration
        UIBuilder.AddLabel(_content, "Configuration", 20, new Color(0.85f, 0.85f, 0.85f),
            preferredHeight: 30);

        foreach (var kv in cfg.CategorySetting)
        {
            string key = kv.Key;

            var catObj = new GameObject("Category_" + key, typeof(RectTransform));
            catObj.transform.SetParent(_content, false);
            var cat = catObj.AddComponent<Category>();
            cat.SetName(key);

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
            UIBuilder.AddSpace(_content, 3);
        }
    }
}
