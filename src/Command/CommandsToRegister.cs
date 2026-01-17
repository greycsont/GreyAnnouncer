using GameConsole;
using GameConsole.CommandTree;
using plog;
using System.Linq;

using GreyAnnouncer.AnnouncerAPI;
using GreyAnnouncer.Config;
using GreyAnnouncer.AudioSourceComponent;

namespace GreyAnnouncer.Commands;

public sealed class CommandsToRegister(Console con) : CommandRoot(con), IConsoleLogger
{

    public override string Name => "grey";
    public override string Description => "tons of setting";

    public override Branch BuildTree(Console con)
        => Branch(Name,
                      GetMainSettingBranches(),
                      GetUtilBranches(),
                      GetAnnouncerBranches(),
                      GetTestBranches()
                      );

    private Branch GetMainSettingBranches()
        => Branch("mainsetting",
                      Branch("set",
                             Leaf<float>("audiosourcevolume", vol => BepInExConfig.audioSourceVolume.Value = vol),
                             Leaf<bool>("ffmpegenabled", enabled => BepInExConfig.isFFmpegSupportEnabled.Value = enabled),
                             Leaf<bool>("lowpassenabled", enabled => BepInExConfig.isLowPassFilterEnabled.Value = enabled)
                             ),
                      Branch("get",
                             Leaf("audiosourcevolume", () => Log.Info($"Audio Source Volume: {BepInExConfig.audioSourceVolume.Value}")),
                             Leaf("ffmpegenabled", () => Log.Info($"FFmpeg Support Enabled: {BepInExConfig.isFFmpegSupportEnabled.Value}")),
                             Leaf("lowpassenabled", () => Log.Info($"Under Water Low Pass Filter Enabled: {BepInExConfig.isLowPassFilterEnabled.Value}"))
                             )
                      );

    private Branch GetAnnouncerBranches()
    {
        var AnnouncersBranches = AnnouncerManager.GetAllAnnouncers()
            .Select(a => 
            {
                // 生成 category 分支
                var categoryBranches = a._announcerConfig.CategoryAudioMap
                    .Select(kvp => 
                    {
                        string categoryName = kvp.Key;
                        var categoryAudio = kvp.Value;
                        
                        return Branch(categoryName,
                                      Leaf<bool>("enabled", enabled => categoryAudio.Enabled = enabled),
                                      Leaf<float>("volume", vol => categoryAudio.VolumeMultiplier = vol),
                                      Leaf<float>("cooldown", cd => categoryAudio.Cooldown = cd)
                                    );
                    })
                    .ToArray();

                return Branch(a.title,
                              Leaf("reload", () => a.ReloadAudio()),
                              Leaf("getsetting", () => ObjectTreePrinter.GetTreeString(a._announcerConfig)),
                              //Leaf("enable", () => a.Enable()),
                              //Leaf("disable", () => a.Disable()),
                              Leaf<string>("audiopath", path => a._announcerConfig.AudioPath = path),
                              Leaf<bool>("randomize", enabled => a._announcerConfig.RandomizeAudioOnPlay = enabled),
                              Branch("category", categoryBranches)
                );
            })
            .ToArray();

            return Branch("announcers",
                          AnnouncersBranches
                          );
    }

    public Branch GetUtilBranches()
        => Branch("util",
                      Leaf("reloadannouncers", () => AnnouncerManager.ReloadAllAnnouncers()),
                      Leaf("stopallaudiosources", () => AudioSourceManager.StopAllAudioSource())
                      );

    private Branch GetTestBranches()
        => null;

    public Logger Log { get; } = new("grey");
}