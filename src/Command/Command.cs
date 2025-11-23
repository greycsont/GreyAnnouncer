using GameConsole;
using GameConsole.CommandTree;
using plog;

using GreyAnnouncer.AnnouncerAPI;

namespace GreyAnnouncer.Commands;

public sealed class GreyAnnouncerCommand(Console con) : CommandRoot(con), IConsoleLogger
{

    public override string Name => "grey";
    public override string Description => "tons of setting";

    public override Branch BuildTree(Console con)
    {
        return Branch(Name,
                      Branch("set",
                             Leaf<float>("audiosourcevolume", vol => BepInExConfig.audioSourceVolume.Value = vol),

                             Leaf<bool>("ffmpegenabled", enabled => BepInExConfig.isFFmpegSupportEnabled.Value = enabled),
                             Leaf<bool>("lowpassenabled", enabled => BepInExConfig.isLowPassFilterEnabled.Value = enabled)
                             ),
                      Branch("get",
                             Leaf("audiosourcevolume", () => Log.Info($"Audio Source Volume: {BepInExConfig.audioSourceVolume.Value}")),
                             Leaf("ffmpegenabled", () => Log.Info($"FFmpeg Support Enabled: {BepInExConfig.isFFmpegSupportEnabled.Value}")),
                             Leaf("lowpassenabled", () => Log.Info($"Under Water Low Pass Filter Enabled: {BepInExConfig.isLowPassFilterEnabled.Value}"))
                             ),
                      Branch("util",
                             Leaf("reloadannouncers", () => AnnouncerManager.ReloadAllAnnouncers()),
                             Leaf("stopallaudiosources", () => AudioSourceManager.StopAllAudioSource())
                             )
                      );
    }

    public Logger Log { get; } = new("grey");
}