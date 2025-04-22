using System.Collections;
using System.Collections.Generic; //audio clip
using greycsont.GreyAnnouncer;
using UnityEngine;

public class AudioSourcePool : MonoBehaviour
{
    private AudioSource                                          soloAudioSource;
    private Queue<AudioSource>                                   pool               = new Queue<AudioSource>();
    private readonly HashSet<AudioSource>                        activeAudioSources = new HashSet<AudioSource>();
    private LinkedList<AudioSource>                              playingList        = new LinkedList<AudioSource>();
    private Dictionary<AudioSource, LinkedListNode<AudioSource>> playingMap         = new Dictionary<AudioSource, LinkedListNode<AudioSource>>();
    private static AudioSourcePool                               instance;
    public  int                                                  initialSize        = 3;
    public  int                                                  maxSize            = 5;

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
            var source = CreateNewAudioSource();
            pool.Enqueue(source);
        }
    }

    private AudioSource CreateNewAudioSource()
    {
        Plugin.Log.LogInfo("Create a new Audio Source");
        var go = new GameObject("PooledAudioSource");
        go.transform.SetParent(transform);
        var source = go.AddComponent<AudioSource>();
        go.SetActive(false);
        return source;
    }

    public void PlayOneShot(AudioClip clip, AudioConfiguration config)
    {
        var source  = Get();
        source      = ConfigureAudioSource(source, config);
        source.clip = clip;
        source.Play();

        StartCoroutine(RecycleAfterPlay(source));
    }

    public void PlayOverridable(AudioClip clip, AudioSourcePool.AudioConfiguration config)
    {
        if (soloAudioSource == null)
        {
            var go = new GameObject("SoloAudioSource");
            DontDestroyOnLoad(go);
            soloAudioSource = go.AddComponent<AudioSource>();
        }

        soloAudioSource = ConfigureAudioSource(soloAudioSource, config);

        // 停止之前的音频（可选，保险）

        soloAudioSource.clip = clip;
        soloAudioSource.Play();
    }

    public void UpdateAllActiveSourcesVolume(float volume, float duration = 0.35f)
    {
        foreach(var source in activeAudioSources)
        {
            if(source != null && source.gameObject.activeInHierarchy && source.isPlaying)
            {
                StartCoroutine(greycsont.GreyAnnouncer.AudioSourceManager.FadeVolume(source, volume, duration));
            }
        }
    }

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

        AudioSource source = (pool.Count > 0) ? pool.Dequeue() : CreateNewAudioSource();
        source.gameObject.SetActive(true);
        activeAudioSources.Add(source);

        var node           = playingList.AddLast(source);
        playingMap[source] = node;

        return source;
    }

    private IEnumerator RecycleAfterPlay(AudioSource source)
    {
        yield return new WaitForSeconds(source.clip.length);

        if (playingMap.TryGetValue(source, out var node))
        {
            playingList.Remove(node);
            playingMap.Remove(source);
        }

        Return(source);
    }

    private void ForceRecycle(AudioSource source)
    {
        if (source == null) return;
        source.Stop();
        source.clip = null;
        source.gameObject.SetActive(false);
        activeAudioSources.Remove(source);
        pool.Enqueue(source);
    }

    private void Return(AudioSource source)
    {
        source.Stop();
        source.clip = null;
        source.gameObject.SetActive(false);
        activeAudioSources.Remove(source);
        pool.Enqueue(source);
    }

    private AudioSource ConfigureAudioSource(AudioSource source, AudioConfiguration config)
    {
        source.spatialBlend = config.SpatialBlend;
        source.priority     = config.Priority;
        source.volume       = config.Volume;
        source.pitch        = config.Pitch;

        if (InstanceConfig.LowPassFilter_Enabled.Value)
        {
            greycsont.GreyAnnouncer.AudioSourceManager.AddLowPassFilter(source);
        }
        else
        {
            greycsont.GreyAnnouncer.AudioSourceManager.RemoveLowPassFilter(source);
        }

        return source;
    }

    public struct AudioConfiguration
    {
        public float SpatialBlend { get; set; }
        public int   Priority     { get; set; }
        public float Volume       { get; set; }
        public float Pitch        { get; set; }

        public static AudioConfiguration Default => new()
        {
            SpatialBlend = 0f,
            Priority     = 0,
            Volume       = 1f,
            Pitch        = 1f,
        };
    }
}

