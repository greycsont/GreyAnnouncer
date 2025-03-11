using System.IO;
using System.Collections;
using System.Collections.Generic; //audioclip
using UnityEngine;
using UnityEngine.Networking;
using System;

namespace greycsont.GreyAnnouncer
{
    
    public class Announcer{       
        private static Dictionary<int, AudioClip> audioClips = new Dictionary<int, AudioClip>();
        private static readonly string[] audioNames = { "D", "C", "B", "A", "S", "SS", "SSS", "U"};
        private static float playCooldown = 0f;  // Timer
        private static float cooldownDuration;
        private static readonly HashSet<string> audioFailedLoading = new();
        private static AudioSource globalAudioSource;
        private static Plugin plugin;

        /// <summary>
        /// Initialize audio file
        /// </summary>
        public static void Initialize(Plugin __plugin){
            plugin = __plugin;

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
                
            AudioSource audioSource = GetGlobalAudioSource();
            audioSource.clip = clip;
            audioSource.volume = 1.0f;
            audioSource.spatialBlend = 0f;
            audioSource.priority = 0;
            audioSource.Play();

            // supports in-game configuration
            cooldownDuration = Math.Max(0,plugin.Config.Bind("General", "CooldownDuration", 0.75f, "Cooldown time for the announcer (seconds)").Value);
            playCooldown = cooldownDuration; // Reset timer
            CoroutineRunner.Instance.StartCoroutine(CooldownTimer());
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
                playCooldown -= Time.deltaTime;
                yield return null;
            }
            playCooldown = 0f;
        }
    }

    
}