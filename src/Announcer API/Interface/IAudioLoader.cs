using System.Threading.Tasks;

using GreyAnnouncer.AudioSourceComponent;

namespace GreyAnnouncer.AnnouncerAPI;
public interface IAudioLoader
{
    public AnnouncerConfig announcerConfig  { get; set; }

    public Task<Sound> LoadAudioClip(string category);

    public Task FindAvailableAudioAsync();

    public void ClearCache();

    public void UpdateAnnouncerConfig(AnnouncerConfig jsonSetting);
    
}