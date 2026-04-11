using System;
using System.IO;
using System.Linq;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;

using GreyAnnouncer.AudioSourceComponent;
using GreyAnnouncer.Util;
using GreyAnnouncer.Config;

namespace GreyAnnouncer.AnnouncerCore;

public partial class AudioAnnouncer : IAnnouncer
{
    public string title { get; private set; }

    private IAudioLoader _audioLoader;

    private ICooldownManager _cooldownManager;

    private IConfigManager _configManager;

    private List<string> category;

    private string _defaultAnnouncerConfigPath;

    public Action syncUI { get; set; }

    /// <summary>When announcerPath changes, it will automatically reload relative configs.</summary>
    public string announcerPath
    {
        get => field;
        set
        {
            LogHelper.LogDebug($"announcerPath seted");
            field = value;
            SaveSelectedPack(value);
            ReloadAudio();
        }
    }

    public AnnouncerConfig announcerConfig
    {
        get => field;
        set
        {
            LogHelper.LogDebug("setter trigged");
            if (field == value)
                return;

            if (field != null)
                field.PropertyChanged -= OnAnnouncerConfigChanged;

            field = value;

            if (field != null)
                field.PropertyChanged += OnAnnouncerConfigChanged;

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
        if (announcerConfig != null && isConfigLoaded) {
            _ = _audioLoader.FindAvailableAudioAsync();
            _configManager.Save(announcerPath, announcerConfig);
        }

        if (_initialized)
            syncUI.Invoke();
    }

    private bool _initialized = false;

    public bool isConfigLoaded { get; private set; } = false;

    /// <summary>When isConfigLoaded is false due to category mismatch, describes which keys are missing or extra.</summary>
    public string configMismatchInfo { get; private set; }

    public AudioAnnouncer(IAudioLoader audioLoader,
                           ICooldownManager cooldownManager,
                           IConfigManager configManager,
                           List<string> category,
                           string title,
                           string defaultAnnouoncerConfigPath
                           )
    {
        this._audioLoader = audioLoader;
        this._audioLoader.SetProvider(this);
        this._cooldownManager = cooldownManager;
        this._configManager = configManager;
        this._defaultAnnouncerConfigPath = defaultAnnouoncerConfigPath;
        this.category = category;
        this.title = title;

        announcerPath = LoadSelectedPack();

        SubscribeAnnouncerManager();

        _initialized = true;
    }

    /// <summary>Will Play a random audio in the belong category</summary>
    public async Task PlayAudioViaCategory(string category)
    {
        LogHelper.LogInfo($"Request to play audio for category: {category}");
        try {
            if (!ValidateCondition(category)) return;
            await PlayAudioClip(category);
        }
        catch (Exception ex) {
            LogPlaybackError(ex);
        }
    }

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
        if (validationState != ValidationState.Success) {
            LogHelper.LogDebug($"PlayValidationState: {category}, {validationState}");
            return false;
        }
        return true;
    }

    /// <summary>Play a Sound by category, audioPlayOptions for </summary>
    private async Task PlayAudioClip(string category)
    {
        LogHelper.LogDebug($"Attempting to play audio for category: {category}");
        Sound sound = await _audioLoader.GetAudioClip(category);

        if (sound == null) {
            LogHelper.LogError($"Failed to load audio clip may for this category: {category}");
            return;
        }
        if (_cooldownManager.IsIndividualCooldownActive(sound.category)) {
            LogHelper.LogDebug($"The sound's category {sound.category} is still in cooldown");
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

        if (announcerConfig == null)
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

    public void EditExternally()
        => ProcessHelper.OpenDirectory(announcerPath);

}
