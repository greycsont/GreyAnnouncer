using System.Threading.Tasks;
using GreyAnnouncer.AnnouncerAPI;
using UnityEngine;

public interface IAudioLoader
{
    public AnnouncerJsonSetting jsonSetting  { get; set; }
    public AudioClipWithCategory? GetClipFromCache(string category);
    public AudioClipWithCategory? GetRandomClipFromAudioClips();
    public Task<AudioClipWithCategory?> LoadAndGetSingleAudioClipAsync(string category);
    public Task<AudioClipWithCategory?> GetRandomClipFromAllAvailableFiles();
    public Task FindAvailableAudioAsync();
    public void UpdateAudioPath(string newAudioPath);
    public void ClearCache();
    public void UpdateJsonSetting(AnnouncerJsonSetting jsonSetting);
}