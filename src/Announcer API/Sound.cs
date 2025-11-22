using UnityEngine;
using UnityEngine.Audio;

public class Sound
{
    public string category;
    public AudioClip clip;
    public float SpatialBlend = 0f;
    [Range(0f, 1f)]
    public float Volume = 1f;
    public float[] Pitch = {1f,1f};
    public bool Loop = false;
    public int Priority = 0;
    public AudioMixerGroup audioMixerGroup;

    public Sound(string category, AudioClip clip, float volume, float[] pitch)
    {
        this.category = category;
        this.clip = clip;
        this.Volume = volume;

        /* RIP fk float
         * pitch[0] ^= pitch[1];
         * pitch[1] ^= pitch[0];
         * pitch[0] ^= pitch[1]; 
         */
        this.Pitch = new[] { Mathf.Min(pitch[0], pitch[1]), Mathf.Max(pitch[0], pitch[1]) };
    }
}
