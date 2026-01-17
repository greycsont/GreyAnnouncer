using System;
using System.Collections.Generic;

namespace GreyAnnouncer.AnnouncerAPI;
                                                      
public static class AnnouncerManager
{
    public static List<AudioAnnouncer> announcers = new List<AudioAnnouncer>();

    public static Action reloadAnnouncer;

    public static Action resetCooldown;

    public static Action clearAudioClipCache;      

    public static long playRequestId  = 0;


    #region Public Methods
    public static List<AudioAnnouncer> GetAllAnnouncers()
        => announcers; // Placeholder for future implementation

    public static void AddAnnouncer(AudioAnnouncer announcer)
        => announcers.Add(announcer);

    public static void ReloadAllAnnouncers()
        => reloadAnnouncer?.Invoke();

    public static void ResetCooldown()
        => resetCooldown?.Invoke();

    public static void ClearAudioClipsCache()
        => clearAudioClipCache?.Invoke();

    #endregion
    
}
