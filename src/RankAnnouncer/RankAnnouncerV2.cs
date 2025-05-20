using System.IO;
using System.Linq;
using System.Collections.Generic;

using GreyAnnouncer.AnnouncerAPI;

namespace GreyAnnouncer.RankAnnouncer;

public static class RankAnnouncerV2
{
    private static readonly string[] m_rankCategory = {
        "D",
        "C",
        "B",
        "A",
        "S",
        "SS",
        "SSS",
        "U"
    };

    private static readonly Dictionary<string, string> m_displayNameMapping = new Dictionary<string, string>{   //used only for creating json
        {"D", "Destruction"},
        {"C", "Chaotic"},
        {"B", "Brutal"},
        {"A", "Anarchic"},
        {"S", "Supreme"},
        {"SS", "SSadistic"},
        {"SSS", "SSShitstorm"},
        {"U",   "ULTRAKILL"}
    };

    private static readonly AudioAnnouncer m_announcer = new AudioAnnouncer();
    private static readonly string m_jsonName = "RankAnnouncer.json";
    private static readonly string m_pageTitle = "Rank Announcer";
    internal static AnnouncerJsonSetting m_jsonSetting;

    public static void Initialize()
    {
        JsonInitialization();

        m_announcer.Initialize(
            m_rankCategory,
            m_jsonSetting,
            InstanceConfig.audioFolderPath.Value
        );

        SubscribeAnnouncerManager();
        PluginConfigPanelInitialization(m_pageTitle, m_jsonSetting);
    }

    public static void PlayRankSound(int rank)
    {
        if (rank < 0 || rank >= m_rankCategory.Length)
        {
            LogManager.LogError($"Invalid rank index: {rank}");
            return;
        }
        m_announcer.PlayAudioViaIndex(rank);
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
        m_announcer.ReloadAudio(m_jsonSetting);
    }

    private static void UpdateAudioPath()
    {
        m_announcer.UpdateAudioPath(InstanceConfig.audioFolderPath.Value);
    }

    private static void ResetCooldowns()
    {
        m_announcer.ResetCooldown();
    }

    private static void ClearAudioClipCache()
    {
        m_announcer.ClearAudioClipsCache();
    }
    #endregion


    #region Json
    private static void JsonInitialization()
    {
        if (CheckDoesJsonExists() == false) CreateJson();
        m_jsonSetting = JsonManager.ReadJson<AnnouncerJsonSetting>(m_jsonName);
    }

    private static bool CheckDoesJsonExists()
    {
        return File.Exists(PathManager.GetCurrentPluginPath(m_jsonName));
    }

    private static void CreateJson()
    {
        var audioDict = m_rankCategory.ToDictionary(
            cat => cat,
            cat => new CategoryAudioSetting
            {
                Enabled = true,
                DisplayName = m_displayNameMapping.TryGetValue(cat, out var name) ? name : cat,
                AudioFiles = new List<string> { cat }
            }
        );
        m_jsonSetting = new AnnouncerJsonSetting { CategoryAudioMap = audioDict };
        JsonManager.CreateJson(m_jsonName, m_jsonSetting);
    }

    public static void UpdateJson(AnnouncerJsonSetting jsonSetting)
    {
        m_jsonSetting = jsonSetting;
        UpdateAnnouncerJson();
    }
    #endregion

    private static void PluginConfigPanelInitialization(string announcerName, AnnouncerJsonSetting jsonSetting)
    {
        PluginDependencies.LoadIfPluginExists(
            PluginDependencies.PLUGINCONFIGURATOR_GUID,
            "RegisterRankAnnouncerPage",
            () => ReflectionManager.LoadByReflection("GreyAnnouncer.RegisterRankAnnouncerPage", "Build", new object[] { announcerName, jsonSetting })
        );
    }

    private static void UpdateAnnouncerJson()
    {
        m_announcer.UpdateJsonSetting(m_jsonSetting);
        JsonManager.WriteJson(m_jsonName, m_jsonSetting);
    }
}
