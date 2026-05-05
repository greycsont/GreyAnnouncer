using System.Collections.Generic;

using GreyAnnouncer.AnnouncerCore;

namespace GreyAnnouncer.Announcers;

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
    private static readonly List<string> source = new List<string>(){   //used only for creating JSON
        "time",
        "kills",
        "style",
        "total",
    };

    private static AudioAnnouncer _announcer;
    public const string title = "FinalRankAnnouncer";
    
    /*
    public static void Initialize()
    {
        _announcer = new AudioAnnouncer(
            audioLoader: new AudioLoader(),
            cooldownManager: new CooldownManager(category.ToArray()),
            configManager: new JsonConfigManager(),
            source: source,
            category: category,
            title: title,
            defaultPackConfigPath: "spzeta"
        );
    }*/
}
