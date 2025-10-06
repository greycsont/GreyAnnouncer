/* 
 * 使用了和线程池/Thread Pool类似的概念
 * 后面发现有PlayOneShot这个函数
 * 但具体性能我觉得还是这个舒服
 * 
 * https://discussions.unity.com/t/playoneshot-performance/595405
 *
 * 爆了，直接使用playOneShot()比搞个池子性能高一倍
 */




using System;
using System.Collections;
using System.Collections.Generic; //audio clip
using UnityEngine;



namespace GreyAnnouncer.AudioSourceComponent;

[Obsolete("https://discussions.unity.com/t/playoneshot-performance/595405 PlayOneShot() have two times better performance than pool system")]
public sealed class AudioSourcePool : MonoBehaviour
{
    private Queue<AudioSource> _pool = new Queue<AudioSource>();
    private readonly HashSet<AudioSource> _activeAudioSources = new HashSet<AudioSource>();
    private LinkedList<AudioSource> _playingList = new LinkedList<AudioSource>();
    private Dictionary<AudioSource, LinkedListNode<AudioSource>> _playingMap = new Dictionary<AudioSource, LinkedListNode<AudioSource>>();
    private static AudioSourcePool _instance;

    public int initialSize = 2;
    public int maxSize = 7;


    #region Constructor
    public static AudioSourcePool Instance
    {
        get
        {
            if (_instance == null)
            {
                var obj = new GameObject("AudioSourcePool");
                DontDestroyOnLoad(obj);
                _instance = obj.AddComponent<AudioSourcePool>();
                _instance.Initialize();
            }
            return _instance;
        }
    }
    #endregion


    #region Public API
    private void Initialize()
    {
        for (int i = 0; i < initialSize; i++)
        {
            var audioSource = CreateNewAudioSource();
            _pool.Enqueue(audioSource);
        }
    }

    public void CancelAudioPlaying(string tag)
    {
        
    }


    public void PlayOneShot(AudioClip clip, AudioSourceSetting config)
    {
        var audioSource = Get();
        audioSource = AudioSourceManager.ConfigureAudioSource(audioSource, config);
        audioSource.clip = clip;
        UnderwaterController_inWater_Instance.CheckIsInWater();
        audioSource.Play();

        StartCoroutine(RecycleAfterPlay(audioSource));
    }

    public void AddAudioLowPassFilterToActiveAudioSource()
    {
        foreach (AudioSource audioSource in _activeAudioSources)
        {
            if (audioSource != null && audioSource.gameObject.activeInHierarchy)
            {
                AudioSourceManager.AddLowPassFilter(audioSource);
            }
        }

    }

    public void RemoveAudioLowPassFilterFromActiveAudioSource()
    {
        foreach (AudioSource audioSource in _activeAudioSources)
        {
            if (audioSource != null && audioSource.gameObject.activeInHierarchy)
            {
                AudioSourceManager.RemoveLowPassFilter(audioSource);
            }
        }

    }

    public void UpdateAllActiveSourcesVolume(float targetVolume, float duration = 0.35f)
    {
        foreach (var audioSource in _activeAudioSources)
        {
            if (
                audioSource != null
                && audioSource.gameObject.activeInHierarchy
                && audioSource.isPlaying
            )
            {
                StartCoroutine(AudioSourceManager.FadeVolume(audioSource,
                                                             targetVolume,
                                                             duration));
            }
        }
    }

    public void StopAllAudioSource()
    {
        foreach (var audioSource in _activeAudioSources)
        {
            Recycle(audioSource);
        }
    }
    #endregion


    #region Pool Management
    private AudioSource Get()
    {
        if (_activeAudioSources.Count >= maxSize)
        {
            if (_playingList.Count > 0)
            {
                var oldestNode = _playingList.First;
                _playingList.RemoveFirst();
                _playingMap.Remove(oldestNode.Value);

                LogManager.LogWarning("Max audio sources reached, forcibly recycling the oldest source.");
                Recycle(oldestNode.Value);
            }
        }

        AudioSource audioSource = (_pool.Count > 0) ? _pool.Dequeue() : CreateNewAudioSource();
        audioSource.gameObject.SetActive(true);
        _activeAudioSources.Add(audioSource);

        var node = _playingList.AddLast(audioSource);
        _playingMap[audioSource] = node;

        return audioSource;
    }

    private AudioSource CreateNewAudioSource()
    {
        LogManager.LogInfo("Create a new Audio Source");
        var go = new GameObject("PooledAudioSource");
        go.transform.SetParent(transform);
        var audioSource = go.AddComponent<AudioSource>();
        go.SetActive(false);
        return audioSource;
    }


    private IEnumerator RecycleAfterPlay(AudioSource audioSource)
    {
        yield return new WaitForSeconds(audioSource.clip.length);

        if (_playingMap.TryGetValue(audioSource, out var node))
        {
            _playingList.Remove(node);
            _playingMap.Remove(audioSource);
        }

        Recycle(audioSource);
    }

    private void Recycle(AudioSource audioSource)
    {
        if (audioSource == null) return;
        audioSource.Stop();
        audioSource.clip = null;
        audioSource.gameObject.SetActive(false);
        _activeAudioSources.Remove(audioSource);
        _pool.Enqueue(audioSource);
    }
    #endregion



}

