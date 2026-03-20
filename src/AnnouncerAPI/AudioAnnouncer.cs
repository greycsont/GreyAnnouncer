using System;
using System.IO;
using System.Linq;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;

using GreyAnnouncer.AudioSourceComponent;
using GreyAnnouncer.Util.Ini;
using GreyAnnouncer.Util;
using GameConsole.pcon;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace GreyAnnouncer.AnnouncerAPI;

public class AudioAnnouncer : IAnnouncer
{
    public string title { get; private set; }

    private IAudioLoader _audioLoader;

    private ICooldownManager _cooldownManager;

    private List<string> category;

    private string _defaultAnnouncerConfigPath;

    private string _announcerPath;

    public Action syncUI { get; set; }

    /// <summary>When announcerPath changes, it will automatically reload relative configs.</summary>
    public string announcerPath
    {
        get => _announcerPath;
        set
        {
            LogHelper.LogDebug($"announcerPath seted");
            _announcerPath = value;
            AnnouncerIndex.Set(title, value);
            ReloadAudio();
        }
    }

    private AnnouncerConfig _announcerConfig;
    public AnnouncerConfig announcerConfig
    {
        get => _announcerConfig;
        set
        {
            LogHelper.LogDebug("setter trigged");
            if (_announcerConfig == value)
                return;

            if (_announcerConfig != null)
                _announcerConfig.PropertyChanged -= OnAnnouncerConfigChanged;

            _announcerConfig = value;

            if (_announcerConfig != null)
                _announcerConfig.PropertyChanged += OnAnnouncerConfigChanged;

            ApplyConfigToOther();
        }
    }

    private void OnAnnouncerConfigChanged(object sender, PropertyChangedEventArgs e)
    {
        LogHelper.LogDebug($"AnnouncerConfig changed: {e.PropertyName}");
        ApplyConfigToOther();
    }

    private void ApplyConfigToOther()
    {
        if (_announcerConfig != null && isConfigLoaded)
        {
            _ = _audioLoader.FindAvailableAudioAsync();
            WriteConfigToIni(announcerConfig);
        }

        if (_initialized)
            syncUI.Invoke();
    }

    /// <summary>A reference to config.ini's path</summary>
    private string iniPath => Path.Combine(announcerPath, "config.ini");

    private bool _initialized = false;

    public bool isConfigLoaded { get; private set; } = false;

    /// <summary>When isConfigLoaded is false due to category mismatch, describes which keys are missing or extra.</summary>
    public string configMismatchInfo { get; private set; }

    public AudioAnnouncer(IAudioLoader audioLoader,
                           ICooldownManager cooldownManager,
                           List<string> category,
                           string title,
                           string defaultAnnouoncerConfigPath
                           )
    {
        this._audioLoader = audioLoader;
        this._audioLoader.SetProvider(this);
        this._cooldownManager = cooldownManager;
        this._defaultAnnouncerConfigPath = defaultAnnouoncerConfigPath;
        this.category = category;
        this.title = title;

        announcerPath = AnnouncerIndex.Get(this.title, _defaultAnnouncerConfigPath);

        SubscribeAnnouncerManager();

        _initialized = true;
    }

    /// <summary>Will Play a random audio in the belong category</summary>
    public async Task PlayAudioViaCategory(string category)
    {
        LogHelper.LogInfo($"Request to play audio for category: {category}");
        try
        {
            if (!ValidateCondition(category))
                return;

            await PlayAudioClip(category);
        }
        catch (Exception ex)
        {
            LogPlaybackError(ex);
        }
    }


    /// <summary>Will Play a random audio in the belong category by jsonSetting mapping via index</summary>
    public async Task PlayAudioViaIndex(int index)
        => await PlayAudioViaCategory(announcerConfig.CategorySetting.Keys.ToArray()[index]);


    /// <summary>Reload Audio, only works when using Preload and Play options</summary>
    public void ReloadAudio()
    {
        announcerConfig = InitializeConfig(category);
    }


    /// <summary>Resets the announcer's category's cooldown</summary>
    public void ResetCooldown()
        => _cooldownManager.ResetCooldowns();


    /// <summary>Clear audioclip in audioloader, only works when using Preload and Play options</summary>
    public void ClearAudioClipsCache()
        => _audioLoader.ClearCache();

    /// <summary>Sets cooldown to the targeted category</summary>
    private void SetCooldown(string category, float cooldown)
        => _cooldownManager.StartCooldowns(category, cooldown);


