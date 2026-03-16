using System.Threading.Tasks;

using GreyAnnouncer.AudioSourceComponent;

namespace GreyAnnouncer.AnnouncerAPI;

public interface IAudioLoader
{
    public Task<Sound> LoadAudioClip(string category);

    public Task FindAvailableAudioAsync();

    public void ClearCache();

    public void SetProvider(IAnnouncer provider);
}
