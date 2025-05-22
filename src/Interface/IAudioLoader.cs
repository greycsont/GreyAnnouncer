using System.Threading.Tasks;
using GreyAnnouncer.AnnouncerAPI;
using UnityEngine;

public interface IAudioLoader
{
    public AnnouncerJsonSetting jsonSetting  { get; set; }
    public AudioClip GetClipFromCache(string category);
    public AudioClip GetRandomClipFromAudioClips();
    public Task<AudioClip> LoadAndGetSingleAudioClipAsync(string category);
    public Task<AudioClip> GetRandomClipFromAllAvailableFiles();
    public Task FindAvailableAudioAsync();
    public void UpdateAudioPath(string newAudioPath);
    public void ClearCache();
    public void UpdateJsonSetting(AnnouncerJsonSetting jsonSetting);
}