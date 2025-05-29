/*
 * Warning : this module is not done yet
 * Please check InstanceConfig to see 
 * how it's configured.
 *
 */



namespace GreyAnnouncer;

public static class GlobalSetting
{
    public static float sharedPlayCooldown { get; set; }
    public static float individualPlayCooldown { get; set;}
    public static float audioSourceVolume { get; set; }
    public static bool isLowPassFilterEnabled { get; set; }
    public static int audioPlayOptions { get; set; }
    public static int audioLoadingOptions { get; set; }
    public static bool isAudioRandomizationEnabled { get; set; }
    public static bool isFFmpegSupportEnabled { get; set; }
}
