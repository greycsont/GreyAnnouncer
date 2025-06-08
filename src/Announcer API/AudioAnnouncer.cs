using System;
using System.Linq;
using System.ComponentModel;
using UnityEngine;
using System.Threading.Tasks;

using GreyAnnouncer.AudioSourceComponent;

namespace GreyAnnouncer.AnnouncerAPI;

public class AudioAnnouncer
{
    private    AnnouncerJsonSetting    _jsonSetting;
    private    IAudioLoader            _audioLoader;
    private    ICooldownManager        _cooldownManager;

    public void Initialize(AnnouncerJsonSetting jsonSetting, 
                           IAudioLoader audioLoader, 
                           ICooldownManager cooldownManager)
    {
        this._jsonSetting = jsonSetting;
        this._audioLoader = audioLoader;
        this._cooldownManager = cooldownManager;
    }

    [Description("Parry balls of Maurice -> Hit Maurice -> AscendingRank() -> Postfix() -> PlaySound() -> CheckPlayValidation(), " +
                 "This error will skip all the function before CheckPlayValidation(), That's why try-catch has implemented in the fucntion")]
                 
    /// <summary>Will Play a random audio in the belong category</summary>
    public async Task PlayAudioViaCategory(string category)
    {
        try
        {
            if (!ValidateAndLogPlayback(category))
                return;
                
            if (await PlayAudioClip(category, InstanceConfig.audioPlayOptions.Value))
                SetCooldown(category, InstanceConfig.individualPlayCooldown.Value);
        }
        catch (Exception ex)
        {
            LogPlaybackError(ex);
        }
    }

    /// <summary>Will Play a random audio in the belong category by jsonSetting mapping via index</summary>
    public async Task PlayAudioViaIndex(int index)
    {
        await PlayAudioViaCategory(_jsonSetting.CategoryAudioMap.Keys.ToArray()[index]);
    }

    /// <summary>Reload Audio, only works when using Preload and Play options</summary>
    public void ReloadAudio(AnnouncerJsonSetting jsonSetting)
    {
        this._jsonSetting = jsonSetting;
        _audioLoader.UpdateJsonSetting(_jsonSetting);
        _ = _audioLoader.FindAvailableAudioAsync();
    }

    /// <summary>Updates path stored audio files</summary>
    public void UpdateAudioPath(string newAudioPaths)
    {
        _audioLoader.UpdateAudioPath(newAudioPaths);
    }

    /// <summary>Resets the announcer's cooldown</summary>
    public void ResetCooldown()
    {
        _cooldownManager.ResetCooldowns();
    }

    /// <summary>Clear audioclip in audioloader, only works when using Preload and Play options</summary>
    public void ClearAudioClipsCache()
    {
        _audioLoader.ClearCache();
    }

    /// <summary>Update jsonSetting and send the setting to other component</summary>
    public void UpdateJsonSetting(AnnouncerJsonSetting jsonSetting)
    {
        this._jsonSetting = jsonSetting;
        _audioLoader.UpdateJsonSetting(jsonSetting);
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

            LogManager.LogInfo($"PlayValidationState: {category}, {validationState}");
            
            return false;
        }
        return true;
    }

    private async Task<bool> PlayAudioClip(string category, int audioPlayOptions = 0)
    {
        AudioClipWithCategory? clip = null;
        // 需要改成加载AudioClip后直接传过去，省的这么多东西（
        // 改好了
        switch (audioPlayOptions)
        {
            case 0:
                clip = await LoadAndPlayAudioClip(category);
                break;
            case 1:
                clip = PlayAudioClipFromAudioClips(category);
                break;
            default:
                LogManager.LogWarning("Invalid play audio options, using the default one");
                clip = await LoadAndPlayAudioClip(category);
                break;
        }

        if (clip == null)
        {
            LogManager.LogError($"Failed to load audio clip for category: {category}");
            return false;
        }

        var audioSourceConfig = new AudioSourceSetting
        {
            SpatialBlend = 0f,
            Priority = 0,
            Volume = InstanceConfig.audioSourceVolume.Value,
            Pitch = _jsonSetting.CategoryAudioMap[clip.Value.category].Pitch,
        };

        var volumeMultiplier = _jsonSetting.CategoryAudioMap[clip.Value.category].VolumeMultiplier;

        LogManager.LogInfo($"category : {clip.Value.category}, Pitch : {audioSourceConfig.Pitch}");

        AudioDispatcher.SendClipToAudioSource(clip.Value.clip,
                                              audioSourceConfig,
                                              audioPlayOptions,
                                              volumeMultiplier);

        return true;

    }
    
    private AudioClipWithCategory? PlayAudioClipFromAudioClips(string category)
    {
        AudioClipWithCategory? clip;
        if (InstanceConfig.isAudioRandomizationEnabled.Value == false)
        {
            clip = _audioLoader.GetClipFromCache(category);
        }
        else
        {
            clip = _audioLoader.GetRandomClipFromAudioClips();
        }

        if (clip == null) 
            return null;

        return clip;
    }

    private async Task<AudioClipWithCategory?> LoadAndPlayAudioClip(string category)
    {
        AudioClipWithCategory? clip = null;

        var currentRequestId = ++AnnouncerManager.playRequestId;

        if (InstanceConfig.isAudioRandomizationEnabled.Value == false)
        {
            clip = await _audioLoader.LoadAndGetSingleAudioClipAsync(category);
        }
        else
        {
            clip = await _audioLoader.GetRandomClipFromAllAvailableFiles();
        }
        
        if (clip == null) 
            return null;

        if (
            currentRequestId != AnnouncerManager.playRequestId
            && InstanceConfig.audioPlayOptions.Value == 0
        )
        {
            LogManager.LogInfo($"Aborted outdated audio request for: {category}");
            return null;
        }

        return clip;
    }

    private void LogPlaybackError(Exception ex)
    {
        LogManager.LogError($"An error occurred while playing sound: {ex.Message}");
        LogManager.LogError(ex.StackTrace);
    }
    #endregion


    private ValidationState GetPlayValidationState(string category)
    {
        if (_cooldownManager == null || _audioLoader == null){
            return ValidationState.ComponentsNotInitialized;
        }


        if (_audioLoader.jsonSetting.CategoryAudioMap.Keys == null 
            || !_audioLoader.jsonSetting.CategoryAudioMap.Keys.Contains(category)){
            return ValidationState.InvalidKey;
        }


        if (_cooldownManager.IsIndividualCooldownActive(category)){
            return ValidationState.IndividualCooldown;
        }


        if (_cooldownManager.IsSharedCooldownActive()){
            return ValidationState.SharedCooldown;
        }


        if (!_jsonSetting.CategoryAudioMap[category].Enabled){
            return ValidationState.DisabledByConfig;
        }

        return ValidationState.Success;
    }
}
