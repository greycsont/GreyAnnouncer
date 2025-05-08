using greycsont.GreyAnnouncer;


namespace rankAnnouncerV2;

public static class RankAnnouncerV2
{
    private static readonly string[]       m_rankCategory = {
        "D",
        "C", 
        "B", 
        "A", 
        "S", 
        "SS", 
        "SSS", 
        "U" 
    };

    private static readonly AudioAnnouncer _announcer     = new AudioAnnouncer();

    public static void Initialize()
    {
        _announcer.Initialize(
            "RankAnnouncer",
            m_rankCategory,
            "rankSettings.json",
            InstanceConfig.audioFolderPath.Value
        );
    }
    
    public static void PlayRankSound(int rank)
    {
        if (rank < 0 || rank >= m_rankCategory.Length)
        {
            Plugin.log.LogError($"Invalid rank index: {rank}");
            return;
        }
        _announcer.PlayAudio(rank);
    }

    public static void ReloadRankSounds()
    {
        _announcer.ReloadAudio();
    }

    public static void UpdateRankAudioPath(string newPath)
    {
        _announcer.UpdateAudioPath(newPath);
    }

    public static void ResetCooldowns()
    {
        _announcer.ResetCooldown();
    }
}
