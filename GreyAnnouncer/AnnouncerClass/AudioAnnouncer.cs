using System;
using System.IO;


namespace greycsont.GreyAnnouncer;

public class AudioAnnouncer
{
    private JsonSetting_v2     jsonSetting;
    private AudioLoader        _audioLoader;
    private CooldownManager    _cooldownManager;
    private AudioSourceSetting _audioSourceConfig;

    private string[]           audioFileNames;
    private string             jsonName;
    private string             audioPath;



    #region Public Methods
    public void Initialize(string[] audioFileNames, string jsonName, string audioPath)
    {
        VariableInitialization(audioFileNames, jsonName, audioPath);
        ComponentInitialization();
    }

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
        _audioLoader.audioFileNames = jsonSetting.AudioNames;
        _audioLoader.FindAvailableAudio();
    }

    public void UpdateAudioPaths(string newAudioPaths)
    {
        _audioLoader.UpdateAudioPaths(newAudioPaths);
    }

    public void ResetTimerToZero()
    {
        _cooldownManager.ResetCooldowns();
    }
    #endregion


    #region Initialize related
    private void VariableInitialization(string[] audioFileNames, string jsonName, string audioPath)
    {
        this.jsonName = jsonName;
        this.audioPath = audioPath;
        this.audioFileNames = audioFileNames;

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
        jsonSetting = JsonManager.ReadJson<JsonSetting_v2>(jsonName);
    }

    private bool CheckDoesJsonExists()
    {
        return File.Exists(PathManager.GetCurrentPluginPath(jsonName));
    }

    private void CreateJson()
    {
        var jsonSetting = new JsonSetting_v2 { AudioNames = audioFileNames };
        JsonManager.CreateJson(jsonName, jsonSetting);
    }

    private void AudioLoaderInitialization()
    {
        _audioLoader = new AudioLoader(audioPath, audioFileNames, jsonSetting.AudioNames);
        _audioLoader.FindAvailableAudio();
    }

    private void CooldownManagerInitialization()
    {
        _cooldownManager = new CooldownManager(jsonSetting.AudioNames.Length);
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
        // 检查组件是否为空
        if (_cooldownManager == null || _audioLoader == null)
            return ValidationState.ComponentsNotInitialized;

        // 检查数组越界
        if (_audioLoader.audioCategories == null || 
            key < 0 || 
            key >= _audioLoader.audioCategories.Length)
            return ValidationState.InvalidKey;

        if (_cooldownManager.IsIndividualCooldownActive(key))
            return ValidationState.IndividualCooldown;

        if (_audioLoader.categoreFailedLoading.Contains(_audioLoader.audioCategories[key]))
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
