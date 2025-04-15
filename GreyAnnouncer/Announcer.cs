using System.IO;
using System.Collections;
using System.Collections.Generic; //audioclip
using UnityEngine;
using UnityEngine.Networking;
using System;

/* Announcer.cs requires :
            PathManager.cs to find and fetch audio
            AudioSourceManager.cs to add LowPassFilter
            CoroutineRunner.cs for set timer
            InstanceConfig.cs for setting
            JsonSetting.cs for setting */
namespace greycsont.GreyAnnouncer
{
    public class Announcer{       
        private static Dictionary<int, AudioClip> audioClips = new Dictionary<int, AudioClip>();
        private static float[] individualRankPlayCooldown = {0f,0f,0f,0f,0f,0f,0f,0f};
        private static float sharedRankPlayCooldown = 0f;  // Timer
        private static readonly HashSet<string> audioFailedLoading = new();
        private static AudioSource globalAudioSource;
        private static AudioSource localAudioSource;

        /// <summary>
        /// Initialize audio file
        /// </summary>
        public static void Initialize(){
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
            localAudioSource ??= GetGlobalAudioSource();
            localAudioSource.spatialBlend = 0f; // 2D
            localAudioSource.priority = 0; // Make sure you can hear the announce
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
                Plugin.Log.LogError($"Failed to find audio from \n New directory : {PathManager.GetGamePath(Path.Combine("ULTRAKILL_DATA","Audio"))}\n Legacy directory : {PathManager.GetCurrentPluginPath("audio")}");
                return;
            }
            limitOfRecursive++;

            audioFailedLoading.Clear();
            TryToFindDirectoryOfAudioFolder(audioPath);
            TryToFetchAudios(audioPath);
            LoggingAudioFailedLoading();
            
            if (audioFailedLoading.SetEquals(JsonSetting.Settings.RankSettings.ranks)){  // array compare to hashset
                Plugin.Log.LogWarning($"No audio files found in the directory : {audioPath}.");
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
            string[] supportedExtensions = new string[] { ".wav", ".mp3", ".ogg", ".aiff", ".aif" };
            for (int i = 0; i < JsonSetting.Settings.RankSettings.ranks.Length; i++)
            {
                string filePath = null;
                foreach (var ext in supportedExtensions)
                {
                    string potentialPath = Path.Combine(audioPath, JsonSetting.Settings.RankSettings.audioNames[i] + ext);
                    if (File.Exists(potentialPath))
                    {
                        filePath = potentialPath;
                        break;
                    }
                }
                if (File.Exists(filePath)){
                    // Using a helper MonoBehaviour to start a coroutine to load audio
                    CoroutineRunner.Instance.StartCoroutine(LoadAudioClip(filePath, i));
                }
                else {
                    audioFailedLoading.Add(JsonSetting.Settings.RankSettings.ranks[i]);
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
            Plugin.Log.LogInfo($"Loading audio : {JsonSetting.Settings.RankSettings.audioNames[key]} for Rank : {JsonSetting.Settings.RankSettings.ranks[key]} from {url}");
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
            /* I don't like to use the try-catch, but when I testing the mod, there's a bug I meet :
               Parry balls of Maurice -> Hit Maurice -> AscendingRank() -> Postfix() -> PlaySound() -> CheckPlayValidation() 
               There's some conflict of pointer in the ValidationState.DisabledByConfig ("D" -- "Rank_D")
               And it gives a nullPointerException and skiped the explosion on the maurice
               So that's why I uses a try-catch in here*/
            try {
                AudioClip clip = CheckPlayValidation(rank);
                if (clip == null) return;
                if (localAudioSource == null) GetLocalAudioSource();
                localAudioSource.clip = clip;
                localAudioSource.volume = InstanceConfig.AudioSourceVolume.Value < 1f ? InstanceConfig.AudioSourceVolume.Value : 1f;
                localAudioSource.Play();

                CoroutineRunner.Instance.StartCoroutine(CooldownCoroutine(value => sharedRankPlayCooldown = value, InstanceConfig.SharedRankPlayCooldown.Value));
                CoroutineRunner.Instance.StartCoroutine(CooldownCoroutine(value => individualRankPlayCooldown[rank] = value, InstanceConfig.IndividualRankPlayCooldown.Value));
            }catch (Exception ex){
                Plugin.Log.LogError($"Error in PlaySound() : {ex}");
            }    
        }

        private static AudioClip CheckPlayValidation(int rank)
        {
            ValidationState state = GetPlayValidationState(rank);
            if (state != ValidationState.Success)
            {
                Plugin.Log.LogInfo($"Skip {JsonSetting.Settings.RankSettings.audioNames[rank]} for {state}");
                return null;
            }

           return audioClips.TryGetValue(rank, out AudioClip clip) ? clip : null; 
        }

        private static ValidationState GetPlayValidationState(int rank)
        {
            if (rank < 0 || rank > JsonSetting.Settings.RankSettings.ranks.Length - 1)   // To compatible with another mods, 0 ~ 7, maybe support to add more ranks
                return ValidationState.InvaildRankIndex;

            if (audioFailedLoading.Contains(JsonSetting.Settings.RankSettings.ranks[rank])) 
                return ValidationState.AudioFailedLoading;

            if (sharedRankPlayCooldown > 0f) 
                return ValidationState.SharedCooldown;

            if (individualRankPlayCooldown[rank] > 0f) 
                return ValidationState.IndividualCooldown;

            if (!InstanceConfig.RankToggleDict[JsonSetting.Settings.RankSettings.ranks[rank]].Value) 
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

        public static void UpdateAudioSourceVolume(float targetVolume, float duration = 0.35f){
             CoroutineRunner.Instance.StartCoroutine(AudioSourceManager.FadeVolume(localAudioSource, targetVolume, duration));
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