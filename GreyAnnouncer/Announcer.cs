using System.IO;
using System.Collections;
using System.Collections.Generic; //audio clip
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Runtime.CompilerServices;

/* Announcer.cs requires :
            PathManager.cs to find and fetch audio
            AudioSourceManager.cs to add LowPassFilter
            CoroutineRunner.cs for set timer
            InstanceConfig.cs for setting
            JsonSetting.cs for setting */
namespace greycsont.GreyAnnouncer
{
    public class Announcer{       
        private static Dictionary<int, AudioClip> audioClips                 = new Dictionary<int, AudioClip>();
        private static readonly HashSet<string>   RankFailedLoading          = new();
        private static readonly string[]          SupportedExtensions        = new string[] { ".wav", ".mp3", ".ogg", ".aiff", ".aif" };
        private static readonly string[]          RankNames                  = new string[] {"D", "C", "B", "A", "S", "SS", "SSS", "U"};
        private static float[]                    individualRankPlayCooldown = {0f,0f,0f,0f,0f,0f,0f,0f};
        private static float                      sharedRankPlayCooldown     = 0f;  // Timer
        private static AudioSource                globalAudioSource;
        private static AudioSource                localAudioSource;

        /// <summary>
        /// Initialize audio file
        /// </summary>
        public static void Initialize()
        {
            FindAvailableAudio(PathManager.GetGamePath(Path.Combine("ULTRAKILL_DATA","Audio")));
        }

        public static void PlaySound(int rank)
        {
            /* Parry balls of Maurice -> Hit Maurice -> AscendingRank() -> Postfix() -> PlaySound() -> CheckPlayValidation() */
            /* This bug will skip all the function before CheckPlayValidation(),  The try-catch has implemented in the fucntion*/
            AudioClip clip = TryToGetAudioClip(rank);
            if (clip == null) return;
            if (localAudioSource == null) GetLocalAudioSource();
            
            localAudioSource.clip   = clip;
            localAudioSource.volume = InstanceConfig.AudioSourceVolume.Value < 1f ? InstanceConfig.AudioSourceVolume.Value : 1f;
            localAudioSource.Play();

            CoroutineRunner.Instance.StartCoroutine(CooldownCoroutine(value => sharedRankPlayCooldown           = value, InstanceConfig.SharedRankPlayCooldown.Value));
            CoroutineRunner.Instance.StartCoroutine(CooldownCoroutine(value => individualRankPlayCooldown[rank] = value, InstanceConfig.IndividualRankPlayCooldown.Value));

        }

        public static void ReloadAudio()
        {
            Plugin.Log.LogInfo($"Reload audio...");
            FindAvailableAudio(PathManager.GetGamePath(Path.Combine("ULTRAKILL_DATA","Audio")));
        }


        public static void AddAudioLowPassFilter()
        {
            if (localAudioSource == null) GetLocalAudioSource();
            localAudioSource = AudioSourceManager.AddLowPassFilter(localAudioSource);
        }
        public static void RemoveAudioLowPassFilter()
        {
            if (localAudioSource == null) GetLocalAudioSource();
            localAudioSource = AudioSourceManager.RemoveLowPassFilter(localAudioSource);
        }

        private static void GetLocalAudioSource()
        {
            localAudioSource              ??= GetGlobalAudioSource();
            localAudioSource.spatialBlend   = 0f; // 2D
            localAudioSource.priority       = 0; // Make sure you can hear the announce
        }

        private static AudioSource GetGlobalAudioSource()
        {
            if (globalAudioSource == null)
            {
                GameObject audioObj = GameObject.Find("GlobalAudioPlayer") ?? new GameObject("GlobalAudioPlayer");
                globalAudioSource   = audioObj.GetComponent<AudioSource>() ?? audioObj.AddComponent<AudioSource>();
                GameObject.DontDestroyOnLoad(audioObj);
            }
            return globalAudioSource;
        }

        private static int findAvailableAudioRecursive = 0;
        private static void FindAvailableAudio(string audioPath)
        {
            if (findAvailableAudioRecursive >= 2 )
            {
                findAvailableAudioRecursive = 0;
                Plugin.Log.LogError($"Failed to find audio from \n New directory : {PathManager.GetGamePath(Path.Combine("ULTRAKILL_DATA","Audio"))}\n Legacy directory : {PathManager.GetCurrentPluginPath("audio")}");
                return;
            }
            findAvailableAudioRecursive++;

            RankFailedLoading.Clear();
            TryToFindDirectoryOfAudioFolder(audioPath);
            TryToFetchAudios(audioPath);
            LoggingAudioFailedLoading();
            
            if (RankFailedLoading.SetEquals(RankNames))
            {  // array compare to hashset
                Plugin.Log.LogWarning($"No audio files found in the directory : {audioPath}.");
                FindAvailableAudio(PathManager.GetCurrentPluginPath("audio"));
            }
            else
            {
                findAvailableAudioRecursive = 0;
            }
        }
        private static void TryToFindDirectoryOfAudioFolder(string audioPath)
        {
            if (!Directory.Exists(audioPath))
            {
                Plugin.Log.LogWarning($"audio directory not found : {audioPath}");
                Directory.CreateDirectory(audioPath);
                return;
            }
        }
        private static void TryToFetchAudios(string audioPath)
        {

            for (int i = 0; i < RankNames.Length; i++)
            {
                if (JsonSetting.Settings.RankSettings.audioNames[i] == "")
                    Plugin.Log.LogWarning ($"You forget to set the audio name of rank {RankNames[i]} ");

                string filePath = null;
                foreach (var ext in SupportedExtensions)
                {

                    string potentialPath = Path.Combine(audioPath, JsonSetting.Settings.RankSettings.audioNames[i] + ext);
                    if (File.Exists(potentialPath))
                    {
                        filePath = potentialPath;
                        break;
                    }
                }

                if (File.Exists(filePath))
                {
                    // Using a helper MonoBehaviour to start a coroutine to load audio
                    CoroutineRunner.Instance.StartCoroutine(LoadAudioClip(filePath, i));
                }
                else 
                {
                    RankFailedLoading.Add(RankNames[i]);
                }
            }
        }
        private static void LoggingAudioFailedLoading()
        {
            if (RankFailedLoading.Count == 0)
            {
                Plugin.Log.LogInfo   ("All audios successfully loaded");
            }
            else
            {
                Plugin.Log.LogWarning("Failed to load audio files: " + string.Join(", ", RankFailedLoading));
            }
        }

