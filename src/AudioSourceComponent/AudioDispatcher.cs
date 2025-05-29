using UnityEngine;

namespace GreyAnnouncer.AudioSourceComponent;

public static class AudioDispatcher
{
    public static void SendClipToAudioSource(AudioClip clip, 
                                             AudioSourceSetting audioSourceConfig, 
                                             int flag)
    {
        switch (flag)
        {
            case 0:
                SoloAudioSource.Instance.PlayOneShot(clip, audioSourceConfig);
                break;
            case 1:
                AudioSourcePool.Instance.PlayOneShot(clip, audioSourceConfig);
                break;
            default:
                SoloAudioSource.Instance.PlayOneShot(clip, audioSourceConfig);
                break;
        }
    }
}
