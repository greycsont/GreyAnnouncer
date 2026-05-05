using System.Collections.Generic;

using GreyAnnouncer.AnnouncerCore;

namespace GreyAnnouncer.RankAnnouncer;

[EntryPoint]
public static class RankAnnouncer
{
    private static readonly List<string> category = new List<string>(){   //used only for creating JSON
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
    public static readonly string title = "RankAnnouncer";

    [EntryPoint]
    public static void Initialize()
    {
        _announcer = new AudioAnnouncer(
            audioLoader: new AudioLoader(),
            cooldownManager: new CooldownManager(category.ToArray()),
            configManager: new JsonConfigManager(),
            source : category,
            category: category,
            title: title,
            defaultPackConfigPath: "greythroat"
        );
        EventTriggers.StyleHUD.OnTriggered += PlayRankSound;
    }

    private static void PlayRankSound(int rank)
        => _ = _announcer?.PlayAudioViaCategory(category[rank]);
}
