

using System.Collections.Generic;

using GreyAnnouncer.AnnouncerAPI;

namespace GreyAnnouncer.NewMovementAnnouncer;

public static class NewMovementAnnouncer
{
    private static readonly List<string> category = new List<string>(){   //used only for creating json
        "Death"
    };

    private static AudioAnnouncer _announcer;
    private static readonly string _title = "NewMovementAnnouncer";

    public static void Initialize()
    {
        _announcer = new AudioAnnouncer(
            new AudioLoader(),
            new CooldownManager(category.ToArray()),
            category,
            _title,
            "greythroat"
        );
    }

    public static void PlaySoundViaIndex(int index)
       => _ = _announcer.PlayAudioViaCategory(category[index]);
}