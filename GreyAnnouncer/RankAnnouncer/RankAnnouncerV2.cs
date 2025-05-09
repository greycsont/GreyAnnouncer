using greycsont.GreyAnnouncer;


namespace rankAnnouncerV2;

public static class RankAnnouncerV2
{
    public static readonly string[]       rankCategory = {
        "D",
        "C", 
        "B", 
        "A", 
        "S", 
        "SS", 
        "SSS", 
        "U" 
    };

    private static readonly AudioAnnouncer m_announcer    = new AudioAnnouncer();

    public static void Initialize()
    {
        m_announcer.Initialize(
            "RankAnnouncer",
            rankCategory,
            "rankSettings.json",
            InstanceConfig.audioFolderPath.Value
        );
    }
    
    public static void PlayRankSound(int rank)
    {
        if (rank < 0 || rank >= rankCategory.Length)
        {
            Plugin.log.LogError($"Invalid rank index: {rank}");
            return;
        }
        m_announcer.PlayAudioViaIndex(rank);
    }

    public static void ReloadRankSounds()
    {
        m_announcer.ReloadAudio();
    }

    public static void UpdateRankAudioPath(string newPath)
    {
        m_announcer.UpdateAudioPath(newPath);
    }

    public static void ResetCooldowns()
    {
        m_announcer.ResetCooldown();
    }
}
