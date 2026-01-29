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
using GreyAnnouncer.Util;

namespace GreyAnnouncer.AnnouncerAPI;

public class AudioAnnouncer
{
    public string title;

    private IAudioLoader _audioLoader;

    private ICooldownManager _cooldownManager;

    private List<string> category;

    private string _defaultAnnouncerConfigPath;

    private string _announcerPath;

    /// <summary>
    /// When announcerPath changes, it will automatically reload relative configs.
    /// </summary>
    public string announcerPath
    {
        get => _announcerPath;
        set
        {
            LogHelper.LogDebug($"announcerPath seted");
            _announcerPath = value;
            iniPath = Path.Combine(value, "config.ini");
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
        LogHelper.LogDebug($"AnnouncerConfig changed: {e.PropertyName}");
        ApplyAnnouncerConfig();
    }

    private void ApplyAnnouncerConfig()
    {
        _audioLoader.UpdateSetting(announcerConfig, announcerPath);
        WriteConfigToIni(announcerConfig);
        page.ApplyConfigToUI(announcerConfig);
    }


    private string iniPath;


    public AudioAnnouncer(IAudioLoader audioLoader,
                           ICooldownManager cooldownManager,
                           List<string> displayNameMapping,
                           string title,
                           string defaultAnnouoncerConfigPath)
    {
        this._audioLoader = audioLoader;
        this._cooldownManager = cooldownManager;
        this.category = displayNameMapping;
        this.title = title;
        this._defaultAnnouncerConfigPath = defaultAnnouoncerConfigPath;

        announcerPath = AnnouncerIndex.Get(this.title, _defaultAnnouncerConfigPath);

        SubscribeAnnouncerManager();

        page.Build(title, this);
    }

    private RegistedAnnouncerPage page = new RegistedAnnouncerPage();

    [Description("Parry balls of Maurice -> Hit Maurice -> AscendingRank() -> Postfix() -> PlaySound() -> CheckPlayValidation(), " +
                 "This error will skip all the function before CheckPlayValidation(), That's why try-catch has implemented in the fucntion")]

    /// <summary>Will Play a random audio in the belong category</summary>
    public async Task PlayAudioViaCategory(string category)
    {
        LogHelper.LogInfo($"Request to play audio for category: {category}");
        try
        {
            if (!ValidateAndLogPlayback(category))
                return;

            await PlayAudioClip(category, BepInExConfig.audioPlayOptions.Value);
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
        if (announcerConfig == null)
            announcerConfig = AnnouncerConfigIniInitialization(category);
        else
        {
            announcerConfig.ApplyFrom(AnnouncerConfigIniInitialization(category));
            ApplyAnnouncerConfig();
        }
    }


    /// <summary>Resets the announcer's cooldown</summary>
    public void ResetCooldown()
        => _cooldownManager.ResetCooldowns();


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
            LogHelper.LogInfo($"PlayValidationState: {category}, {validationState}");
            return false;
        }
        return true;
    }

    private async Task PlayAudioClip(string category, int audioPlayOptions = 0)
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

        SoundDispatcher.SendClipToAudioSource(sound, audioPlayOptions);

        SetCooldown(sound.category, announcerConfig.CategorySetting[sound.category].Cooldown);
    }

    private void LogPlaybackError(Exception ex)
        => LogHelper.LogError($"An error occurred while playing sound: {ex.Message}\n{ex.StackTrace}");
    #endregion


    private ValidationState GetPlayValidationState(string category)
    {
        if (_cooldownManager == null || _audioLoader == null)
            return ValidationState.ComponentsNotInitialized;

        if (_audioLoader.announcerConfig.CategorySetting.Keys == null
            || !_audioLoader.announcerConfig.CategorySetting.Keys.Contains(category))
            return ValidationState.InvalidKey;

        if (!announcerConfig.CategorySetting[category].Enabled && announcerConfig.RandomizeAudioOnPlay == false)
            return ValidationState.DisabledByConfig;

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
        LogHelper.LogDebug($"current iniPath for {title}: {iniPath}");
        if (File.Exists(iniPath) == false)
        {
            LogHelper.LogDebug($"Initialize new config.ini in: {iniPath}");
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

        doc = AnnouncerIniMapper.ToIni(doc, announcerConfig);

        IniWriter.Write(iniPath, doc);
    }

    private AnnouncerConfig ReadConfigFromIni()
    {
        if (!File.Exists(iniPath))
            return null;

        var doc = IniReader.Read(iniPath);

        var announcerConfig = AnnouncerIniMapper.FromIni(doc);

        return announcerConfig;
    }

    public void EditExternally()
        => PathHelper.OpenDirectory(announcerPath);

}
