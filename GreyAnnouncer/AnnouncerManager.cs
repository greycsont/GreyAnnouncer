using System.Collections.Generic;
using System;
using System.Text;

namespace greycsont.GreyAnnouncer;
                                                      
public class AnnouncerManager
{           
    public  static float                          sharedCooldown = 0f;
    private static Dictionary<string, IAnnouncer> m_announcers   = new Dictionary<string, IAnnouncer>();
    public  static long                           playRequestId  = 0;
    #region Public Methods
    public static void RegisterAnnouncer(string name, IAnnouncer announcer)
    {
        if (m_announcers.ContainsKey(name))
        {
            Plugin.log.LogWarning($"Overwriting existing announcer: {name}");
        }
        m_announcers[name] = announcer;
    }

    public static void ReloadAllAnnouncers()
    {
        if (m_announcers.Count == 0) return;

        foreach (var announcer in m_announcers.Values)
        {
            TryReloadAnnouncerAudios(announcer);
        }
    }

    public static void UpdateAllAnnouncerPaths(string newPath)
    {
        if (m_announcers.Count == 0) return;

        foreach (var announcer in m_announcers.Values)
        {
            TryUpdateAnnouncerAudioPath(announcer, newPath);
        }
    }

    public static void ResetCooldown()
    {
        foreach (var announcer in m_announcers.Values)
        {
            announcer.ResetCooldown();
        }
        sharedCooldown = 0f;
    }

    public static void ClearAudioClipsCache()
    {
        foreach (var announcer in m_announcers.Values)
        {
            announcer.ClearAudioClipsCache();
        }
    }

    public static IAnnouncer GetAnnouncer(string name)
    {
        return m_announcers.TryGetValue(name, out var announcer) ? announcer : null;
    }
    #endregion


    #region Private Methods
    private static void TryReloadAnnouncerAudios(IAnnouncer announcer)
    {
        try 
        {
            announcer.ReloadAudio();
        }
        catch (Exception ex)
        {
            Plugin.log.LogError($"Failed to reload announcer: {ex.Message}");
        }
    }

    private static void TryUpdateAnnouncerAudioPath(IAnnouncer announcer, string newPath)
    {
        try 
        {
            announcer.UpdateAudioPath(newPath);
            Plugin.log.LogInfo("Successfully updated announcer audio path");
        }
        catch (Exception ex)
        {
            Plugin.log.LogError($"Failed to update announcer audio path: {ex.Message}");
        }
    }
    #endregion
    
}
