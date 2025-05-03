using System;
using System.IO;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;


namespace greycsont.GreyAnnouncer;

public class AudioAnnouncer : IAnnouncer
{
    #region Private Fields
    private AnnouncerJsonSetting m_jsonSetting;
    private AudioLoader          _audioLoader;
    private CooldownManager      _cooldownManager;
    private AudioSourceSetting   _audioSourceConfig;

    private string               m_announcerName;
    private string[]             m_audioCategories;
    private string               m_jsonName;
    private string               m_audioPath;
    #endregion


    #region Public API
    public void Initialize(string announcerName, string[] audioFileNames, string jsonName, string audioPath)
    {
        VariableInitialization(announcerName, audioFileNames, jsonName, audioPath);
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
            SetCooldown(key, InstanceConfig.IndividualRankPlayCooldown.Value);
        }
        catch(Exception ex)
        {
            LogPlaybackError(ex);
        }
    }

    public void ReloadAudio()
    {
        JsonInitialization();
        _audioLoader.UpdateAudioFileNames(m_jsonSetting);
        _audioLoader.FindAvailableAudio();
    }

    public void UpdateAudioPath(string newAudioPaths)
    {
        _audioLoader.UpdateAudioPath(newAudioPaths);
    }

    public void ResetCooldown()
    {
        _cooldownManager.ResetCooldowns();
    }
    #endregion


    #region Initialize related
    private void VariableInitialization(string announcerName, string[] audioCategories, string jsonName, string audioPath)
    {
        this.m_announcerName   = announcerName;
        this.m_jsonName        = jsonName;
        this.m_audioPath       = audioPath;
        this.m_audioCategories = audioCategories;

        _audioSourceConfig = new AudioSourceSetting
        {
            SpatialBlend = 0f,
            Priority     = 0,
            Volume       = InstanceConfig.AudioSourceVolume.Value,
            Pitch        = 1f,
        };
    }

    private void ComponentInitialization()
    {
        JsonInitialization();
        AudioLoaderInitialization();
        CooldownManagerInitialization();
    }

    private void JsonInitialization()
    {
        if (CheckDoesJsonExists() == false) CreateJson();
        m_jsonSetting = JsonManager.ReadJson<AnnouncerJsonSetting>(m_jsonName);
    }

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

    private void AudioLoaderInitialization()
    {
        _audioLoader = new AudioLoader(m_audioPath, m_audioCategories, m_jsonSetting);
        _audioLoader.FindAvailableAudio();
    }

    private void CooldownManagerInitialization()
    {
        _cooldownManager = new CooldownManager(m_jsonSetting.CategoryAudioMap.Count);
    }

    private void RegisterAnnouncer()
    {
        AnnouncerManager.RegisterAnnouncer(m_announcerName, this);
    }
    #endregion


    #region Cooldown related
    public void SetCooldown(int key, float cooldown)
    {
        _cooldownManager.StartSharedCooldown(InstanceConfig.SharedRankPlayCooldown.Value);
        _cooldownManager.StartIndividualCooldown(key, cooldown);
    }
    #endregion


    #region Play Audio related
    private bool ValidateAndLogPlayback(int key)
    {
        var validationState = GetPlayValidationState(key);
        if (validationState != ValidationState.Success)
        {
            Plugin.Log.LogInfo($"PlayValidationState: {_audioLoader.audioCategories[key]}, {validationState}");
            return false;
        }
        return true;
    }
    
    private void PlayAudioClip(int key)
    {
        var clip = _audioLoader.TryToGetAudioClip(key);
        if (clip == null) return;

        switch (InstanceConfig.AudioPlayOptions.Value)
        {
            case 0:
                SoloAudioSource.Instance.PlayOverridable(clip, _audioSourceConfig);
                break;
            case 1:
                AudioSourcePool.Instance.PlayOneShot(clip, _audioSourceConfig);
                break;
            default:
                Plugin.Log.LogWarning("Invalid play audio options, using the default one");
                SoloAudioSource.Instance.PlayOverridable(clip, _audioSourceConfig);
                break;
        }
    }

    private void LogPlaybackError(Exception ex)
    {
        Plugin.Log.LogError($"An error occurred while playing sound: {ex.Message}");
        Plugin.Log.LogError(ex.StackTrace);
    }
    #endregion


    #region Validation related
    private ValidationState GetPlayValidationState(int key)
    {
        if (_cooldownManager == null || _audioLoader == null)
            return ValidationState.ComponentsNotInitialized;

        if (_audioLoader.audioCategories == null || 
            key < 0 || 
            key >= _audioLoader.audioCategories.Length)
            return ValidationState.InvalidKey;

        if (_cooldownManager.IsIndividualCooldownActive(key))
            return ValidationState.IndividualCooldown;

        if (_audioLoader.categoryFailedLoading.Contains(_audioLoader.audioCategories[key]))
            return ValidationState.AudioFailedLoading;

        if (_cooldownManager.IsSharedCooldownActive())
            return ValidationState.SharedCooldown;

        if (!InstanceConfig.RankToggleDict[_audioLoader.audioCategories[key]].Value)
            return ValidationState.DisabledByConfig;

        if (_audioLoader.TryToGetAudioClip(key) == null)
            return ValidationState.ClipNotFound;

        return ValidationState.Success;
    }

    private enum ValidationState
    {
        Success,
        AudioFailedLoading,
        SharedCooldown,
        IndividualCooldown,
        DisabledByConfig,
        ClipNotFound,
        ValidationError,
        ComponentsNotInitialized,
        InvalidKey
    }
    #endregion
}
