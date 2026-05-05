using System.Collections.Generic;

using GreyAnnouncer.AnnouncerCore;

namespace GreyAnnouncer.FinalRankAnnouncer;

[EntryPoint]
public static class FinalRankAnnouncer
{
    private static readonly List<string> category = new List<string>(){   //used only for creating JSON
        "D",
        "C",
        "B",
        "A",
        "S",
        "P"
    };

    private static AudioAnnouncer _announcer;
    public const string title = "FinalRankAnnouncer";

    [EntryPoint]
    public static void Initialize()
    {
        _announcer = new AudioAnnouncer(
            audioLoader: new AudioLoader(),
            cooldownManager: new CooldownManager(category.ToArray()),
            configManager: new JsonConfigManager(),
            category: category,
            title: title,
            defaultPackConfigPath: "spzeta"
        );
    }

    public static void PlayRankSound(int rank) 
        => _ = _announcer?.PlayAudioViaCategory(category[rank]);
}
