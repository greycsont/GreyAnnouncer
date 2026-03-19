using System.Collections.Generic;

using GreyAnnouncer.AnnouncerAPI;

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

    [EntryPoint]
    public static void Initialize()
    {
        /*_announcer = new AudioAnnouncer(
            new AudioLoader(),
            new CooldownManager(category.ToArray()),
            category,
            _title,
            "spzeta"
        );*/
    }

    public static void PlayRankSound(int rank) 
        => _ = _announcer?.PlayAudioViaCategory(category[rank]);
}