        private static IEnumerator LoadAudioClip(string path, int key)
        {
            string url = new Uri(path).AbsoluteUri;
            //string url = "file://" + path;
            AudioType audioType = GetAudioTypeFromExtension(url);
            Plugin.Log.LogInfo($"Loading audio : {JsonSetting.Settings.RankSettings.audioNames[key]} for Rank : {RankNames[key]} from {Uri.UnescapeDataString(url)}");
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, audioType))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success) 
                    Plugin.Log.LogError($"Failed to Load audio : {key}, Error message : {www.error}");
                else 
                    audioClips[key] = DownloadHandlerAudioClip.GetContent(www);
            }
        }

        private static AudioType GetAudioTypeFromExtension(string path)
        {
            string extension = Path.GetExtension(path).ToLower();
            switch (extension) 
            {
                case ".wav": return AudioType.WAV;
                case ".mp3": return AudioType.MPEG;
                case ".ogg": return AudioType.OGGVORBIS;
                case ".aiff": case ".aif": return AudioType.AIFF;
                default:
                    Plugin.Log.LogWarning($"Unsupported audio format: {extension}, defaulting to WAV");
                    return AudioType.WAV;
            }
        }

        
        private static AudioClip TryToGetAudioClip(int rank)
        {
            ValidationState state = GetPlayValidationState(rank);
            if (state != ValidationState.Success)
            {
                Plugin.Log.LogInfo($"Skip {JsonSetting.Settings.RankSettings.audioNames[rank]} of rank {RankNames[rank]} for {state}");
                return null;
            }

           return audioClips.TryGetValue(rank, out AudioClip clip) ? clip : null; 
        }

        public static void ResetTimerToZero()
        {
            sharedRankPlayCooldown = 0f;
            Array.Clear(individualRankPlayCooldown, 0, individualRankPlayCooldown.Length);
        }

        public static void UpdateAudioSourceVolume(float targetVolume, float duration = 0.35f)
        {
             CoroutineRunner.Instance.StartCoroutine(AudioSourceManager.FadeVolume(localAudioSource, targetVolume, duration));
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
            if (rank < 0 || rank > RankNames.Length - 1)
                return ValidationState.InvaildRankIndex;

            if (individualRankPlayCooldown[rank] > 0f) 
                return ValidationState.IndividualCooldown;

            if (RankFailedLoading.Contains(RankNames[rank])) 
                return ValidationState.AudioFailedLoading;

            if (sharedRankPlayCooldown > 0f) 
                return ValidationState.SharedCooldown;    

            if (!InstanceConfig.RankToggleDict[RankNames[rank]].Value) 
                return ValidationState.DisabledByConfig;

            if (!audioClips.TryGetValue(rank, out _)) 
                return ValidationState.ClipNotFound;

            return ValidationState.Success;
        }

        [Obsolete("Debug uses only")]
        private static ValidationState GetPlayValidationState(int rank, int Debug)
        {
            List<ValidationState> valiationStateList = new List<ValidationState>();
            try
            {
                foreach (var rule in validationRules)
                {
                    var result = rule(rank);
                    if (result.HasValue) valiationStateList.Add(result.Value);
                }
                if (valiationStateList != null)
                {
                    foreach (var state in valiationStateList)
                    {
                        Plugin.Log.LogInfo($"failedValiationState: {state}");
                    }
                    return valiationStateList[0];
                }
                return ValidationState.Success;
            }
            catch
            {
                return ValidationState.ValidationError;
            }    
        }

        private static readonly List<Func<int, ValidationState?>> validationRules = new()
        {
            rank => (rank < 0 || rank >= RankNames.Length) ?                ValidationState.InvaildRankIndex :   null,
            rank => RankFailedLoading.Contains(RankNames[rank]) ?           ValidationState.AudioFailedLoading : null,
            rank => sharedRankPlayCooldown > 0f ?                           ValidationState.SharedCooldown :     null,
            rank => individualRankPlayCooldown[rank] > 0f ?                 ValidationState.IndividualCooldown : null,
            rank => !InstanceConfig.RankToggleDict[RankNames[rank]].Value ? ValidationState.DisabledByConfig :   null,
            rank => !audioClips.ContainsKey(rank) ?                         ValidationState.ClipNotFound :       null  
        };

        private enum ValidationState //Finite-state machine，启动！
        {
            Success,                   
            AudioFailedLoading,             
            SharedCooldown,           
            IndividualCooldown,         
            DisabledByConfig,          
            ClipNotFound,
            InvaildRankIndex,
            ValidationError               
        }

    }
}