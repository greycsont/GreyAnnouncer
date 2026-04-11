using System.Threading.Tasks;

using GreyAnnouncer.AudioSourceComponent;

namespace GreyAnnouncer.AnnouncerCore;

public interface IAudioLoader
{
    public Task<Sound> GetAudioClip(string category);

    public Task FindAvailableAudioAsync();

    public void ClearCache();

    public void SetProvider(IAnnouncer provider);
}
