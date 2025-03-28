using System.IO;
using System.Collections;
using System.Collections.Generic; //audioclip
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Linq;

/* Announcer.cs requires :
            PathManager.cs to find and fetch audio
            AudioSourceManager.cs to add LowPassFilter
            CoroutineRunner.cs for set timer
            InstanceConfig for setting */
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
            FindAvailableAudio(PathManager.GetGamePath(Path.Combine("ULTRAKILL_DATA","Audio")));
        }


        public static void ReloadAudio(){
            Plugin.Log.LogInfo($"Reload audio...");
            FindAvailableAudio(PathManager.GetGamePath(Path.Combine("ULTRAKILL_DATA","Audio")));
        }


        public static void AddAudioLowPassFilter(){
            if (localAudioSource == null) GetLocalAudioSource();
            localAudioSource = AudioSourceManager.AddLowPassFilter(localAudioSource);
        }
        public static void RemoveAudioLowPassFilter(){
            if (localAudioSource == null) GetLocalAudioSource();
            localAudioSource = AudioSourceManager.RemoveLowPassFilter(localAudioSource);
        }

        private static void GetLocalAudioSource(){
            if (localAudioSource == null){
                localAudioSource = GetGlobalAudioSource();
            }
            localAudioSource.spatialBlend = 0f;
            localAudioSource.priority = 0;
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

        private static int limitOfRecursive = 0;
        private static void FindAvailableAudio(string audioPath){
            if (limitOfRecursive >= 2 ){
                limitOfRecursive = 0;
                return;
            }
            limitOfRecursive++;
            audioFailedLoading.Clear();
            TryToFindDirectoryOfAudioFolder(audioPath);
            TryToFetchAudios(audioPath);
            LoggingAudioFailedLoading();
            if (audioFailedLoading.SetEquals(rankAudioNames)){  // array compare to hashset
                Plugin.Log.LogWarning($"No audio files found in the directory : {audioPath}. Start to search the legacy folder which is near the plugin/dll.");
                FindAvailableAudio(PathManager.GetCurrentPluginPath("audio"));
            }
        }
        private static void TryToFindDirectoryOfAudioFolder(string audioPath){
            if (!Directory.Exists(audioPath))
            {
                Plugin.Log.LogWarning($"audio directory not found : {audioPath}");
                Directory.CreateDirectory(audioPath);
                return;
            }
        }
        private static void TryToFetchAudios(string audioPath){
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
        }
        private static void LoggingAudioFailedLoading(){
            if (audioFailedLoading.Count == 0){
                Plugin.Log.LogInfo("All audios succeesfully loaded");
            }else{
                Plugin.Log.LogWarning("Failed to load audio files: " + string.Join(", ", audioFailedLoading));
            }
        }

        private static IEnumerator LoadAudioClip(string path, int key){
            string url = new Uri(path).AbsoluteUri;
            //string url = "file://" + path;
            AudioType audioType = GetAudioTypeFromExtension(url);
            Plugin.Log.LogInfo($"Loading audio : {rankAudioNames[key]} from {url} with audioType {audioType}");
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, audioType))
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

        private static AudioType GetAudioTypeFromExtension(string path) {
            string extension = Path.GetExtension(path).ToLower();
            switch (extension) {
                case ".wav": return AudioType.WAV;
                case ".mp3": return AudioType.MPEG;
                case ".ogg": return AudioType.OGGVORBIS;
                case ".aiff": case ".aif": return AudioType.AIFF;
                default:
                    Plugin.Log.LogWarning($"Unsupported audio format: {extension}, defaulting to WAV");
                    return AudioType.WAV;
    }
        }

        
        public static void PlaySound(int rank){
            AudioClip clip = CheckPlayValidation(rank);
            if (clip == null) return;
            if (localAudioSource == null) GetLocalAudioSource();
            localAudioSource.clip = clip;
            localAudioSource.volume = InstanceConfig.AudioSourceVolume.Value < 1f ? InstanceConfig.AudioSourceVolume.Value : 1f;
            localAudioSource.Play();

            CoroutineRunner.Instance.StartCoroutine(CooldownCoroutine(value => sharedRankPlayCooldown = value, InstanceConfig.SharedRankPlayCooldown.Value));
            CoroutineRunner.Instance.StartCoroutine(CooldownCoroutine(value => individualRankPlayCooldown[rank] = value, InstanceConfig.IndividualRankPlayCooldown.Value));
        }

        private static AudioClip CheckPlayValidation(int rank)
        {
            ValidationState state = GetPlayValidationState(rank);
            if (state != ValidationState.Success)
            {
                Plugin.Log.LogInfo($"Skip {rankAudioNames[rank]} for {state}");
                return null;
            }

           return audioClips.TryGetValue(rank, out AudioClip clip) ? clip : null; 
        }

        private static ValidationState GetPlayValidationState(int rank)
        {
            if (rank < 0 || rank > 7)   // To compatible with another mods, 0 ~ 7, maybe support to add more ranks
                return ValidationState.InvaildRankIndex;

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
            float updateInterval = (cooldown < 0.5f) ? cooldown / 3f : cooldown / 10f;
            setCooldown(cooldown);  //delegate

            while (cooldown > 0){
                yield return new WaitForSeconds(updateInterval);
                cooldown = Math.Max(0, cooldown - updateInterval);
                setCooldown(cooldown);
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
            ClipNotFound,
            InvaildRankIndex                
        }


    }
}