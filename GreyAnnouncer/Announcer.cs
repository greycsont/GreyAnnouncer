using System.IO;
using System.Collections;
using System.Collections.Generic; //audioclip
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.Networking;
using System;

namespace greycsont.GreyAnnouncer
{
    
    public class Announcer{       
        private static Dictionary<int, AudioClip> audioClips = new Dictionary<int, AudioClip>();
        public static List<ConfigEntry<bool>> EnabledStyleConfigs = new List<ConfigEntry<bool>>();
        private static readonly string[] rankAudioNames = { "D", "C", "B", "A", "S", "SS", "SSS", "U"};
        private static float[] individualRankPlayCooldown = {0f,0f,0f,0f,0f,0f,0f,0f};
        private static float sharedRankPlayCooldown = 0f;  // Timer
        private static readonly HashSet<string> audioFailedLoading = new();
        private static AudioSource globalAudioSource;
        private static AudioSource localAudioSource;

        /// <summary>
        /// Initialize audio file
        /// </summary>
        public static void Initialize(){
            AddStyleConfigEntryToList();
            FindAvailableAudio();
            GetLocalAudioSource();
        }


        public static void ReloadAudio(){
            Plugin.Log.LogInfo($"Reload audio...");
            FindAvailableAudio();
        }


        public static void AddAudioLowPassFilter(){
            localAudioSource = AudioSourceManager.AddLowPassFilter(localAudioSource);
        }
        public static void RemoveAudioLowPassFilter(){
            localAudioSource = AudioSourceManager.RemoveLowPassFilter(localAudioSource);
        }

        private static void GetLocalAudioSource(){
            if (localAudioSource == null){
                localAudioSource = GetGlobalAudioSource();
            }
            localAudioSource.spatialBlend = 0f;
            localAudioSource.priority = 0;
            localAudioSource.volume = InstanceConfig.AudioSourceVolume.Value;
        }


        private static void FindAvailableAudio(){
            string audioPath = PathManager.GetCurrentPluginPath("audio");
            audioFailedLoading.Clear();
            if (!Directory.Exists(audioPath))
            {
                Plugin.Log.LogError($"audio directory not found: {audioPath}");
                Directory.CreateDirectory(audioPath);
                return;
            }

            for (int i = 0; i < rankAudioNames.Length; i++)
            {
                string fullPath = Path.Combine(audioPath, rankAudioNames[i] + ".wav");

                if (File.Exists(fullPath)){
                    // Using a helper MonoBehaviour to start a coroutine to load audio
                    CoroutineRunner.Instance.StartCoroutine(LoadAudioClip(fullPath, i));
                }
                else {
                    audioFailedLoading.Add(rankAudioNames[i]);
                    continue;
                }
            }

            if (audioFailedLoading.Count == 0){
                Plugin.Log.LogInfo("All audios succeesfully loaded");
            }
            if (audioFailedLoading.Count > 0){
                Plugin.Log.LogWarning("Failed to load audio files: " + string.Join(", ", audioFailedLoading));
            }
        }


        private static IEnumerator LoadAudioClip(string path, int key){
            string url = new Uri(path).AbsoluteUri;
            //string url = "file://" + path;
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.WAV))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Plugin.Log.LogError($"Failed to Load audio : {key}, Error message : {www.error}");
                }
                else
                {
                    audioClips[key] = DownloadHandlerAudioClip.GetContent(www);;
                }
            }
        }

        
        public static void PlaySound(int rank){
            AudioClip clip = CheckPlayValidation(rank);
            if (clip == null) return;
            localAudioSource.clip = clip;
            localAudioSource.volume = 1.0f;
            localAudioSource.Play();

            CoroutineRunner.Instance.StartCoroutine(CooldownCoroutine(value => sharedRankPlayCooldown = value, InstanceConfig.SharedRankPlayCooldown.Value));
            CoroutineRunner.Instance.StartCoroutine(CooldownCoroutine(value => individualRankPlayCooldown[rank] = value, InstanceConfig.IndividualRankPlayCooldown.Value));
        }

        private static AudioClip CheckPlayValidation(int rank)
        {
            ValidationState state = GetValidationState(rank);
            if (state != ValidationState.Success)
            {
                Plugin.Log.LogInfo($"Skip {rankAudioNames[rank]} for {state}");
                return null;
            }

           return audioClips.TryGetValue(rank, out AudioClip clip) ? clip : null; 
        }

        private static ValidationState GetValidationState(int rank)
        {
            if (audioFailedLoading.Contains(rankAudioNames[rank])) 
                return ValidationState.AudioFailedLoading;

            if (sharedRankPlayCooldown > 0f) 
                return ValidationState.SharedCooldown;

            if (individualRankPlayCooldown[rank] > 0f) 
                return ValidationState.IndividualCooldown;

            if (!EnabledStyleConfigs[rank].Value) 
                return ValidationState.DisabledByConfig;

            if (!audioClips.TryGetValue(rank, out _)) 
                return ValidationState.ClipNotFound;

            return ValidationState.Success;
        }


        private static AudioSource GetGlobalAudioSource()
        {
            if (globalAudioSource == null)
            {
                GameObject audioObj = GameObject.Find("GlobalAudioPlayer") ?? new GameObject("GlobalAudioPlayer");
                globalAudioSource = audioObj.GetComponent<AudioSource>() ?? audioObj.AddComponent<AudioSource>();
                GameObject.DontDestroyOnLoad(audioObj);
            }
            return globalAudioSource;
        }

        public static void ResetTimerToZero(){
            sharedRankPlayCooldown = 0f;
            for (int i = 0; i < individualRankPlayCooldown.Length; i++)
            {
                individualRankPlayCooldown[i] = 0f;
            }
        }


        private static IEnumerator CooldownCoroutine(Action<float> setCooldown, float initialCooldown)
        {
            if (initialCooldown <= 0){
                setCooldown(0);
                yield break;
            }
            float cooldown = initialCooldown;
            setCooldown(cooldown);  //delegate

            while (cooldown > 0){
                cooldown = Math.Max(0, cooldown - Time.deltaTime);
                setCooldown(cooldown);
                yield return null;
            }
        }


        
        private static void AddStyleConfigEntryToList(){
            EnabledStyleConfigs.Clear();
            EnabledStyleConfigs.Add(InstanceConfig.RankD_Enabled);
            EnabledStyleConfigs.Add(InstanceConfig.RankC_Enabled);
            EnabledStyleConfigs.Add(InstanceConfig.RankB_Enabled);
            EnabledStyleConfigs.Add(InstanceConfig.RankA_Enabled);
            EnabledStyleConfigs.Add(InstanceConfig.RankS_Enabled);
            EnabledStyleConfigs.Add(InstanceConfig.RankSS_Enabled);
            EnabledStyleConfigs.Add(InstanceConfig.RankSSS_Enabled);
            EnabledStyleConfigs.Add(InstanceConfig.RankU_Enabled);
        }

        private enum ValidationState //Finite-state machine，启动！
        {
            Success,                   
            AudioFailedLoading,             
            SharedCooldown,           
            IndividualCooldown,         
            DisabledByConfig,          
            ClipNotFound                
        }


    }
}