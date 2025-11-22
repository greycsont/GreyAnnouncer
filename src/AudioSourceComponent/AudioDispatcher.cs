using UnityEngine;

namespace GreyAnnouncer.AudioSourceComponent;

public static class AudioDispatcher
{
    public static void SendClipToAudioSource(Sound sound,
                                             int flag)
    {
        switch (flag)
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
