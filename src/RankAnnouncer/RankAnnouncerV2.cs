using System.IO;
using System.Linq;
using System.Collections.Generic;

using GreyAnnouncer.AnnouncerAPI;
using GreyAnnouncer.FrontEnd;

namespace GreyAnnouncer.RankAnnouncer;

public static class RankAnnouncer
{
    private static readonly Dictionary<string, string> _displayNameMapping = new Dictionary<string, string>{   //used only for creating json
        {"D", "Destruction"},
        {"C", "Chaotic"},
        {"B", "Brutal"},
        {"A", "Anarchic"},
        {"S", "Supreme"},
        {"SS", "SSadistic"},
        {"SSS", "SSShitstorm"},
        {"U",   "ULTRAKILL"}
    };

    private static AudioAnnouncer _announcer;
    private static readonly string _jsonName = "RankAnnouncer.json";
    private static readonly string _title = "RankAnnouncer";

    public static void Initialize()
    {
        _announcer = new AudioAnnouncer(
            new AudioLoader(),
            new CooldownManager(_displayNameMapping.Keys.ToArray()),
            _displayNameMapping,
            _jsonName,
            _title
        );

        SubscribeAnnouncerManager();
    }

    public static void PlayRankSound(int rank)
    {
        _ = _announcer.PlayAudioViaIndex(rank);
    }

    private static void SubscribeAnnouncerManager()
    {
        AnnouncerManager.reloadAnnouncer     += ReloadAnnouncer;
        AnnouncerManager.resetCooldown       += ResetCooldowns;
        AnnouncerManager.clearAudioClipCache += ClearAudioClipCache;
    }

 
    #region Subscription
    private static void ReloadAnnouncer()
    {
        _announcer.ReloadAudio();
    }

    private static void ResetCooldowns()
    {
        _announcer.ResetCooldown();
    }

    private static void ClearAudioClipCache()
    {
        _announcer.ClearAudioClipsCache();
    }
    #endregion
}
