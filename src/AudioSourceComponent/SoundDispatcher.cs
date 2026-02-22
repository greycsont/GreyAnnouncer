
using GreyAnnouncer.Config;

namespace GreyAnnouncer.AudioSourceComponent;

public static class SoundDispatcher
{
    public static void SendClipToAudioSource(Sound sound)
    {
        switch (BepInExConfig.audioPlayOptions.Value)
        {
            case 0:
                SoloAudioSource.Instance.Play(sound);
                break;
            case 1:
                //AudioSourcePool.Instance.PlayOneShot(clip, audioSourceConfig);
                SoloAudioSource.Instance.PlayOneShot(sound);
                break;
            default:
                SoloAudioSource.Instance.Play(sound);
                break;
        }
    }
}
