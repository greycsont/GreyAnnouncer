using System;
using System.Linq;
using System.ComponentModel;
using UnityEngine;
using System.Threading.Tasks;


namespace greycsont.GreyAnnouncer;

public class AudioAnnouncer
{
    #region Private Fields
    private AnnouncerJsonSetting m_jsonSetting;
    private AudioLoader          m_audioLoader;
    private CooldownManager      m_cooldownManager;
    private AudioSourceSetting   m_audioSourceConfig;
    
    private string[]             m_audioCategories;
    private string               m_audioPath;
    #endregion


    #region Public API
    public void Initialize(string[] audioCategories, AnnouncerJsonSetting jsonSetting, string audioPath)
    {
        VariableInitialization(audioCategories, jsonSetting, audioPath);
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
        m_audioLoader.UpdateAudioFileNames(m_jsonSetting);
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
        m_cooldownManager.StartSharedCooldown(InstanceConfig.sharedPlayCooldown.Value);
        m_cooldownManager.StartIndividualCooldown(category, cooldown);
    }
    #endregion


    #region Play Audio related
    private bool ValidateAndLogPlayback(string category)
    {
        var validationState = GetPlayValidationState(category);
        if (validationState != ValidationState.Success)
        {
            Plugin.log.LogInfo($"PlayValidationState: {category}, {validationState}");
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
                Plugin.log.LogWarning("Invalid play audio options, using the default one");
                _ = LoadAndPlayAudioClip(category);
                break;
        }
    }
    
    private void PlayAudioClipFromAudioClips(string category)
    {
        var clip = m_audioLoader.GetClipFromAudioClips(category);
        if (clip == null) return;

        SendClipToAudioSource(clip);
    }

    private async Task LoadAndPlayAudioClip(string category)
    {
        var currentRequestId = ++AnnouncerManager.playRequestId;

        var clip = await m_audioLoader.LoadSingleAudioClipAsync(category);
        if (clip == null) return;

        if (
            currentRequestId != AnnouncerManager.playRequestId 
            && InstanceConfig.audioPlayOptions.Value == 0
        )
        {
            Plugin.log.LogInfo($"Aborted outdated audio request for: {category}");
            return;
        }

        SendClipToAudioSource(clip);
        AnnouncerManager.ClearAudioClipsCache();
    }

    private void SendClipToAudioSource(AudioClip clip)
    {
        switch (InstanceConfig.audioPlayOptions.Value)
        {
            case 0:
                SoloAudioSource.Instance.PlayOneShot(clip, m_audioSourceConfig);
                break;
            case 1:
                AudioSourcePool.Instance.PlayOneShot(clip, m_audioSourceConfig);
                break;
            default:
                Plugin.log.LogWarning("Invalid play audio options, using the default one");
                SoloAudioSource.Instance.PlayOneShot(clip, m_audioSourceConfig);
                break;
        }
    }

    private void LogPlaybackError(Exception ex)
    {
        Plugin.log.LogError($"An error occurred while playing sound: {ex.Message}");
        Plugin.log.LogError(ex.StackTrace);
    }
    #endregion


    #region Validation related
    private ValidationState GetPlayValidationState(string category)
    {
        if (m_cooldownManager == null || m_audioLoader == null){
            return ValidationState.ComponentsNotInitialized;
        }


        if (m_audioLoader.audioCategories == null || !m_audioLoader.audioCategories.Contains(category)){
            return ValidationState.InvalidKey;
        }


        if (m_cooldownManager.IsIndividualCooldownActive(category)){
            return ValidationState.IndividualCooldown;
        }
           

        if (m_audioLoader.categoryFailedLoading.Contains(category)){
            return ValidationState.AudioFailedLoading;
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
