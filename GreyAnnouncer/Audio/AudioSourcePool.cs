using greycsont.GreyAnnouncer;
using System.Collections;
using System.Collections.Generic; //audio clip
using UnityEngine;

public class AudioSourcePool : MonoBehaviour
{
    private                 Queue<AudioSource>                                   pool               = new Queue<AudioSource>();
    private        readonly HashSet<AudioSource>                                 activeAudioSources = new HashSet<AudioSource>();
    private                 LinkedList<AudioSource>                              playingList        = new LinkedList<AudioSource>();
    private                 Dictionary<AudioSource, LinkedListNode<AudioSource>> playingMap         = new Dictionary<AudioSource, LinkedListNode<AudioSource>>();
    private static          AudioSourcePool                                      instance;
    public                  int                                                  initialSize        = 3;
    public                  int                                                  maxSize            = 5;

    #region Public Methods
    public static AudioSourcePool Instance
    {
        get
        {
            if (instance == null)
            {
                var obj = new GameObject("AudioSourcePool");
                DontDestroyOnLoad(obj);
                instance = obj.AddComponent<AudioSourcePool>();
                instance.Initialize();
            }
            return instance;
        }
    }

    private void Initialize()
    {
        for (int i = 0; i < initialSize; i++)
        {
            var audioSource = CreateNewAudioSource();
            pool.Enqueue(audioSource);
        }
    }


    public void PlayOneShot(AudioClip clip, AudioSourceSetting config)
    {
        var audioSource  = Get();
        audioSource      = AudioSourceManager.ConfigureAudioSource(audioSource, config);
        audioSource.clip = clip;
        UnderwaterController_inWater_Instance.CheckIsInWater();
        audioSource.Play();

        StartCoroutine(RecycleAfterPlay(audioSource));
    }

    public void AddAudioLowPassFilterToActiveAudioSource()
    {
        foreach (AudioSource audioSource in activeAudioSources)
        {
            if (audioSource != null && audioSource.gameObject.activeInHierarchy)
            {
                AudioSourceManager.AddLowPassFilter(audioSource);
            }
        }

    }

    public void RemoveAudioLowPassFilterFromActiveAudioSource()
    {
        foreach (AudioSource audioSource in activeAudioSources)
        {
            if (audioSource != null && audioSource.gameObject.activeInHierarchy)
            {
                AudioSourceManager.RemoveLowPassFilter(audioSource);
            }
        }

    }

    public void UpdateAllActiveSourcesVolume(float targetVolume, float duration = 0.35f)
    {
        foreach(var audioSource in activeAudioSources)
        {
            if(audioSource != null && audioSource.gameObject.activeInHierarchy && audioSource.isPlaying)
            {
                StartCoroutine(AudioSourceManager.FadeVolume(audioSource, targetVolume, duration));
            }
        }
    }
    #endregion


    #region Private Methods

    private AudioSource Get()
    {
        if (activeAudioSources.Count >= maxSize)
        {
            if (playingList.Count > 0)
            {
                var oldestNode = playingList.First;
                playingList.RemoveFirst();
                playingMap.Remove(oldestNode.Value);

                Plugin.Log.LogWarning("Max audio sources reached, forcibly recycling the oldest source.");
                ForceRecycle(oldestNode.Value);
            }
        }

        AudioSource audioSource = (pool.Count > 0) ? pool.Dequeue() : CreateNewAudioSource();
        audioSource.gameObject.SetActive(true);
        activeAudioSources.Add(audioSource);

        var node                = playingList.AddLast(audioSource);
        playingMap[audioSource] = node;

        return audioSource;
    }

    private AudioSource CreateNewAudioSource()
    {
        Plugin.Log.LogInfo("Create a new Audio Source");
        var go = new GameObject("PooledAudioSource");
        go.transform.SetParent(transform);
        var audioSource = go.AddComponent<AudioSource>();
        go.SetActive(false);
        return audioSource;
    }


    private IEnumerator RecycleAfterPlay(AudioSource audioSource)
    {
        yield return new WaitForSeconds(audioSource.clip.length);

        if (playingMap.TryGetValue(audioSource, out var node))
        {
            playingList.Remove(node);
            playingMap.Remove(audioSource);
        }

        Return(audioSource);
    }

    private void ForceRecycle(AudioSource audioSource)
    {
        if (audioSource == null) return;
        audioSource.Stop();
        audioSource.clip = null;
        audioSource.gameObject.SetActive(false);
        activeAudioSources.Remove(audioSource);
        pool.Enqueue(audioSource);
    }

    private void Return(AudioSource audioSource)
    {
        audioSource.Stop();
        audioSource.clip = null;
        audioSource.gameObject.SetActive(false);
        activeAudioSources.Remove(audioSource);
        pool.Enqueue(audioSource);
    }
    #endregion


    
}

