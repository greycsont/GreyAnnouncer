using System;
using System.IO;
using System.Linq;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;

using GreyAnnouncer.AudioSourceComponent;
using GreyAnnouncer.Config;
using GreyAnnouncer.FrontEnd;
using GreyAnnouncer.Util.Ini;

namespace GreyAnnouncer.AnnouncerAPI;

public class AudioAnnouncer
{
    public string title;

    private IAudioLoader _audioLoader;

    private ICooldownManager _cooldownManager;

    private List<string> category;

    private string _announcerPath;

    /// <summary>
    /// When announcerPath changes, it will automatically reload relative configs.
    /// </summary>
    public string announcerPath
    {
        get => _announcerPath;
        set
        {
            LogManager.LogDebug($"announcerPath seted");
            _announcerPath = value;
            iniPath = Path.Combine(value, "config.ini");
            AnnouncerIndex.Set(title, value);
            ReloadAudio();
        }
    }

    public AnnouncerConfig _announcerConfig;
    public AnnouncerConfig announcerConfig
    {
        get => _announcerConfig;
        set
        {
            if (_announcerConfig == value)
                return;

            if (_announcerConfig != null)
                _announcerConfig.PropertyChanged -= OnAnnouncerConfigChanged;

            _announcerConfig = value;

            if (_announcerConfig != null)
                _announcerConfig.PropertyChanged += OnAnnouncerConfigChanged;

            ApplyAnnouncerConfig();
        }
    }

    private void OnAnnouncerConfigChanged(object sender, PropertyChangedEventArgs e)
    {
        LogManager.LogDebug($"AnnouncerConfig changed: {e.PropertyName}");
        ApplyAnnouncerConfig();
    }

    private void ApplyAnnouncerConfig()
    {
        _audioLoader.UpdateSetting(_announcerConfig, announcerPath);
        WriteConfigToIni(_announcerConfig);
        RegisterAnnouncerPage.ApplyConfigToUI(_announcerConfig);
    }


    private string iniPath;


    public AudioAnnouncer(IAudioLoader audioLoader,
                           ICooldownManager cooldownManager,
                           List<string> displayNameMapping,
                           string title)
    {
        this._audioLoader = audioLoader;
        this._cooldownManager = cooldownManager;
        this.category = displayNameMapping;
        this.title = title;

        announcerPath = AnnouncerIndex.Get(this.title);

        SubscribeAnnouncerManager();

        RegisterAnnouncerPage.Build(title, this);
    }

    [Description("Parry balls of Maurice -> Hit Maurice -> AscendingRank() -> Postfix() -> PlaySound() -> CheckPlayValidation(), " +
                 "This error will skip all the function before CheckPlayValidation(), That's why try-catch has implemented in the fucntion")]

    /// <summary>Will Play a random audio in the belong category</summary>
    public async Task PlayAudioViaCategory(string category)
    {
        LogManager.LogInfo($"Request to play audio for category: {category}");
        try
        {
            if (!ValidateAndLogPlayback(category))
                return;

            if (await PlayAudioClip(category, BepInExConfig.audioPlayOptions.Value))
                SetCooldown(category, announcerConfig.CategoryAudioMap[category].Cooldown);
        }
        catch (Exception ex)
        {
            LogPlaybackError(ex);
        }
    }

    /// <summary>Will Play a random audio in the belong category by jsonSetting mapping via index</summary>
    public async Task PlayAudioViaIndex(int index)
        => await PlayAudioViaCategory(announcerConfig.CategoryAudioMap.Keys.ToArray()[index]);

    /// <summary>Reload Audio, only works when using Preload and Play options</summary>
    public void ReloadAudio()
        => announcerConfig = AnnouncerConfigIniInitialization(category);


    /// <summary>Resets the announcer's cooldown</summary>
    public void ResetCooldown()
        => _cooldownManager.ResetCooldowns();

    public void UpdateAnnouncerConfig(AnnouncerConfig newSetting)
        => announcerConfig = newSetting;


    /// <summary>Clear audioclip in audioloader, only works when using Preload and Play options</summary>
    public void ClearAudioClipsCache()
        => _audioLoader.ClearCache();

