using System.Collections;
using UnityEngine;
using System;
using System.ComponentModel;

/* RankAnnouncer.cs requires :
            PathManager.cs to find and fetch audio
            AudioSourceManager.cs to add LowPassFilter
            CoroutineRunner.cs for set timer
            InstanceConfig.cs for setting
            JsonSetting.cs for setting */
namespace greycsont.GreyAnnouncer;

public class RankAnnouncer
{       
    private static readonly string[]                   rankNames                  = new string[] {"D", "C", "B", "A", "S", "SS", "SSS", "U"};
    private static          float[]                    individualRankPlayCooldown;
    private static          float                      sharedRankPlayCooldown     = 0f;
    private static          AudioLoader                _audioLoader;
    private static          AudioSourceSetting   audioSourceConfig          = new AudioSourceSetting
    {
        SpatialBlend = 0f,
        Priority     = 0,
        Volume       = InstanceConfig.AudioSourceVolume.Value,
        Pitch        = 1f,
    };
    

    public static void Initialize()
    {
        individualRankPlayCooldown = new float[rankNames.Length];

        _audioLoader = new AudioLoader
        (
            InstanceConfig.AudioFolderPath.Value,
            rankNames
        );

        _audioLoader.FindAvailableAudio();
    }

    [Description("Parry balls of Maurice -> Hit Maurice -> AscendingRank() -> Postfix() -> PlaySound() -> CheckPlayValidation(), " +
                 "This bug will skip all the function before CheckPlayValidation(),  The try-catch has implemented in the fucntion")]
    public static void PlaySound(int rank)
    {
        try{
            var ValidationState = GetPlayValidationState(rank);
            if (ValidationState != ValidationState.Success) 
            {
                Plugin.Log.LogInfo($"PlayValidationState: {_audioLoader.audioCategories[rank]}, {ValidationState}");
                return;
            }

            AudioClip clip = _audioLoader.TryToGetAudioClip(rank);
            if (clip == null) return;

            if (true) SoloAudioSource.Instance.PlayOverridable(clip, audioSourceConfig);
            //else if (true) AudioSourcePool.Instance.PlayOneShot(clip, audioSourceConfig);
            StartRankCooldownCoroutine(rank);
        }
        catch(Exception ex)
        {
            Plugin.Log.LogError($"An error occurred while playing sound: {ex.Message}");
            Plugin.Log.LogError(ex.StackTrace);
        }

    }

    private static void StartRankCooldownCoroutine(int rank)
    {
        CoroutineRunner.Instance.StartCoroutine(CooldownCoroutine(value => 
            sharedRankPlayCooldown           = value, InstanceConfig.SharedRankPlayCooldown.Value
        ));
            
        CoroutineRunner.Instance.StartCoroutine(CooldownCoroutine(value => 
            individualRankPlayCooldown[rank] = value, InstanceConfig.IndividualRankPlayCooldown.Value
        ));
    }

    public static void ReloadAudio()
    {
        _audioLoader.ReloadAudio();
    }

    public static void UpdateAudioFolderPath(string newPath)
    {
        _audioLoader.UpdateAudioPaths(newPath);
    }

    public static void ResetTimerToZero()
    {
        sharedRankPlayCooldown = 0f;
        Array.Clear(individualRankPlayCooldown, 0, individualRankPlayCooldown.Length);
    }

    private static IEnumerator CooldownCoroutine(Action<float> setCooldown, float initialCooldown)
    {
        if (initialCooldown <= 0)
        {
            setCooldown(0);
            yield break;
        }

        float cooldown      = initialCooldown;
        float waitTime      = cooldown * 3 / 4f;
        float deltaTimeTime = cooldown * 1 / 4f;
        setCooldown(cooldown);

        yield return new WaitForSeconds(waitTime);

        float timePassed = 0f;
        while (timePassed < deltaTimeTime)
        {
            timePassed += Time.deltaTime;
            cooldown   -= Time.deltaTime;
            setCooldown(cooldown);
            yield return null;
        }

        setCooldown(0); 
    }

    private static ValidationState GetPlayValidationState(int rank)
    {
        if (individualRankPlayCooldown[rank] > 0f) 
            return ValidationState.IndividualCooldown;

        if (_audioLoader.categoreFailedLoading.Contains(rankNames[rank]))
            return ValidationState.AudioFailedLoading;

        if (sharedRankPlayCooldown > 0f) 
            return ValidationState.SharedCooldown;    

        if (!InstanceConfig.RankToggleDict[rankNames[rank]].Value) 
            return ValidationState.DisabledByConfig;

        if (_audioLoader.TryToGetAudioClip(rank) == null)
            return ValidationState.ClipNotFound;

        return ValidationState.Success;
    }

    private enum ValidationState //Finite-state machine，启动！
    {
        Success,                   
        AudioFailedLoading,             
        SharedCooldown,           
        IndividualCooldown,         
        DisabledByConfig,          
        ClipNotFound,
        ValidationError               
    }
}
