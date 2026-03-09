
using System.Collections.Generic;

using GreyAnnouncer.AnnouncerAPI;


namespace GreyAnnouncer.FinalRankAnnouncer;

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
    private static readonly string _title = "ResultAnnouncer";
    private static readonly string _GUID = "com.greycsont.resultannouncer";

    public static void Initialize()
    {
        _announcer = new AudioAnnouncer(
            new AudioLoader(),
            new CooldownManager(category.ToArray()),
            category,
            _title,
            "spzeta",
            _GUID
        );
    }

    public static void PlayRankSound(int rank) 
        => _ = _announcer.PlayAudioViaCategory(category[rank]);
}