    /// <summary>Gets bool with validate serveral conditions </summary>
    private bool ValidateCondition(string category)
    {
        var validationState = GetPlayValidationState(category);
        if (validationState != ValidationState.Success)
        {
            LogHelper.LogInfo($"PlayValidationState: {category}, {validationState}");
            return false;
        }
        return true;
    }

    /// <summary>Play a Sound by category, audioPlayOptions for </summary>
    private async Task PlayAudioClip(string category)
    {
        LogHelper.LogInfo($"Attempting to play audio for category: {category}");
        Sound sound = await _audioLoader.LoadAudioClip(category);

        if (sound == null)
        {
            LogHelper.LogError($"Failed to load audio clip may for this category: {category}");
            return;
        }
        if (_cooldownManager.IsIndividualCooldownActive(sound.category))
        {
            LogHelper.LogInfo($"The sound's category {sound.category} is still in cooldown");
            return;
        }

        LogHelper.LogDebug($"category : {sound.category}, Cooldown : {announcerConfig.CategorySetting[sound.category].Cooldown}");

        SoundDispatcher.SendClipToAudioSource(sound);

        SetCooldown(sound.category, announcerConfig.CategorySetting[sound.category].Cooldown);
    }

    private void LogPlaybackError(Exception ex)
        => LogHelper.LogError($"An error occurred while playing sound: {ex.Message}\n{ex.StackTrace}");


    private ValidationState GetPlayValidationState(string category)
    {
        if (_cooldownManager == null || _audioLoader == null)
            return ValidationState.ComponentsNotInitialized;

        if (_announcerConfig == null)
            return ValidationState.ConfigNotLoaded;

        if (!announcerConfig.CategorySetting.ContainsKey(category))
            return ValidationState.InvalidKey;

        if (!announcerConfig.CategorySetting[category].Enabled && announcerConfig.RandomizeAudioOnPlay == false)
            return ValidationState.DisabledByConfig;
        
        if (_cooldownManager.IsIndividualCooldownActive(category) == true)
            return ValidationState.IndividualCooldown;

        return ValidationState.Success;
    }

    private void SubscribeAnnouncerManager()
    {
        AnnouncerManager.reloadAnnouncer += ReloadAudio;
        AnnouncerManager.resetCooldown += ResetCooldown;
        AnnouncerManager.clearAudioClipCache += ClearAudioClipsCache;

        AnnouncerManager.AddAnnouncer(this);
    }

    
    private AnnouncerConfig InitializeConfig(List<string> category)
    {
        LogHelper.LogDebug($"current iniPath of {title}: {iniPath}");

        var defaultConfig = new AnnouncerConfig().SetCategorySettingMap(
            category.ToDictionary(cat => cat, cat => new CategorySetting())
        );

        if (!File.Exists(iniPath))
        {
            LogHelper.LogDebug($"Initialize new config.ini in: {iniPath}");
            WriteConfigToIni(defaultConfig);
            isConfigLoaded = false;
            return defaultConfig;
        }

        var iniConfig = ReadConfigFromIni();
        if (iniConfig == null || !IsCategoryMatch(iniConfig, category))
        {
            LogHelper.LogError($"[{title}] AnnouncerConfig category mismatch — {configMismatchInfo}");
            isConfigLoaded = false;
            return null;
        }

        isConfigLoaded = true;
        return iniConfig;
    }

    private bool IsCategoryMatch(AnnouncerConfig config, List<string> expected)
    {
        var keys = config.CategorySetting.Keys;
        var missing = expected.Except(keys).ToList();
        var extra = keys.Except(expected).ToList();

        if (missing.Count == 0)
            return true;

        configMismatchInfo = $"Missing: [{string.Join(", ", missing)}], Extra: [{string.Join(", ", extra)}]";
        return false;
    }

    private void WriteConfigToIni(AnnouncerConfig announcerConfig)
    {
        LogHelper.LogDebug($"[WriteConfigToIni] called by {new StackTrace().GetFrame(1).GetMethod().Name}");
        var doc = new IniDocument();

        doc = AnnouncerIniMapper.ToIni(doc, announcerConfig);

        IniWriter.Write(iniPath, doc);
    }

    private AnnouncerConfig ReadConfigFromIni()
    {
        if (!File.Exists(iniPath))
            return null;

        var doc = IniReader.Read(iniPath);

        var announcerConfig = AnnouncerIniMapper.FromIni(doc);
        CheckConfigUpdate(ref announcerConfig);
        return announcerConfig;
    }

    private void CheckConfigUpdate(ref AnnouncerConfig announcerConfig)
    {
        
    }

    public void EditExternally()
        => ProcessHelper.OpenDirectory(announcerPath);

}
