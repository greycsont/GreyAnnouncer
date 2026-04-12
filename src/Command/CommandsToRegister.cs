using GameConsole;
using GameConsole.CommandTree;
using plog;
using System.Linq;

using GreyAnnouncer.AnnouncerCore;
using GreyAnnouncer.Config;
using GreyAnnouncer.AudioSourceComponent;
using GreyAnnouncer.AudioLoading;
using GreyAnnouncer.FrontEnd;

namespace GreyAnnouncer.Commands;

public sealed class CommandsToRegister(Console con) : CommandRoot(con), IConsoleLogger
{
    public override string Name => "grey";
    public override string Description => "";

    public override Branch BuildTree(Console con)
        => Branch(Name,
                      GetMainSettingBranches(),
                      GetUtilBranches(),
                      GetAnnouncerBranches(),
                      GetTestBranches(),
                      Leaf("help", () =>
                        {
                            Log.Info("=== GreyAnnouncer Command Reference ===");
                            Log.Info("");
                            Log.Info("[Main Settings] grey m");
                            Log.Info("  audiosourcevolume get/set <float>   - Global announcer volume (0.0 ~ 1.0)");
                            Log.Info("  ffmpegenabled     get/set <bool>    - Enable FFmpeg support for non-Unity audio formats");
                            Log.Info("  lowpassenabled    get/set <bool>    - Enable underwater low-pass filter effect");
                            Log.Info("  announcerspath    get/set <string>  - Folder path where announcer packs are stored");
                            Log.Info("");
                            Log.Info("[Announcer] grey a <announcer>");
                            Log.Info("  randomize         get/set <bool>    - Randomize category selection on playback");
                            Log.Info("  category <name>   get/set");
                            Log.Info("    enabled         get/set <bool>    - Enable or disable this category");
                            Log.Info("    volume          get/set <float>   - Volume multiplier for this category");
                            Log.Info("    cooldown        get/set <float>   - Cooldown in seconds between plays");
                            Log.Info("    audiofiles      get/set <files>   - Audio files assigned to this category");
                            Log.Info("  editexternal                        - Open announcer folder in file explorer");
                            Log.Info("  reload                              - Reload announcer config from disk");
                            Log.Info("  structure                           - Print announcer object tree");
                            Log.Info("");
                            Log.Info("[Utilities] grey u");
                            Log.Info("  reloadannouncers                   - Reload all announcers");
                            Log.Info("  stopallaudiosources                - Stop all active audio sources");
                            Log.Info("");
                            Log.Info("[Test] grey t");
                        })
                      );

    private Branch GetMainSettingBranches()
        => Branch("m",
            // audiosourcevolume
            Branch("audiosourcevolume",
                Branch("get", Leaf(() =>
                    Log.Info($"Audio Source Volume: {Setting.audioSourceVolume}"))
                ),
                Branch("set", Leaf<float>(val =>
                {
                    Setting.audioSourceVolume = val;
                    Log.Info($"Audio Source Volume set to {val}");
                }))
            ),

            // ffmpegenabled
            Branch("ffmpegenabled",
                Branch("get", Leaf(() =>
                    Log.Info($"FFmpeg Support Enabled: {Setting.isFFmpegSupportEnabled}"))
                ),
                Branch("set", Leaf<bool>(val =>
                {
                    Setting.isFFmpegSupportEnabled = val;
                    Log.Info($"FFmpeg Support Enabled set to {val}");
                }))
            ),

            // lowpassenabled
            Branch("lowpassenabled",
                Branch("get", Leaf(() =>
                    Log.Info($"Under Water Low Pass Filter Enabled: {Setting.isLowPassFilterEnabled}"))
                ),
                Branch("set", Leaf<bool>(val =>
                {
                    Setting.isLowPassFilterEnabled = val;
                    Log.Info($"Under Water Low Pass Filter Enabled set to {val}");
                }))
            ),

            Branch("announcerspath",
                Branch("get", Leaf(() =>
                    Log.Info($"Current Announcers Path: {Setting.announcersPath}"))
                ),
                Branch("set", Leaf<string>(val =>
                {
                    Log.Info($"This command doesn't have any effect yet");
                    Log.Info($"Announcers Path set to {val}");
                }))
            )
        );


    private Branch GetAnnouncerBranches()
    {
        return Branch("a",
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
                    Leaf("reload", () => a.ReloadPack())
                );
            }).ToArray()
        );
    }


    public Branch GetUtilBranches()
        => Branch("u",
                      Leaf("reloadannouncers", () => AnnouncerManager.ReloadAllAnnouncers()),
                      Leaf("stopallaudiosources", () => AudioSourceManager.StopAllAudioSource())
                      );

    private Branch GetTestBranches()
        => Branch("t",
            Leaf<string>("ffmpeg", async path =>
            {
                Log.Info($"Testing FFmpeg load: {path}");
                try
                {
                    var clip = await FFmpegSupport.DecodeAndLoadViaFFmpeg(path);
                    if (clip != null)
                        Log.Info($"OK — loaded clip: {clip.length:F2}s, ch={clip.channels}, rate={clip.frequency}");
                    else
                        Log.Warning("FFmpeg returned null clip.");
                }
                catch (System.Exception ex)
                {
                    Log.Error($"FFmpeg failed: {ex.Message}");
                }
            })
        );

    public Logger Log { get; } = new("grey");
}