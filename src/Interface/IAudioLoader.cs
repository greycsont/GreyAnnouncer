using System.Threading.Tasks;
using GreyAnnouncer.AnnouncerAPI;
using UnityEngine;

public interface IAudioLoader
{
    public AnnouncerJsonSetting jsonSetting  { get; set; }
    public Task<Sound> LoadAudioClip(string category);
    public Task FindAvailableAudioAsync();
    public void UpdateAudioPath(string newAudioPath);
    public void ClearCache();
    public void UpdateJsonSetting(AnnouncerJsonSetting jsonSetting);
}