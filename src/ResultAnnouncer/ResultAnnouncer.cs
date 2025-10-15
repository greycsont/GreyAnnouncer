using System.IO;
using System.Linq;
using System.Collections.Generic;

using GreyAnnouncer.AnnouncerAPI;
using GreyAnnouncer.PluginConfiguratorGUI;

namespace GreyAnnouncer.ResultAnnouncer;

public static class ResultAnnouncer
{
    private static readonly Dictionary<string, string> _displayNameMapping = new Dictionary<string, string>{   //used only for creating json
        
    };

    private static readonly AudioAnnouncer _announcer = new AudioAnnouncer();
    private static readonly string _jsonName = "ResultAnnouncer.json";
    private static readonly string _pageTitle = "Result Announcer";

    public static void Initialize()
    {
        _announcer.Initialize(
            new AudioLoader(InstanceConfig.audioFolderPath.Value),
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
        _announcer.UpdateAudioPath(InstanceConfig.audioFolderPath.Value);
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
