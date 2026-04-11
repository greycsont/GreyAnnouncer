using System;
using System.Collections.Generic;

namespace GreyAnnouncer.AnnouncerCore;
                                                      
public static class AnnouncerManager
{
    public static List<IAnnouncer> announcers = new List<IAnnouncer>();

    public static Action reloadAnnouncer;

    public static Action resetCooldown;

    public static Action clearAudioClipCache;      

    public static long playRequestId = 0;


    public static List<IAnnouncer> GetAllAnnouncers()
        => announcers; // Placeholder for future implementation

    public static event Action<IAnnouncer> OnRegistered;

    public static void AddAnnouncer(IAnnouncer announcer)
    {
        announcers.Add(announcer);
        OnRegistered?.Invoke(announcer);
    }

    public static void ReloadAllAnnouncers()
        => reloadAnnouncer?.Invoke();

    public static void ResetCooldown()
        => resetCooldown?.Invoke();

    public static void ClearAudioClipsCache()
        => clearAudioClipCache?.Invoke();
    
}
