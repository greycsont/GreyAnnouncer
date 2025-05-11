using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace greycsont.GreyAnnouncer;

public static class RankAnnouncerV2
{
    private static readonly string[]              m_rankCategory = {
        "D",
        "C", 
        "B", 
        "A", 
        "S", 
        "SS", 
        "SSS", 
        "U" 
    };

    private static readonly AudioAnnouncer       m_announcer     = new AudioAnnouncer();
    private static readonly string               m_jsonName      = "rankSettings.json";
    private static readonly string               m_pageTitle     = "Style Announcer";
    private static          AnnouncerJsonSetting m_jsonSetting;

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
            Plugin.log.LogError($"Invalid rank index: {rank}");
            return;
        }
        m_announcer.PlayAudioViaIndex(rank);
    }

    private static void SubscribeAnnouncerManager()
    {
        AnnouncerManager.reloadAnnouncer     += ReloadAudio;
        AnnouncerManager.resetCooldown       += ResetCooldowns;
        AnnouncerManager.clearAudioClipCache += ClearAudioClipCache;
        AnnouncerManager.updateAnnouncerPath += UpdateAudioPath;
    }


    #region Subscription
    private static void ReloadAudio()
    {
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
                AudioFiles = new List<string> { cat }
            }
        );
        m_jsonSetting = new AnnouncerJsonSetting { CategoryAudioMap = audioDict };
        JsonManager.CreateJson(m_jsonName, m_jsonSetting);
    }

    public static void UpdateJson(AnnouncerJsonSetting jsonSetting)
    {
        m_jsonSetting = jsonSetting;
        m_announcer.UpdateJsonSetting(m_jsonSetting);
        JsonManager.WriteJson(m_jsonName, m_jsonSetting);
    }
    #endregion

    private static void PluginConfigPanelInitialization(string announcerName, AnnouncerJsonSetting jsonSetting)
    {
        ReflectionManager.LoadByReflection(
            "greycsont.GreyAnnouncer.RegisterAnnouncerPage", 
            "Build", 
            new object[]{announcerName, jsonSetting}
        );
    }
}
