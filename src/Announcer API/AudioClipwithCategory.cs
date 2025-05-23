using UnityEngine;

public struct AudioClipWithCategory
{
    public string category;
    public AudioClip clip;

    public AudioClipWithCategory(string category, AudioClip clip)
    {
        this.category = category;
        this.clip = clip;
    }
}