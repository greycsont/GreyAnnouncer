using System;

namespace GreyAnnouncer.AnnouncerAPI;
                                                      
public static class AnnouncerManager
{
    public  static Action reloadAnnouncer;
    public  static Action resetCooldown;
    public  static Action clearAudioClipCache;            
    public  static long   playRequestId  = 0;

    #region Public Methods
    public static void ReloadAllAnnouncers()
    {
        reloadAnnouncer?.Invoke();
    }

    public static void ResetCooldown()
    {
        resetCooldown?.Invoke();
    }

    public static void ClearAudioClipsCache()
    {
        clearAudioClipCache?.Invoke();
    }
    #endregion
    
}
