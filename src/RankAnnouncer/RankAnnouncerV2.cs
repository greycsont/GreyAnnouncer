using System.IO;
using System.Linq;
using System.Collections.Generic;

using GreyAnnouncer.AnnouncerAPI;
using GreyAnnouncer.FrontEnd;

namespace GreyAnnouncer.RankAnnouncer;

public static class RankAnnouncer
{
    private static readonly List<string> category = new List<string>(){   //used only for creating json
        "Destruction",
        "Chaotic",
        "Brutal",
        "Anarchic",
        "Supreme",
        "SSadistic",
        "SSShitstorm",
        "ULTRAKILL"
    };

    private static AudioAnnouncer _announcer;
    private static readonly string _title = "RankAnnouncer";

    public static void Initialize()
    {
        _announcer = new AudioAnnouncer(
            new AudioLoader(),
            new CooldownManager(category.ToArray()),
            category,
            _title
        );
    }

    public static void PlayRankSound(int rank)
       => _ = _announcer.PlayAudioViaIndex(rank);
}
