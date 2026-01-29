using UnityEngine;
using UnityEngine.Audio;

namespace GreyAnnouncer.AudioSourceComponent;

public class Sound
{
    public string category;

    public AudioClip clip;

    public float spatialBlend = 0f;

    [Range(0f, 1f)]
    public float volume = 1f;

    public bool loop = false;

    public int priority = 0;

    public AudioMixerGroup AudioMixerGroup;
    

    public Sound(string category, AudioClip clip, float volume)
    {
        this.category = category;
        this.clip = clip;
        this.volume = volume;
    }
}
