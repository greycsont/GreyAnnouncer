

namespace GreyAnnouncer.Commands;


public static class CommandPreset
{
    public static void ExecuteCommand(params string[] args)
    {
        if (args.Length == 0)
            return;

        string fullCommand = string.Join(" ", args);
        ProcessInput(fullCommand);
    }

    private static void ProcessInput(string input)
        => GameConsole.Console.Instance.ProcessInput(input);

    #region Presets
    public static void SetVolume(float volume)
        => ExecuteCommand("grey", "set", "audiosourcevolume", volume.ToString());

    public static void SetAudioFolderPath(string path)
        => ExecuteCommand("grey", "set", "audiofolderpath", path);

    public static void EnableFFmpegSupport(bool enabled)
        => ExecuteCommand("grey", "set", "ffmpegenabled", enabled.ToString());

    public static void EnableLowPassFilter(bool enabled)
        => ExecuteCommand("grey", "set", "lowpassenabled", enabled.ToString());

    public static void OpenAudioFolder()
        => ExecuteCommand("grey", "util", "openaudiofolder");

    public static void ReloadAnnouncers()
        => ExecuteCommand("grey", "util", "reloadannouncers");
    #endregion
}