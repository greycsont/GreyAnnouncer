using System;

namespace GreyAnnouncer.AnnouncerAPI;
                                                      
public static class AnnouncerManager
{
    public  static Action reloadAnnouncer;
    public  static Action updateAnnouncerPath;
    public  static Action resetCooldown;
    public  static Action clearAudioClipCache;            
    public  static float  sharedCooldown = 0f;
    public  static long   playRequestId  = 0;

    #region Public Methods
    public static void ReloadAllAnnouncers()
    {
        reloadAnnouncer?.Invoke();
    }

    public static void UpdateAllAnnouncerPaths()
    {
        updateAnnouncerPath?.Invoke();
    }

    public static void ResetCooldown()
    {
        sharedCooldown = 0f;
        resetCooldown?.Invoke();
    }

    public static void ClearAudioClipsCache()
    {
        clearAudioClipCache?.Invoke();
    }
    #endregion
    
}
