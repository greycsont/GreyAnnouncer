using System;
using System.IO;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;


namespace greycsont.GreyAnnouncer;

public class AudioAnnouncer : IAnnouncer
{
    #region Private Fields
    private AnnouncerJsonSetting m_jsonSetting;
    private AudioLoader          m_audioLoader;
    private CooldownManager      m_cooldownManager;
    private AudioSourceSetting   m_audioSourceConfig;

    private string               m_announcerName;
    private string[]             m_audioCategories;
    private string               m_jsonName;
    private string               m_audioPath;
    #endregion


    #region Public API
    public void Initialize(string announcerName, string[] audioCategories, string jsonName, string audioPath)
    {
        VariableInitialization(announcerName, audioCategories, jsonName, audioPath);
        ComponentInitialization();
        RegisterAnnouncer();
    }

    [Description("Parry balls of Maurice -> Hit Maurice -> AscendingRank() -> Postfix() -> PlaySound() -> CheckPlayValidation(), " +
                 "This error will skip all the function before CheckPlayValidation(), That's why try-catch has implemented in the fucntion")]
    public void PlayAudio(int key)
    {
        try
        {
            if (!ValidateAndLogPlayback(key)) return;
            PlayAudioClip(key);
            SetCooldown(key, InstanceConfig.individualRankPlayCooldown.Value);
        }
        catch(Exception ex)
        {
            LogPlaybackError(ex);
        }
    }

    public void ReloadAudio()
    {
        JsonInitialization();
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

    public void UpdateJson(AnnouncerJsonSetting jsonSetting)
    {
        m_jsonSetting = jsonSetting;
        JsonManager.WriteJson(m_jsonName, m_jsonSetting);
    }

    public void ClearAudioClipsCache()
    {
        m_audioLoader.ClearCache();
    }
    #endregion


    #region Initialize related
    private void VariableInitialization(string announcerName, string[] audioCategories, string jsonName, string audioPath)
    {
        this.m_announcerName   = announcerName;
        this.m_jsonName        = jsonName;
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
        JsonInitialization();
        AudioLoaderInitialization();
        CooldownManagerInitialization();
        PluginConfigPanelInitialization();
    }

    private void JsonInitialization()
    {
        if (CheckDoesJsonExists() == false) CreateJson();
        m_jsonSetting = JsonManager.ReadJson<AnnouncerJsonSetting>(m_jsonName);
    }

    private void AudioLoaderInitialization()
    {
        m_audioLoader = new AudioLoader(m_audioPath, m_audioCategories, m_jsonSetting);
        _ = m_audioLoader.FindAvailableAudioAsync();
    }

    private void CooldownManagerInitialization()
    {
        m_cooldownManager = new CooldownManager(m_jsonSetting.CategoryAudioMap.Count);
    }

    private void PluginConfigPanelInitialization()
    {
        // load via reflection
        ReflectionManager.LoadByReflection(
            "greycsont.GreyAnnouncer.RegisterAnnouncerPage", 
            "Build", 
            new object[]{m_announcerName, m_jsonSetting, this});
    }

    private void RegisterAnnouncer()
    {
        AnnouncerManager.RegisterAnnouncer(m_announcerName, this);
    }
    #endregion

    #region Json related
    private bool CheckDoesJsonExists()
    {
        return File.Exists(PathManager.GetCurrentPluginPath(m_jsonName));
    }

    private void CreateJson()
    {
        var audioDict = m_audioCategories.ToDictionary(
            cat => cat,
            cat => new CategoryAudioSetting 
            { 
                Enabled = true,
                AudioFiles = new List<string> { cat }
            }
        );
        m_jsonSetting = new AnnouncerJsonSetting { CategoryAudioMap = audioDict };
        JsonManager.CreateJson(m_jsonName, m_jsonSetting);
    }
    #endregion


    #region Cooldown related
    public void SetCooldown(int key, float cooldown)
    {
        m_cooldownManager.StartSharedCooldown(InstanceConfig.sharedRankPlayCooldown.Value);
        m_cooldownManager.StartIndividualCooldown(key, cooldown);
    }
    #endregion


    #region Play Audio related
    private bool ValidateAndLogPlayback(int key)
    {
        var validationState = GetPlayValidationState(key);
        if (validationState != ValidationState.Success)
        {
            Plugin.log.LogInfo($"PlayValidationState: {m_audioLoader.audioCategories[key]}, {validationState}");
            return false;
        }
        return true;
    }

    private void PlayAudioClip(int key)
    {
        switch (InstanceConfig.audioPlayOptions.Value)
        {
            case 0:
                _ = LoadAndPlayAudioClip(key);
                break;
            case 1:
                PlayAudioClipFromAudioClips(key);
                break;
            default:
                Plugin.log.LogWarning("Invalid play audio options, using the default one");
                _ = LoadAndPlayAudioClip(key);
                break;
        }
    }
    
    private void PlayAudioClipFromAudioClips(int key)
    {
        var clip = m_audioLoader.TryToGetAudioClip(m_audioCategories[key]);
        if (clip == null) return;

        SendClipToAudioSource(clip);
    }

    private async Task LoadAndPlayAudioClip(int key)
    {
        var currentRequestId = ++AnnouncerManager.playRequestId;

        var clip = await m_audioLoader.LoadSingleAudioClipAsync(m_audioCategories[key]);
        if (clip == null) return;

        if (currentRequestId != AnnouncerManager.playRequestId && InstanceConfig.audioPlayOptions.Value == 0)
        {
            Plugin.log.LogInfo($"Aborted outdated audio request for: {m_audioCategories[key]}");
            return;
        }

        SendClipToAudioSource(clip);
    }

    private void SendClipToAudioSource(AudioClip clip)
    {
        switch (InstanceConfig.audioPlayOptions.Value)
        {
            case 0:
                SoloAudioSource.Instance.PlayOverridable(clip, m_audioSourceConfig);
                break;
            case 1:
                AudioSourcePool.Instance.PlayOneShot(clip, m_audioSourceConfig);
                break;
            default:
                Plugin.log.LogWarning("Invalid play audio options, using the default one");
                SoloAudioSource.Instance.PlayOverridable(clip, m_audioSourceConfig);
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
    private ValidationState GetPlayValidationState(int key)
    {
        if (m_cooldownManager == null || m_audioLoader == null)
            return ValidationState.ComponentsNotInitialized;

        if (m_audioLoader.audioCategories == null || 
            key < 0 || 
            key >= m_audioLoader.audioCategories.Length)
            return ValidationState.InvalidKey;

        if (m_cooldownManager.IsIndividualCooldownActive(key))
            return ValidationState.IndividualCooldown;

        if (m_audioLoader.categoryFailedLoading.Contains(m_audioLoader.audioCategories[key]))
            return ValidationState.AudioFailedLoading;

        if (m_cooldownManager.IsSharedCooldownActive())
            return ValidationState.SharedCooldown;
        //edited
        if (!m_jsonSetting.CategoryAudioMap[m_audioCategories[key]].Enabled)
            return ValidationState.DisabledByConfig;
        
        /*if (_audioLoader.TryToGetAudioClip(key) == null)
            return ValidationState.ClipNotFound;*/

        return ValidationState.Success;
    }

    
    #endregion
}
