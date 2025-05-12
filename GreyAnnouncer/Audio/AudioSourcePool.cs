using greycsont.GreyAnnouncer;
using System.Collections;
using System.Collections.Generic; //audio clip
using UnityEngine;

public class AudioSourcePool : MonoBehaviour
{
    #region Private Fields
    private                 Queue<AudioSource>                                   m_pool               = new Queue<AudioSource>();
    private        readonly HashSet<AudioSource>                                 m_activeAudioSources = new HashSet<AudioSource>();
    private                 LinkedList<AudioSource>                              m_playingList        = new LinkedList<AudioSource>();
    private                 Dictionary<AudioSource, LinkedListNode<AudioSource>> m_playingMap         = new Dictionary<AudioSource, LinkedListNode<AudioSource>>();
    private static          AudioSourcePool                                      m_instance;
    #endregion

    #region Public Fields
    public                  int                                                  initialSize          = 2;
    public                  int                                                  maxSize              = 7;
    #endregion


    #region Constructor
    public static AudioSourcePool Instance
    {
        get
        {
            if (m_instance == null)
            {
                var obj = new GameObject("AudioSourcePool");
                DontDestroyOnLoad(obj);
                m_instance = obj.AddComponent<AudioSourcePool>();
                m_instance.Initialize();
            }
            return m_instance;
        }
    }
    #endregion


    #region Public API
    private void Initialize()
    {
        for (int i = 0; i < initialSize; i++)
        {
            var audioSource = CreateNewAudioSource();
            m_pool.Enqueue(audioSource);
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
        foreach (AudioSource audioSource in m_activeAudioSources)
        {
            if (audioSource != null && audioSource.gameObject.activeInHierarchy)
            {
                AudioSourceManager.AddLowPassFilter(audioSource);
            }
        }

    }

    public void RemoveAudioLowPassFilterFromActiveAudioSource()
    {
        foreach (AudioSource audioSource in m_activeAudioSources)
        {
            if (audioSource != null && audioSource.gameObject.activeInHierarchy)
            {
                AudioSourceManager.RemoveLowPassFilter(audioSource);
            }
        }

    }

    public void UpdateAllActiveSourcesVolume(float targetVolume, float duration = 0.35f)
    {
        foreach(var audioSource in m_activeAudioSources)
        {
            if(
                audioSource != null 
                && audioSource.gameObject.activeInHierarchy 
                && audioSource.isPlaying
            )
            {
                StartCoroutine(AudioSourceManager.FadeVolume(audioSource, targetVolume, duration));
            }
        }
    }
    #endregion


    #region Pool Management
    private AudioSource Get()
    {
        if (m_activeAudioSources.Count >= maxSize)
        {
            if (m_playingList.Count > 0)
            {
                var oldestNode = m_playingList.First;
                m_playingList.RemoveFirst();
                m_playingMap.Remove(oldestNode.Value);

                Plugin.log.LogWarning("Max audio sources reached, forcibly recycling the oldest source.");
                Recycle(oldestNode.Value);
            }
        }

        AudioSource audioSource = (m_pool.Count > 0) ? m_pool.Dequeue() : CreateNewAudioSource();
        audioSource.gameObject.SetActive(true);
        m_activeAudioSources.Add(audioSource);

        var node                  = m_playingList.AddLast(audioSource);
        m_playingMap[audioSource] = node;

        return audioSource;
    }

    private AudioSource CreateNewAudioSource()
    {
        Plugin.log.LogInfo("Create a new Audio Source");
        var go = new GameObject("PooledAudioSource");
        go.transform.SetParent(transform);
        var audioSource = go.AddComponent<AudioSource>();
        go.SetActive(false);
        return audioSource;
    }


    private IEnumerator RecycleAfterPlay(AudioSource audioSource)
    {
        yield return new WaitForSeconds(audioSource.clip.length);

        if (m_playingMap.TryGetValue(audioSource, out var node))
        {
            m_playingList.Remove(node);
            m_playingMap.Remove(audioSource);
        }

        Recycle(audioSource);
    }

    private void Recycle(AudioSource audioSource)
    {
        if (audioSource == null) return;
        audioSource.Stop();
        audioSource.clip = null;
        audioSource.gameObject.SetActive(false);
        m_activeAudioSources.Remove(audioSource);
        m_pool.Enqueue(audioSource);
    }
    #endregion


    
}

