using System;
using System.IO;
using System.Linq;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;

using GreyAnnouncer.AudioSourceComponent;
using GreyAnnouncer.Setting;
using GreyAnnouncer.FrontEnd;

namespace GreyAnnouncer.AnnouncerAPI;

public class AudioAnnouncer
{
    public AnnouncerConfig _announcerConfig;
    private string _announcerConfigJsonName;
    public string title;
    private IAudioLoader _audioLoader;
    private ICooldownManager _cooldownManager;
    private Dictionary<string, string> _displayNameMapping;

    public AudioAnnouncer(IAudioLoader audioLoader,
                           ICooldownManager cooldownManager,
                           Dictionary<string, string> displayNameMapping,
                           string jsonName,
                           string title)
    {
        LogManager.LogInfo($"Initializing AudioAnnouncer {jsonName}");
        this._audioLoader = audioLoader;
        this._cooldownManager = cooldownManager;
        this._displayNameMapping = displayNameMapping;
        this._announcerConfigJsonName = jsonName;
        this.title = title;

        this._announcerConfig = AnnouncerConfigInitialization(jsonName, _displayNameMapping);

        _audioLoader.UpdateAnnouncerConfig(_announcerConfig);
        LogManager.LogInfo("Starting to find available audio asynchronously.");
        _ = _audioLoader.FindAvailableAudioAsync();

        SubscribeAnnouncerManager();

        RegisterRankAnnouncerPagev2.Build(title, this);

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
        await PlayAudioViaCategory(_announcerConfig.CategoryAudioMap.Keys.ToArray()[index]);
    }

    /// <summary>Reload Audio, only works when using Preload and Play options</summary>
    public void ReloadAudio()
    {
        this._announcerConfig = AnnouncerConfigInitialization(_announcerConfigJsonName, _displayNameMapping);
        _audioLoader.UpdateAnnouncerConfig(_announcerConfig);
        _ = _audioLoader.FindAvailableAudioAsync();
    }


    /// <summary>Resets the announcer's cooldown</summary>
    public void ResetCooldown()
    {
        _cooldownManager.ResetCooldowns();
    }

    public void UpdateJsonSetting(AnnouncerConfig newSetting)
    {
        _announcerConfig = newSetting;
        _audioLoader.UpdateAnnouncerConfig(_announcerConfig);
        JsonManager.WriteJson(_announcerConfigJsonName, _announcerConfig);
    }

    /// <summary>Clear audioclip in audioloader, only works when using Preload and Play options</summary>
    public void ClearAudioClipsCache()
    {
        _audioLoader.ClearCache();
    }

    #region Cooldown related
    private void SetCooldown(string category, float cooldown)
    {
        _cooldownManager.StartCooldowns(category, cooldown);
    }
    #endregion


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

        LogManager.LogDebug($"category : {sound.category}, Pitch : {sound.Pitch[0]}, {sound.Pitch[1]}, Cooldown : {_announcerConfig.CategoryAudioMap[category].Cooldown}");

        AudioDispatcher.SendClipToAudioSource(sound, audioPlayOptions);

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

    private static AnnouncerConfig AnnouncerConfigInitialization(string jsonName, Dictionary<string, string> displayNameMapping)
    {
        if (File.Exists(PathManager.GetCurrentPluginPath(jsonName)) == false)
        {
            var audioDict = displayNameMapping.Keys.ToDictionary(
                cat => cat,
                cat => new CategoryAudioSetting
                {
                    DisplayName = displayNameMapping.TryGetValue(cat, out var name) ? name : cat,
                    AudioFiles = new List<string> { cat }
                }
            );
            var jsonSetting = new AnnouncerConfig { CategoryAudioMap = audioDict };
            JsonManager.CreateJson(jsonName, jsonSetting);
        }
        return JsonManager.ReadJson<AnnouncerConfig>(jsonName);
    }

    private void SubscribeAnnouncerManager()
    {
        AnnouncerManager.reloadAnnouncer += ReloadAudio;
        AnnouncerManager.resetCooldown += ResetCooldown;
        AnnouncerManager.clearAudioClipCache += ClearAudioClipsCache;

        AnnouncerManager.AddAnnouncer(this);
    }

    public void ChangeAnnouncerConfig(int index)
    {

    }
}
