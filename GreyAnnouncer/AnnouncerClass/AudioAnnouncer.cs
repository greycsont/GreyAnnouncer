﻿using System;
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
    public void PlayAudioViaCategory(string category)
    {
        try
        {
            if (!ValidateAndLogPlayback(category)) return;
            PlayAudioClip(category);
            SetCooldown(category, InstanceConfig.individualRankPlayCooldown.Value);
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
        m_cooldownManager = new CooldownManager(m_audioCategories);
    }

    private void PluginConfigPanelInitialization()
    {
        ReflectionManager.LoadByReflection(
            "greycsont.GreyAnnouncer.RegisterAnnouncerPage", 
            "Build", 
            new object[]{m_announcerName, m_jsonSetting, this}
        );
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
    public void SetCooldown(string category, float cooldown)
    {
        m_cooldownManager.StartSharedCooldown(InstanceConfig.sharedRankPlayCooldown.Value);
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
    }

    private void SendClipToAudioSource(AudioClip clip)
    {
        switch (InstanceConfig.audioPlayOptions.Value)
        {
            case 0:
                SoloAudioSource.Instance.Play(clip, m_audioSourceConfig);
                break;
            case 1:
                AudioSourcePool.Instance.Play(clip, m_audioSourceConfig);
                break;
            default:
                Plugin.log.LogWarning("Invalid play audio options, using the default one");
                SoloAudioSource.Instance.Play(clip, m_audioSourceConfig);
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
        if (m_cooldownManager == null || m_audioLoader == null)
            return ValidationState.ComponentsNotInitialized;

        if (m_audioLoader.audioCategories == null || !m_audioLoader.audioCategories.Contains(category))
            return ValidationState.InvalidKey;

        if (m_cooldownManager.IsIndividualCooldownActive(category))
            return ValidationState.IndividualCooldown;

        if (m_audioLoader.categoryFailedLoading.Contains(category))
            return ValidationState.AudioFailedLoading;

        if (m_cooldownManager.IsSharedCooldownActive())
            return ValidationState.SharedCooldown;

        if (!m_jsonSetting.CategoryAudioMap[category].Enabled)
            return ValidationState.DisabledByConfig;

        return ValidationState.Success;
    }

    
    #endregion
}
