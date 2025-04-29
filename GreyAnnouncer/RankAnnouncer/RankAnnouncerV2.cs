using System.ComponentModel;


using greycsont.GreyAnnouncer;

public static class RankAnnouncerV2
{
    private static readonly string[] RANK_NAMES = { "D", "C", "B", "A", "S", "SS", "SSS", "U" };
    private static readonly AudioAnnouncer _announcer = new AudioAnnouncer();

    public static void Initialize()
    {
        _announcer.Initialize(
            RANK_NAMES,
            "rankSettings.json",
            InstanceConfig.AudioFolderPath.Value
        );
    }

    [Description("Parry balls of Maurice -> Hit Maurice -> AscendingRank() -> Postfix() -> PlaySound() -> CheckPlayValidation(), " +
                 "This bug will skip all the function before CheckPlayValidation(),  The try-catch has implemented in the fucntion")]
    public static void PlayRankSound(int rank)
    {
        if (rank < 0 || rank >= RANK_NAMES.Length)
        {
            Plugin.Log.LogError($"Invalid rank index: {rank}");
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
        _announcer.UpdateAudioPaths(newPath);
    }

    public static void ResetCooldowns()
    {
        _announcer.ResetTimerToZero();
    }
}
