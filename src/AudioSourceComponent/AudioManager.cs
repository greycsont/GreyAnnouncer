using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    // <summary> 存储单个音频的信息 </summary>
    public class Sound
    {
        [Header("音频名称")]
        public string name;
        [Header("音频分组")]
        public AudioMixerGroup outputMixerGroup;
        public AudioClip clip;
        public float volume = 1f;
        public float pitch = 1f;
        public bool playOnAwake = false;
        public bool loop = false;
    }
    // <summary> 
    // 所有的音频信息
    // </summary>
    public List<Sound> sounds;
    // <summary> 
    // 音频对应的音频组件 
    // </summary>
    private Dictionary<string, AudioSource> audioSourcesDic;

    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        audioSourcesDic = new Dictionary<string, AudioSource>();
    }
    private void Start()
    {
        
    }
}