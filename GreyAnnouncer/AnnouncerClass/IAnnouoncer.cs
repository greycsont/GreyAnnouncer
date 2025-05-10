namespace greycsont.GreyAnnouncer;

public interface IAnnouncer
{
    public void ReloadAudio();
    public void UpdateAudioPath(string newAudioPaths);
    public void ResetCooldown();
    public void ClearAudioClipsCache();
}