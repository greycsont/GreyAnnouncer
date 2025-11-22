using System.IO;
using System.Linq;
using System.Collections.Generic;

using GreyAnnouncer.AnnouncerAPI;
using GreyAnnouncer.PluginConfiguratorGUI;

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
    private static readonly string _pageTitle = "Rank Announcer";

    public static void Initialize()
    {
        _announcer = new AudioAnnouncer(
            new AudioLoader(BepInExConfig.audioFolderPath.Value),
            new CooldownManager(_displayNameMapping.Keys.ToArray()),
            _displayNameMapping,
            _jsonName
        );

        SubscribeAnnouncerManager();
        PluginConfigPanelInitialization(_pageTitle, _announcer);
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
        AnnouncerManager.updateAnnouncerPath += UpdateAudioPath;
    }

 
    #region Subscription
    private static void ReloadAnnouncer()
    {
        _announcer.ReloadAudio();
    }

    private static void UpdateAudioPath()
    {
        _announcer.UpdateAudioPath(BepInExConfig.audioFolderPath.Value);
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

    private static void PluginConfigPanelInitialization(string announcerName, AudioAnnouncer audioAnnouncer)
    {
        RegisterRankAnnouncerPagev2.Build(announcerName, audioAnnouncer);
        /*PluginDependencies.LoadIfPluginExists(
            PluginDependencies.PLUGINCONFIGURATOR_GUID,
            "RegisterRankAnnouncerPageV2",
            () => ReflectionManager.LoadByReflection(
                      "GreyAnnouncer.RankAnnouncer.RegisterRankAnnouncerPageV2", 
                      "Build", 
                      new object[] { announcerName, audioAnnouncer })
        );*/
    }
}
