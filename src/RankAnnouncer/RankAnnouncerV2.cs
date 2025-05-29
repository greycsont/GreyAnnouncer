using System.IO;
using System.Linq;
using System.Collections.Generic;

using GreyAnnouncer.AnnouncerAPI;
using GreyAnnouncer.AudioLoading;

namespace GreyAnnouncer.RankAnnouncer;

public static class RankAnnouncerV2
{
    private static readonly string[] _rankCategory = {
        "D",
        "C",
        "B",
        "A",
        "S",
        "SS",
        "SSS",
        "U"
    };

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

    private static readonly AudioAnnouncer _announcer = new AudioAnnouncer();
    private static readonly string _jsonName = "RankAnnouncer.json";
    private static readonly string _pageTitle = "Rank Announcer";
    internal static AnnouncerJsonSetting _jsonSetting;

    public static void Initialize()
    {
        JsonInitialization();

        _announcer.Initialize(
            _jsonSetting,
            new AudioLoader(InstanceConfig.audioFolderPath.Value, _jsonSetting),
            new CooldownManager(_jsonSetting.CategoryAudioMap.Keys.ToArray())
        );

        SubscribeAnnouncerManager();
        PluginConfigPanelInitialization(_pageTitle, _jsonSetting);
    }

    public static void PlayRankSound(int rank)
    {
        if (rank < 0 || rank >= _rankCategory.Length)
        {
            LogManager.LogError($"Invalid rank index: {rank}");
            return;
        }
        _announcer.PlayAudioViaIndex(rank);
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
        JsonInitialization();
        UpdateAnnouncerJson();
        _announcer.ReloadAudio(_jsonSetting);

        PluginDependencies.LoadIfPluginExists(
            PluginDependencies.PLUGINCONFIGURATOR_GUID,
            "RegisterRankAnnouncerPage",
            () => ReflectionManager.LoadByReflection(
                      "GreyAnnouncer.RankAnnouncer.RegisterRankAnnouncerPage", 
                      "UpdateJsonSetting", 
                      new object[] { _jsonSetting }
                  )
        );
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


    #region Json
    private static void JsonInitialization()
    {
        if (CheckDoesJsonExists() == false) CreateJson();
        _jsonSetting = JsonManager.ReadJson<AnnouncerJsonSetting>(_jsonName);
    }

    private static bool CheckDoesJsonExists()
    {
        return File.Exists(PathManager.GetCurrentPluginPath(_jsonName));
    }

    private static void CreateJson()
    {
        var audioDict = _rankCategory.ToDictionary(
            cat => cat,
            cat => new CategoryAudioSetting
            {
                Enabled = true,
                DisplayName = _displayNameMapping.TryGetValue(cat, out var name) ? name : cat,
                Pitch = 1f,
                AudioFiles = new List<string> { cat }
            }
        );
        _jsonSetting = new AnnouncerJsonSetting { CategoryAudioMap = audioDict };
        JsonManager.CreateJson(_jsonName, _jsonSetting);
    }

    public static void UpdateJson(AnnouncerJsonSetting jsonSetting)
    {
        _jsonSetting = jsonSetting;
        UpdateAnnouncerJson();
    }
    #endregion

    private static void PluginConfigPanelInitialization(string announcerName, 
                                                        AnnouncerJsonSetting jsonSetting)
    {
        PluginDependencies.LoadIfPluginExists(
            PluginDependencies.PLUGINCONFIGURATOR_GUID,
            "RegisterRankAnnouncerPage",
            () => ReflectionManager.LoadByReflection(
                      "GreyAnnouncer.RankAnnouncer.RegisterRankAnnouncerPage", 
                      "Build", 
                      new object[] { announcerName, jsonSetting })
        );
    }

    private static void UpdateAnnouncerJson()
    {
        _announcer.UpdateJsonSetting(_jsonSetting);
        JsonManager.WriteJson(_jsonName, _jsonSetting);
    }
}
