using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
        _announcer.syncUI += Refresh;
        Build();
    }

    /// <summary>Push live config values back into the UI controls.</summary>
    public void Refresh()
    {
        if (_announcer == null) return;
        var cfg = _announcer.announcerConfig;

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

        // Title
        UIBuilder.AddLabel(content, _announcer.title, 26, Color.white, preferredHeight: 36);
        UIBuilder.AddSpace(content, 3);

        // Announcer selector
        UIBuilder.AddLabel(content, "Selected Announcer", 13, UIBuilder.SubLabelColor, preferredHeight: 22);
        var announcerOptions = AnnouncerIndex.GetTargetAnnouncer(_announcer.title);
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

        // Randomize toggle row
        var randomizeRow = UIBuilder.AddRow(content, "RandomizeRow").transform;
        UIBuilder.AddLabel(randomizeRow, "Randomize Audio On Play", 14, UnityEngine.Color.white,
            preferredWidth: 200, flexibleWidth: 1);
        _randomizeToggle = UIBuilder.AddToggle(randomizeRow, preferredWidth: 30);
        _randomizeToggle.isOn = _announcer.announcerConfig.RandomizeAudioOnPlay;
        _randomizeToggle.onValueChanged.AddListener(v =>
            _announcer.announcerConfig.RandomizeAudioOnPlay = v);
        UIBuilder.AddSpace(content, 3);

        // Per-category configuration
        UIBuilder.AddLabel(content, "Configuration", 20, new UnityEngine.Color(0.85f, 0.85f, 0.85f),
            preferredHeight: 30);

        foreach (var kv in _announcer.announcerConfig.CategorySetting)
        {
            string key = kv.Key;

            var catObj = new GameObject("Category_" + key, typeof(RectTransform));
            catObj.transform.SetParent(content, false);
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
            UIBuilder.AddSpace(content, 3);
        }
    }
}
