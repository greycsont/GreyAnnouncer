using System.IO;
using System.Collections;
using System.Collections.Generic; //audioclip
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.Networking;
using System;
using GameConsole.pcon;

namespace greycsont.GreyAnnouncer
{
    
    public class Announcer{       
        private static Dictionary<int, AudioClip> audioClips = new Dictionary<int, AudioClip>();
        public static List<ConfigEntry<bool>> EnabledStyleConfigs = new List<ConfigEntry<bool>>();
        private static readonly string[] audioNames = { "D", "C", "B", "A", "S", "SS", "SSS", "U"};
        private static float playCooldown = 0f;  // Timer
        private static readonly HashSet<string> audioFailedLoading = new();
        private static AudioSource globalAudioSource;

        /// <summary>
        /// Initialize audio file
        /// </summary>
        public static void Initialize(){
            AddStyleConfigEntryToList();
            FindAvailableAudio();
        }

        public static void ReloadAudio(){
            Plugin.Log.LogInfo($"Reload audio...");
            FindAvailableAudio();
        }

        private static void FindAvailableAudio(){
            string audioPath = PathManager.GetCurrentPluginPath("audio");
            audioFailedLoading.Clear();
            if (!Directory.Exists(audioPath)){
                Plugin.Log.LogError($"audio directory not found: {audioPath}");
                Directory.CreateDirectory(audioPath);
                return;
            }

            for (int i = 0; i < audioNames.Length; i++)
            {
                string fullPath = Path.Combine(audioPath, audioNames[i] + ".wav");

                if (File.Exists(fullPath))
                {
                    // Using a helper MonoBehaviour to start a coroutine to load audio
                    CoroutineRunner.Instance.StartCoroutine(LoadAudioClip(fullPath, i));
                }
                else {
                    audioFailedLoading.Add(audioNames[i]);
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
                    Plugin.Log.LogError($"Failed to Load audio ：{key}，Error message ：{www.error}");
                }
                else
                {
                    audioClips[key] = DownloadHandlerAudioClip.GetContent(www);;
                }
            }
        }

        public static void PlaySound(int rank){
            if (audioFailedLoading.Contains(audioNames[rank])) return;  // Skip if failed loading
            if (playCooldown > 0f) return; // Skip if still in cooldown
            if (!audioClips.TryGetValue(rank, out AudioClip clip)) return;
            if (EnabledStyleConfigs[rank].Value == false) return;
                
            AudioSource audioSource = GetGlobalAudioSource();
            audioSource.clip = clip;
            audioSource.volume = 1.0f;
            audioSource.spatialBlend = 0f;
            audioSource.priority = 0;
            audioSource.Play();

            // supports in-game configuration
            playCooldown = Math.Max(0,InstanceConfig.AnnounceCooldown.Value);  //Reset timer
            CoroutineRunner.Instance.StartCoroutine(CooldownTimer());
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

        private static IEnumerator CooldownTimer()
        {
            while (playCooldown > 0)
            {
                playCooldown = Math.Max(0, playCooldown - Time.deltaTime);
                yield return null;
            }
        }
        public static void ResetTimerToZero(){
            playCooldown = 0;
        }
    }

    
}