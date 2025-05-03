using System.Collections.Generic;
using System;

namespace greycsont.GreyAnnouncer;
                                                      
public class AnnouncerManager
{           
    public  static float                              sharedCooldown = 0f;
    private static Dictionary<string, IAnnouncer> _announcers    = new Dictionary<string, IAnnouncer>();

    #region Public Methods
    public static void RegisterAnnouncer(string name, IAnnouncer announcer)
    {
        if (_announcers.ContainsKey(name))
        {
            Plugin.Log.LogWarning($"Overwriting existing announcer: {name}");
        }
        _announcers[name] = announcer;
    }

    public static void ReloadAllAnnouncers()
    {
        if (_announcers.Count == 0) return;

        foreach (var announcer in _announcers.Values)
        {
            TryReloadAnnouncerAudios(announcer);
        }
    }

    public static void UpdateAllAnnouncerPaths(string newPath)
    {
        if (_announcers.Count == 0) return;

        foreach (var announcer in _announcers.Values)
        {
            TryUpdateAnnouncerAudioPath(announcer, newPath);
        }
    }

    public static IAnnouncer GetAnnouncer(string name)
    {
        return _announcers.TryGetValue(name, out var announcer) ? announcer : null;
    }

    public static void ResetCooldown()
    {
        foreach (var announcer in _announcers.Values)
        {
            announcer.ResetCooldown();
        }
        sharedCooldown = 0f;
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
            Plugin.Log.LogError($"Failed to reload announcer: {ex.Message}");
        }
    }

    private static void TryUpdateAnnouncerAudioPath(IAnnouncer announcer, string newPath)
    {
        try 
        {
            announcer.UpdateAudioPath(newPath);
            Plugin.Log.LogInfo("Successfully updated announcer audio path");
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"Failed to update announcer audio path: {ex.Message}");
        }
    }
    #endregion
    
}
