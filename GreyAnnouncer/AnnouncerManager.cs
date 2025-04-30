using System.Collections.Generic;
using System;

namespace greycsont.GreyAnnouncer;
                                                      
public class AnnouncerManager
{           
    public  static float                              sharedCooldown = 0f;
    private static Dictionary<string, AudioAnnouncer> _announcers    = new Dictionary<string, AudioAnnouncer>();

    #region Public Methods
    public static void RegisterAnnouncer(string name, AudioAnnouncer announcer)
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

    public static AudioAnnouncer GetAnnouncer(string name)
    {
        return _announcers.TryGetValue(name, out var announcer) ? announcer : null;
    }
    #endregion


    #region Private Methods
    private static void TryReloadAnnouncerAudios(AudioAnnouncer announcer)
    {
        try 
        {
            announcer.ReloadAudio();
            Plugin.Log.LogInfo("Successfully reloaded announcer");
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"Failed to reload announcer: {ex.Message}");
        }
    }

    private static void TryUpdateAnnouncerAudioPath(AudioAnnouncer announcer, string newPath)
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
