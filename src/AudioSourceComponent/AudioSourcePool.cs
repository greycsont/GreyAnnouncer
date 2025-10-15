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
using UnityEngine.Rendering;



namespace GreyAnnouncer.AudioSourceComponent;

[Obsolete("https://discussions.unity.com/t/playoneshot-performance/595405 PlayOneShot() have two times better performance than pool system")]
public sealed class AudioSourcePool : MonoBehaviour
{
    private Queue<TagedAudioSource> _pool = new Queue<TagedAudioSource>();
    private readonly HashSet<TagedAudioSource> _activeAudioSources = new HashSet<TagedAudioSource>();
    private LinkedList<TagedAudioSource> _playingList = new LinkedList<TagedAudioSource>();
    private Dictionary<TagedAudioSource, LinkedListNode<TagedAudioSource>> _playingMap = new Dictionary<TagedAudioSource, LinkedListNode<TagedAudioSource>>();
    private static AudioSourcePool _instance;

    public int initialSize = 2;
    public int maxSize = 15;


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

    public void CancelAudioPlaying(string[] tags)
    {
        var toRecycle = new List<TagedAudioSource>();

        // 第一步：先找出要回收的
        foreach (var tagedAudioSource in _activeAudioSources)
        {
            foreach (var tag in tags)
            {
                if (tagedAudioSource.HasTag(tag))
                {
                    toRecycle.Add(tagedAudioSource);
                    break;
                }
            }
        }

        // 第二步：再统一回收
        foreach (var tagedAudioSource in toRecycle)
        {
            Recycle(tagedAudioSource);
        }
    }


    public void PlayOneShot(AudioClip clip, AudioSourceSetting config)
    {
        var tagedAudioSource = Get();
        tagedAudioSource.tags = config.Tags;

        var audioSource = tagedAudioSource.audioSource;

        audioSource = AudioSourceManager.ConfigureAudioSource(audioSource, config);
        audioSource.clip = clip;
        UnderwaterController_inWater_Instance.CheckIsInWater();
        audioSource.Play();

        StartCoroutine(RecycleAfterPlay(tagedAudioSource));
    }

    public void AddAudioLowPassFilterToActiveAudioSource()
    {
        foreach (TagedAudioSource tagedAudioSource in _activeAudioSources)
        {
            var audioSource = tagedAudioSource.audioSource;
            if (audioSource != null && audioSource.gameObject.activeInHierarchy)
            {
                AudioSourceManager.AddLowPassFilter(audioSource);
            }
        }

    }

    public void RemoveAudioLowPassFilterFromActiveAudioSource()
    {
        foreach (TagedAudioSource tagedAudioSource in _activeAudioSources)
        {
            var audioSource = tagedAudioSource.audioSource;
            if (audioSource != null && audioSource.gameObject.activeInHierarchy)
            {
                AudioSourceManager.RemoveLowPassFilter(audioSource);
            }
        }

    }

    public void UpdateAllActiveSourcesVolume(float targetVolume, float duration = 0.35f)
    {
        foreach (var tagedAudioSource in _activeAudioSources)
        {
            var audioSource = tagedAudioSource.audioSource;
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
    private TagedAudioSource Get()
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

        var tagedAudioSource = (_pool.Count > 0) ? _pool.Dequeue() : CreateNewAudioSource();
        tagedAudioSource.audioSource.gameObject.SetActive(true);
        _activeAudioSources.Add(tagedAudioSource);

        var node = _playingList.AddLast(tagedAudioSource);
        _playingMap[tagedAudioSource] = node;

        return tagedAudioSource;
    }

    private TagedAudioSource CreateNewAudioSource()
    {
        LogManager.LogInfo("Create a new Audio Source");
        var go = new GameObject("PooledAudioSource");
        go.transform.SetParent(transform);
        var newAudioSource = go.AddComponent<AudioSource>();
        go.SetActive(false);
        
        return new TagedAudioSource { audioSource = newAudioSource };
    }


    private IEnumerator RecycleAfterPlay(TagedAudioSource tagedAudioSource)
    {
        yield return new WaitForSeconds(tagedAudioSource.audioSource.clip.length);

        if (_playingMap.TryGetValue(tagedAudioSource, out var node))
        {
            _playingList.Remove(node);
            _playingMap.Remove(tagedAudioSource);
        }

        Recycle(tagedAudioSource);
    }

    private void Recycle(TagedAudioSource tagedAudioSource)
    {
        tagedAudioSource.tags = Array.Empty<string>();

        var audioSource = tagedAudioSource.audioSource;

        if (audioSource == null) return;
        audioSource.Stop();
        audioSource.clip = null;
        audioSource.gameObject.SetActive(false);

        _activeAudioSources.Remove(tagedAudioSource);
        _pool.Enqueue(tagedAudioSource);
    }
    #endregion



}

