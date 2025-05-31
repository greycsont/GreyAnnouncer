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
    public void PlayAudioViaCategory(string category)
    {
        try
        {
            if (!ValidateAndLogPlayback(category))
                return;
            PlayAudioClip(category);
            SetCooldown(category, InstanceConfig.individualPlayCooldown.Value);
        }
        catch(Exception ex)
        {
            LogPlaybackError(ex);
        }
    }

    public void PlayAudioViaIndex(int index)
    {
        PlayAudioViaCategory(_jsonSetting.CategoryAudioMap.Keys.ToArray()[index]);
    }

    public void ReloadAudio(AnnouncerJsonSetting jsonSetting)
    {
        this._jsonSetting = jsonSetting; 
        _audioLoader.UpdateJsonSetting(_jsonSetting);
        _ = _audioLoader.FindAvailableAudioAsync();
    }

    public void UpdateAudioPath(string newAudioPaths)
    {
        _audioLoader.UpdateAudioPath(newAudioPaths);
    }

    public void ResetCooldown()
    {
        _cooldownManager.ResetCooldowns();
    }

    public void ClearAudioClipsCache()
    {
        _audioLoader.ClearCache();
    }

    public void UpdateJsonSetting(AnnouncerJsonSetting jsonSetting)
    {
        this._jsonSetting = jsonSetting;
        _audioLoader.UpdateJsonSetting(jsonSetting);
    }


    #region Cooldown related
    public void SetCooldown(string category, float cooldown)
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

    private void PlayAudioClip(string category)
    {
        // 需要改成加载AudioClip后直接传过去，省的这么多东西（
        switch (InstanceConfig.audioLoadingOptions.Value)
        {
            case 0:
                _ = LoadAndPlayAudioClip(category);
                break;
            case 1:
                PlayAudioClipFromAudioClips(category);
                break;
            default:
                LogManager.LogWarning("Invalid play audio options, using the default one");
                _ = LoadAndPlayAudioClip(category);
                break;
        } 
    }
    
    private void PlayAudioClipFromAudioClips(string category)
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
            return;

        var audioSourceConfig = new AudioSourceSetting
        {
            SpatialBlend = 0f,
            Priority     = 0,
            Volume       = InstanceConfig.audioSourceVolume.Value,
            Pitch        = _jsonSetting.CategoryAudioMap[clip.Value.category].Pitch,
        };

        var volumeMultiplier = _jsonSetting.CategoryAudioMap[clip.Value.category].VolumeMultiplier;

        LogManager.LogInfo($"category : {clip.Value.category}, Pitch : {audioSourceConfig.Pitch}");
        
        AudioDispatcher.SendClipToAudioSource(clip.Value.clip, 
                                              audioSourceConfig, 
                                              InstanceConfig.audioPlayOptions.Value,
                                              volumeMultiplier);
    }

    private async Task LoadAndPlayAudioClip(string category)
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
            return;

        if (
            currentRequestId != AnnouncerManager.playRequestId
            && InstanceConfig.audioPlayOptions.Value == 0
        )
        {
            LogManager.LogInfo($"Aborted outdated audio request for: {category}");
            return;
        }

        var audioSourceConfig = new AudioSourceSetting
        {
            SpatialBlend = 0f,
            Priority     = 0,
            Volume       = InstanceConfig.audioSourceVolume.Value,
            Pitch        = _jsonSetting.CategoryAudioMap[clip.Value.category].Pitch,
        };

        var volumeMultiplier = _jsonSetting.CategoryAudioMap[clip.Value.category].VolumeMultiplier;
        
        LogManager.LogInfo($"category : {clip.Value.category}, Pitch : {audioSourceConfig.Pitch}");

        AudioDispatcher.SendClipToAudioSource(clip.Value.clip, 
                                              audioSourceConfig, 
                                              InstanceConfig.audioPlayOptions.Value,
                                              volumeMultiplier);
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
