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
    public AnnouncerConfig _announcerConfig;

    private string _announcerConfigJsonName = "RankAnnouncer.json";

    public string title;

    private IAudioLoader _audioLoader;

    private ICooldownManager _cooldownManager;

    private List<string> category;


    public AudioAnnouncer(IAudioLoader audioLoader,
                           ICooldownManager cooldownManager,
                           List<string> displayNameMapping,
                           string title)
    {
        this._audioLoader = audioLoader;
        this._cooldownManager = cooldownManager;
        this.category = displayNameMapping;
        this.title = title;

        this._announcerConfig = AnnouncerConfigInitialization(category);

        _audioLoader.UpdateAnnouncerConfig(_announcerConfig);
        LogManager.LogInfo("Starting to find available audio asynchronously.");
        _ = _audioLoader.FindAvailableAudioAsync();

        SubscribeAnnouncerManager();

        RegisterRankAnnouncerPagev2.Build(title, this);

        WriteConfigToIni();

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
                SetCooldown(category, _announcerConfig.CategoryAudioMap[category].Cooldown);
        }
        catch (Exception ex)
        {
            LogPlaybackError(ex);
        }
    }

    /// <summary>Will Play a random audio in the belong category by jsonSetting mapping via index</summary>
    public async Task PlayAudioViaIndex(int index)
    {
        LogManager.LogInfo($"{index}");
        await PlayAudioViaCategory(_announcerConfig.CategoryAudioMap.Keys.ToArray()[index]);
    }

    /// <summary>Reload Audio, only works when using Preload and Play options</summary>
    public void ReloadAudio()
    {
        this._announcerConfig = AnnouncerConfigInitialization(category);
        _audioLoader.UpdateAnnouncerConfig(_announcerConfig);
        _ = _audioLoader.FindAvailableAudioAsync();
    }


    /// <summary>Resets the announcer's cooldown</summary>
    public void ResetCooldown()
        => _cooldownManager.ResetCooldowns();

    public void UpdateJsonSetting(AnnouncerConfig newSetting)
    {
        _announcerConfig = newSetting;
        _audioLoader.UpdateAnnouncerConfig(_announcerConfig);
        JsonManager.WriteJson(_announcerConfigJsonName, _announcerConfig);
    }

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
    {
        LogManager.LogError($"An error occurred while playing sound: {ex.Message}");
        LogManager.LogError(ex.StackTrace);
    }
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

    private AnnouncerConfig AnnouncerConfigInitialization(List<string> category)
    {
        if (File.Exists(PathManager.GetCurrentPluginPath(_announcerConfigJsonName)) == false)
        {
            var audioDict = category.ToDictionary(
                cat => cat,
                cat => new CategoryAudioSetting
                {
                    AudioFiles = new List<string> { cat }
                }
            );
            var jsonSetting = new AnnouncerConfig { CategoryAudioMap = audioDict };
            JsonManager.CreateJson(_announcerConfigJsonName, jsonSetting);
        }
        return JsonManager.ReadJson<AnnouncerConfig>(_announcerConfigJsonName);
    }

    private void SubscribeAnnouncerManager()
    {
        AnnouncerManager.reloadAnnouncer += ReloadAudio;
        AnnouncerManager.resetCooldown += ResetCooldown;
        AnnouncerManager.clearAudioClipCache += ClearAudioClipsCache;

        AnnouncerManager.AddAnnouncer(this);
    }

    private void WriteConfigToIni()
    {
        var doc = new IniDocument();
        doc = IniMapper.ToIni(doc, _announcerConfig, "General"); // 写 General
        foreach (var pair in _announcerConfig.CategoryAudioMap)
        {
            doc = IniMapper.ToIni(doc, pair.Value, $"Category:{pair.Key}");
        }
        IniWriter.Write(PathManager.GetCurrentPluginPath("announcer.ini"), doc);
    }

    public void ChangeAnnouncerConfig(int index)
    {

    }
}
