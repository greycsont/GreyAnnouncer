using System.Collections.Generic;
using System;
using System.Text;

namespace greycsont.GreyAnnouncer;
                                                      
public class AnnouncerManager
{
    public  static Action reloadAnnouncer;
    public  static Action updateAnnouncerPath;
    public  static Action resetCooldown;
    public  static Action clearAudioClipCache;            
    public  static float  sharedCooldown = 0f;
    public  static long   playRequestId  = 0;

    #region Public Methods
    // need test
    //
    //
    //
    //
    public static void ReloadAllAnnouncer()
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
