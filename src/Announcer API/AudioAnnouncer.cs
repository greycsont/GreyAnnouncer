using System;
using System.Linq;
using System.ComponentModel;
using UnityEngine;
using System.Threading.Tasks;

using GreyAnnouncer.AudioSourceComponent;

namespace GreyAnnouncer.AnnouncerAPI;

public class AudioAnnouncer
{
    #region Private Fields
    private    AnnouncerJsonSetting    m_jsonSetting;
    private    AudioLoader             m_audioLoader;
    private    IAudioLoader            audioLoader;
    private    CooldownManager         m_cooldownManager;
    private    ICooldownManager        cooldownManager;
    private    AudioSourceSetting      m_audioSourceConfig;
    
    private    string[]                m_audioCategories;
    private    string                  m_audioPath;
    #endregion


    #region Public API
    public void Initialize(string[] audioCategories, AnnouncerJsonSetting jsonSetting, string audioPath, IAudioLoader audioLoader, ICooldownManager cooldownManager)
    {
        VariableInitialization(audioCategories, jsonSetting, audioPath);
        this.audioLoader = audioLoader;
        this.cooldownManager = cooldownManager;
        ComponentInitialization();
    }

    [Description("Parry balls of Maurice -> Hit Maurice -> AscendingRank() -> Postfix() -> PlaySound() -> CheckPlayValidation(), " +
                 "This error will skip all the function before CheckPlayValidation(), That's why try-catch has implemented in the fucntion")]
    public void PlayAudioViaCategory(string category)
    {
        try
        {
            if (!ValidateAndLogPlayback(category)) return;
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
        PlayAudioViaCategory(m_audioCategories[index]);
    }

    public void ReloadAudio(AnnouncerJsonSetting jsonSetting)
    {
        this.m_jsonSetting = jsonSetting; 
        m_audioLoader.UpdateJsonSetting(m_jsonSetting);
        _ = m_audioLoader.FindAvailableAudioAsync();
    }

    public void UpdateAudioPath(string newAudioPaths)
    {
        m_audioLoader.UpdateAudioPath(newAudioPaths);
    }

    public void ResetCooldown()
    {
        m_cooldownManager.ResetCooldowns();
    }

    public void ClearAudioClipsCache()
    {
        m_audioLoader.ClearCache();
    }

    public void UpdateJsonSetting(AnnouncerJsonSetting jsonSetting)
    {
        this.m_jsonSetting = jsonSetting;
        m_audioLoader.UpdateJsonSetting(jsonSetting);
    }
    #endregion


    #region Initialize related
    private void VariableInitialization(string[] audioCategories, AnnouncerJsonSetting jsonSetting, string audioPath)
    {
        this.m_jsonSetting     = jsonSetting;
        this.m_audioPath       = audioPath;
        this.m_audioCategories = audioCategories;

        m_audioSourceConfig = new AudioSourceSetting
        {
            SpatialBlend = 0f,
            Priority     = 0,
            Volume       = InstanceConfig.audioSourceVolume.Value,
            Pitch        = 1f,
        };
    }

    private void ComponentInitialization()
    {
        AudioLoaderInitialization();
        CooldownManagerInitialization();
    }


    private void AudioLoaderInitialization()
    {
        m_audioLoader = new AudioLoader(m_audioPath, m_audioCategories, m_jsonSetting);
        _ = m_audioLoader.FindAvailableAudioAsync();
    }

    private void CooldownManagerInitialization()
    {
        m_cooldownManager = new CooldownManager(m_audioCategories);
    }
    #endregion


    #region Cooldown related
    public void SetCooldown(string category, float cooldown)
    {
        m_cooldownManager.StartCooldowns(category, cooldown);
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
        AudioClip clip = null;
        if (InstanceConfig.isAudioRandomizationEnabled.Value == false)
        {
            clip = m_audioLoader.GetClipFromCache(category);
        }
        else
        {
            clip = m_audioLoader.GetRandomClipFromAudioClips();
        }
        if (clip == null) return;

        AudioDispatcher.SendClipToAudioSource(clip, m_audioSourceConfig, InstanceConfig.audioPlayOptions.Value);
    }

    private async Task LoadAndPlayAudioClip(string category)
    {
        AudioClip clip = null;

        var currentRequestId = ++AnnouncerManager.playRequestId;

        if (InstanceConfig.isAudioRandomizationEnabled.Value == false)
        {
            clip = await m_audioLoader.LoadAndGetSingleAudioClipAsync(category);
        }
        else
        {
            clip = await m_audioLoader.GetRandomClipFromAllAvailableFiles();
        }

        if (clip == null) return;

        if (
            currentRequestId != AnnouncerManager.playRequestId 
            && InstanceConfig.audioPlayOptions.Value == 0
        )
        {
            LogManager.LogInfo($"Aborted outdated audio request for: {category}");
            return;
        }

        AudioDispatcher.SendClipToAudioSource(clip, m_audioSourceConfig, InstanceConfig.audioPlayOptions.Value);
    }

    private void LogPlaybackError(Exception ex)
    {
        LogManager.LogError($"An error occurred while playing sound: {ex.Message}");
        LogManager.LogError(ex.StackTrace);
    }
    #endregion


    #region Validation related
    private ValidationState GetPlayValidationState(string category)
    {
        if (m_cooldownManager == null || m_audioLoader == null){
            return ValidationState.ComponentsNotInitialized;
        }


        if (m_audioLoader.jsonSetting.CategoryAudioMap.Keys == null || !m_audioLoader.jsonSetting.CategoryAudioMap.Keys.Contains(category)){
            return ValidationState.InvalidKey;
        }


        if (m_cooldownManager.IsIndividualCooldownActive(category)){
            return ValidationState.IndividualCooldown;
        }


        if (m_cooldownManager.IsSharedCooldownActive()){
            return ValidationState.SharedCooldown;
        }


        if (!m_jsonSetting.CategoryAudioMap[category].Enabled){
            return ValidationState.DisabledByConfig;
        }

        return ValidationState.Success;
    }

    #endregion
}
