using System.Collections;
using System.Collections.Generic; //audioclip
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

namespace greycsont.GreyAnnouncer
{
    
    public class Announcer{       
        private static Dictionary<int, AudioClip> audioClips = new Dictionary<int, AudioClip>();
        private static readonly string[] audioNames = { "D", "C", "B", "A", "S", "SS", "SSS", "U"};
        private static float playCooldown = 0f;  // time counter
        private static float cooldownDuration = 3f; // 3 sec cool down

        /// <summary>
        /// Initialize audio file
        /// </summary>
        public static void Initialize(){
            string pluginDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string audioPath = Path.Combine(pluginDirectory, "Audio");

            if (!Directory.Exists(audioPath)){
                Debug.LogError($"audio directory not found: {audioPath}");
                return;
            }

            for (int i = 0; i < audioNames.Length; i++)
            {
                string audioName = audioNames[i];
                string fullPath = Path.Combine(audioPath, audioName + ".wav");
                if (File.Exists(fullPath))
                {
                    // 使用辅助 MonoBehaviour 启动协程加载音频
                    CoroutineRunner.Instance.StartCoroutine(LoadAudioClip(fullPath, i));
                }
                if (!File.Exists(fullPath)) {
                    Debug.LogWarning($"Skip non exist audio file : {fullPath}");
                    continue; // No longer trying to loading
                }
            }

        }

       private static IEnumerator LoadAudioClip(string path, int key){
            string url = "file://" + path;
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.WAV))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"加载音频失败：{key}，错误信息：{www.error}");
                }
                else
                {
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                    audioClips[key] = clip;
                    Debug.Log($"加载成功：{key}, 长度：{clip.length}");
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
            ///Debug.Log("Audio dictionary data:");
            ///foreach (var entry in audioClips)
            ///{
            ///    Debug.Log($"Key: {entry.Key}, AudioClip: {entry.Value}");
            ///}

            

            if (audioClips.TryGetValue(rank, out AudioClip clip)){
                if (playCooldown > 0f) {
                    Debug.Log($"冷却中，剩余时间: {playCooldown:F2} 秒，跳过播放音频 {rank}");
                    return; // 冷却未结束，不播放
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

                playCooldown = cooldownDuration; // 设置倒计时
                CoroutineRunner.Instance.StartCoroutine(CooldownTimer());
            }
            else{
                Debug.LogError($"未找到音频: {rank}");
            }
        }
    }
}