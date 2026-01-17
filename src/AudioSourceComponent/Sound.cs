using UnityEngine;
using UnityEngine.Audio;

namespace GreyAnnouncer.AudioSourceComponent;

public class Sound
{
    public string category;
    public AudioClip clip;
    public float SpatialBlend = 0f;
    [Range(0f, 1f)]
    public float Volume = 1f;
    public bool Loop = false;
    public int Priority = 0;
    public AudioMixerGroup audioMixerGroup;

    public Sound(string category, AudioClip clip, float volume)
    {
        this.category = category;
        this.clip = clip;
        this.Volume = volume;
    }
}
