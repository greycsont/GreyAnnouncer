using System;
using System.IO;
using System.Linq;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

using GreyAnnouncer.AudioSourceComponent;
using GreyAnnouncer;
using GameConsole.pcon;

namespace GreyAnnouncer.AnnouncerAPI;

public class AudioAnnouncer
{
    public AnnouncerMapping _jsonSetting;
    private string _jsonName;
    private    IAudioLoader            _audioLoader;
    private    ICooldownManager        _cooldownManager;
    private Dictionary<string, string> _displayNameMapping;

    public AudioAnnouncer(IAudioLoader audioLoader,
                           ICooldownManager cooldownManager,
                           Dictionary<string, string> displayNameMapping,
                           string jsonName)
    {
        LogManager.LogInfo($"Initializing AudioAnnouncer {jsonName}");
        this._audioLoader = audioLoader;
        this._cooldownManager = cooldownManager;
        this._displayNameMapping = displayNameMapping;
        this._jsonName = jsonName;
        this._jsonSetting = JsonInitialization(jsonName, _displayNameMapping);

        _audioLoader.UpdateJsonSetting(_jsonSetting);
        LogManager.LogInfo("Starting to find available audio asynchronously.");
        _ = _audioLoader.FindAvailableAudioAsync();

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
                SetCooldown(category, BepInExConfig.individualPlayCooldown.Value);
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
    public void ReloadAudio()
    {
        this._jsonSetting = JsonInitialization(_jsonName, _displayNameMapping);
        _audioLoader.UpdateJsonSetting(_jsonSetting);
        _ = _audioLoader.FindAvailableAudioAsync();
    }


    /// <summary>Resets the announcer's cooldown</summary>
    public void ResetCooldown()
    {
        _cooldownManager.ResetCooldowns();
    }

    public void UpdateJsonSetting(AnnouncerMapping newSetting)
    {
        _jsonSetting = newSetting;
        _audioLoader.UpdateJsonSetting(_jsonSetting);
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

        LogManager.LogInfo($"category : {sound.category}, Pitch : {sound.Pitch}");

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


        if (_audioLoader.jsonSetting.CategoryAudioMap.Keys == null
            || !_audioLoader.jsonSetting.CategoryAudioMap.Keys.Contains(category))
        {
            return ValidationState.InvalidKey;
        }


        if (_cooldownManager.IsIndividualCooldownActive(category))
        {
            return ValidationState.IndividualCooldown;
        }


        if (_cooldownManager.IsSharedCooldownActive())
        {
            return ValidationState.SharedCooldown;
        }


        if (!_jsonSetting.CategoryAudioMap[category].Enabled)
        {
            return ValidationState.DisabledByConfig;
        }

        return ValidationState.Success;
    }

    private static AnnouncerMapping JsonInitialization(string jsonName, Dictionary<string, string> displayNameMapping)
    {
        if (File.Exists(PathManager.GetCurrentPluginPath(jsonName)) == false)
        {
            var audioDict = displayNameMapping.Keys.ToDictionary(
                cat => cat,
                cat => new CategoryAudioSetting
                {
                    Enabled = true,
                    DisplayName = displayNameMapping.TryGetValue(cat, out var name) ? name : cat,
                    Pitch = 1f,
                    AudioFiles = new List<string> { cat }
                }
            );
            var jsonSetting = new AnnouncerMapping { CategoryAudioMap = audioDict };
            JsonManager.CreateJson(jsonName, jsonSetting);
        }
        return JsonManager.ReadJson<AnnouncerMapping>(jsonName);
    }
}
