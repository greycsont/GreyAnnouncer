using System.Collections;
using System.Collections.Generic; //audioclip
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace greycsont.GreyAnnouncer
{
    
    public class Announcer{       
        private static Dictionary<int, AudioClip> audioClips = new Dictionary<int, AudioClip>();
        private static readonly string[] audioNames = { "D", "C", "B", "A", "S", "SS", "SSS", "U"};
        private static float playCooldown = 0f;  // countdown
        private static float cooldownDuration = 0.5f; // cooldown

        /// <summary>
        /// Initialize audio file
        /// </summary>
        public static void Initialize(){
            string audioPath = PathManager.GetCurrentPluginPath("audio");

            var analyzer = new ConfiginiAnalyzer("config.ini");
            cooldownDuration = analyzer.GetCooldownDuration();
            DebugLogger.Log($"Cooldown: {cooldownDuration} seconds");  
                  
            if (!Directory.Exists(audioPath)){
                Debug.LogError($"audio directory not found: {audioPath}");
                Directory.CreateDirectory(audioPath);
                return;
            }

            List<string> audioFailedLoading = new List<string>();
            for (int i = 0; i < audioNames.Length; i++)
            {
                string audioName = audioNames[i];
                
                string fullPath = Path.Combine(audioPath, audioName + ".wav");
                if (File.Exists(fullPath))
                {
                    // Using a helper MonoBehaviour to start a coroutine to load audio
                    CoroutineRunner.Instance.StartCoroutine(LoadAudioClip(fullPath, i));
                }
                if (!File.Exists(fullPath)) {
                    audioFailedLoading[i] += audioName;
                    continue;
                }
            }

            if (audioFailedLoading.Count == 0){
                Debug.Log("All audios succeesfully loaded");
            }
            if (audioFailedLoading.Count > 0){
                Debug.LogWarning("Failed to load audio files: " + string.Join(", ", audioFailedLoading));
            }
        }

       private static IEnumerator LoadAudioClip(string path, int key){
            string url = "file://" + path;
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.WAV))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Failed to Load audio ：{key}，Error message ：{www.error}");
                }
                else
                {
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                    audioClips[key] = clip;
                }
            }
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


        public static void PlaySound(int rank){
            if (audioClips.TryGetValue(rank, out AudioClip clip)){
                if (playCooldown > 0f) {
                    return; // interupt play sound
                }

                GameObject audioObj = GameObject.Find("GlobalAudioPlayer");
                if (audioObj == null){
                    audioObj = new GameObject("GlobalAudioPlayer");
                    audioObj.AddComponent<AudioSource>();
                    GameObject.DontDestroyOnLoad(audioObj);
                }

                AudioSource audioSource = audioObj.GetComponent<AudioSource>();
                audioSource.clip = clip;
                audioSource.volume = 1.0f;
                audioSource.spatialBlend = 0f;
                audioSource.priority = 0;
                audioSource.Play();
                DebugLogger.Log($"Play sound : {rank}");

                playCooldown = cooldownDuration; // set countdown
                CoroutineRunner.Instance.StartCoroutine(CooldownTimer());
            }
            else{
                Debug.LogError($"audio not found : {rank}");
            }
        }
    }
}