using System.Threading.Tasks;
using GreyAnnouncer.AnnouncerAPI;
using UnityEngine;

public interface IAudioLoader
{
    public AnnouncerMapping jsonSetting  { get; set; }
    public Task<Sound> LoadAudioClip(string category);
    public Task FindAvailableAudioAsync();
    public void ClearCache();
    public void UpdateJsonSetting(AnnouncerMapping jsonSetting);
}