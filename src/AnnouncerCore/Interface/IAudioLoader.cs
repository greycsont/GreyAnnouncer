using System.Collections.Generic;
using System.Threading.Tasks;

using GreyAnnouncer.AudioSourceComponent;

namespace GreyAnnouncer.AnnouncerCore;

public interface IAudioLoader
{
    public Task<Sound> GetAudioClipInCategory(string category);
    public Task<Sound> GetRandomAudioClipInCategory(List<string> categories);

    public Task FindAvailableAudioAsync();

    public void ClearCache();

    public void SetProvider(IAnnouncer provider);
}
