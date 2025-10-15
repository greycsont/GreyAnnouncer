using UnityEngine;
using System;

public class TagedAudioSource
{
    public string[] tags = Array.Empty<string>();
    public AudioSource audioSource;

    public bool HasTag(string tag)
    {
        return tags != null && Array.Exists(tags, t => t == tag);
    }
}