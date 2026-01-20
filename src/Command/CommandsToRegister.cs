using GameConsole;
using GameConsole.CommandTree;
using plog;
using System.Linq;

using GreyAnnouncer.AnnouncerAPI;
using GreyAnnouncer.Config;
using GreyAnnouncer.AudioSourceComponent;
using GreyAnnouncer.Util;

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
            // audiosourcevolume
            Branch("audiosourcevolume",
                Branch("get", Leaf(() =>
                    Log.Info($"Audio Source Volume: {BepInExConfig.audioSourceVolume.Value}"))
                ),
                Branch("set", Leaf<float>(val =>
                {
                    BepInExConfig.audioSourceVolume.Value = val;
                    Log.Info($"Audio Source Volume set to {val}");
                }))
            ),

            // ffmpegenabled
            Branch("ffmpegenabled",
                Branch("get", Leaf(() =>
                    Log.Info($"FFmpeg Support Enabled: {BepInExConfig.isFFmpegSupportEnabled.Value}"))
                ),
                Branch("set", Leaf<bool>(val =>
                {
                    BepInExConfig.isFFmpegSupportEnabled.Value = val;
                    Log.Info($"FFmpeg Support Enabled set to {val}");
                }))
            ),

            // lowpassenabled
            Branch("lowpassenabled",
                Branch("get", Leaf(() =>
                    Log.Info($"Under Water Low Pass Filter Enabled: {BepInExConfig.isLowPassFilterEnabled.Value}"))
                ),
                Branch("set", Leaf<bool>(val =>
                {
                    BepInExConfig.isLowPassFilterEnabled.Value = val;
                    Log.Info($"Under Water Low Pass Filter Enabled set to {val}");
                }))
            )
        );


    private Branch GetAnnouncerBranches()
    {
        return Branch("announcers",
            AnnouncerManager.GetAllAnnouncers().Select(a =>
            {
                return Branch(a.title,

                    Branch("randomize",
                        Branch("get",
                            Leaf(() => Log.Info($"RandomizeAudioOnPlay: {a.announcerConfig.RandomizeAudioOnPlay}"))
                        ),
                        Branch("set",
                            Leaf<bool>(val =>
                            {
                                a.announcerConfig.RandomizeAudioOnPlay = val;
                                Log.Info($"Set RandomizeAudioOnPlay to {val}");
                            })
                        )
                    ),

                    Branch("category",
                        a.announcerConfig.CategorySetting.Select(kvp =>
                        {
                            var categoryName = kvp.Key;
                            var category = kvp.Value;

                            return Branch(categoryName,
                                Branch("get",
                                    Leaf("enabled", () => Log.Info($"Enabled: {category.Enabled}")),
                                    Leaf("volume", () => Log.Info($"VolumeMultiplier: {category.VolumeMultiplier}")),
                                    Leaf("cooldown", () => Log.Info($"Cooldown: {category.Cooldown}")),
                                    Leaf("audiofiles", () => 
                                        Log.Info($"AudioFiles: {string.Join(", ", category.AudioFiles)}"))
                                ),
                                Branch("set",
                                    Leaf<bool>("enabled", val => category.Enabled = val),
                                    Leaf<float>("volume", val => category.VolumeMultiplier = val),
                                    Leaf<float>("cooldown", val => category.Cooldown = val),
                                    Leaf<string[]>("audiofiles", files =>
                                    {
                                        category.AudioFiles.Clear();
                                        category.AudioFiles.AddRange(files);
                                        Log.Info($"AudioFiles set: {string.Join(", ", files)}");
                                    })
                                )
                            );
                        }).ToArray()
                    ),
                    Leaf("editexternal", () => a.EditExternally()),
                    Leaf("reload", () => a.ReloadAudio())
                );
            }).ToArray()
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