    private void SetCooldown(string category, float cooldown)
        => _cooldownManager.StartCooldowns(category, cooldown);


    #region Play Audio related
    private bool ValidateAndLogPlayback(string category)
    {
        var validationState = GetPlayValidationState(category);
        if (validationState != ValidationState.Success)
        {
            //LogManager.LogInfo($"PlayValidationState: {category}, {validationState}");
            return false;
        }
        return true;
    }

    private async Task<bool> PlayAudioClip(string category, int audioPlayOptions = 0)
    {
        LogManager.LogInfo($"Attempting to play audio for category: {category}");
        Sound sound = await _audioLoader.LoadAudioClip(category);

        if (sound == null)
        {
            LogManager.LogError($"Failed to load audio clip for category: {category}");
            return false;
        }

        LogManager.LogDebug($"category : {sound.category}, Cooldown : {_announcerConfig.CategoryAudioMap[category].Cooldown}");

        SoundDispatcher.SendClipToAudioSource(sound, audioPlayOptions);

        return true;

    }

    private void LogPlaybackError(Exception ex)
        => LogManager.LogError($"An error occurred while playing sound: {ex.Message}\n{ex.StackTrace}");
    #endregion


    private ValidationState GetPlayValidationState(string category)
    {
        if (_cooldownManager == null || _audioLoader == null)
        {
            return ValidationState.ComponentsNotInitialized;
        }


        if (_audioLoader.announcerConfig.CategoryAudioMap.Keys == null
            || !_audioLoader.announcerConfig.CategoryAudioMap.Keys.Contains(category))
        {
            return ValidationState.InvalidKey;
        }


        if (_cooldownManager.IsIndividualCooldownActive(category))
        {
            return ValidationState.IndividualCooldown;
        }


        if (!_announcerConfig.CategoryAudioMap[category].Enabled)
        {
            return ValidationState.DisabledByConfig;
        }

        return ValidationState.Success;
    }

    private void SubscribeAnnouncerManager()
    {
        AnnouncerManager.reloadAnnouncer += ReloadAudio;
        AnnouncerManager.resetCooldown += ResetCooldown;
        AnnouncerManager.clearAudioClipCache += ClearAudioClipsCache;

        AnnouncerManager.AddAnnouncer(this);
    }

    private AnnouncerConfig AnnouncerConfigIniInitialization(List<string> category)
    {
        LogManager.LogDebug($"current iniPath for {title}: {iniPath}");
        if (File.Exists(iniPath) == false)
        {
            LogManager.LogDebug($"Initialize new config.ini in: {iniPath}");
            var audioDict = category.ToDictionary(
                cat => cat,
                cat => new CategorySetting{}
            );
            var announcerConfig = new AnnouncerConfig().SetCategorySettingMap(audioDict);
            WriteConfigToIni(announcerConfig);
        }

        return ReadConfigFromIni();
    }

    private void WriteConfigToIni(AnnouncerConfig announcerConfig)
    {
        var doc = new IniDocument();
        doc = IniMapper.ToIni(doc, announcerConfig, "General"); // 写 General
        foreach (var pair in announcerConfig.CategoryAudioMap)
        {
            doc = IniMapper.ToIni(doc, pair.Value, $"Category:{pair.Key}");
        }
        IniWriter.Write(iniPath, doc);
    }

    private AnnouncerConfig ReadConfigFromIni()
    {
        var announcerConfig = new AnnouncerConfig();
        if (!File.Exists(iniPath))
            return null;
        var doc = IniReader.Read(iniPath); // 假设你有 IniReader.Read 返回 IniDocument

        // 读取 General
        announcerConfig = IniMapper.FromIni<AnnouncerConfig>(doc, "General");

        // 读取所有 Category: 开头的 section
        announcerConfig.CategoryAudioMap.Clear();
        foreach (var key in doc.Sections.Keys.Where(k => k.StartsWith("Category:")))
        {
            var categoryName = key.Substring("Category:".Length);
            var categoryConfig = IniMapper.FromIni<CategorySetting>(doc, key);
            announcerConfig.AddCategory(categoryName, categoryConfig);
        }
        return announcerConfig;
    }
}
