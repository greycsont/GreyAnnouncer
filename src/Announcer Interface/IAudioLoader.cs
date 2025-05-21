using System.Threading.Tasks;
using UnityEngine;

public interface IAudioLoader
{
    public AudioClip GetClipFromCache(string category);
    public AudioClip GetRandomClipFromAudioClips();
    public Task<AudioClip> LoadAndGetSingleAudioClipAsync(string category);
    public Task<AudioClip> GetRandomClipFromAllAvailableFiles();
    public Task FindAvailableAudioAsync();
    public void UpdateAudioPath(string newAudioPath);
    public void ClearCache();
}