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
                SoloAudioSource.Instance.Play(clip, audioSourceConfig);
                break;
            case 1:
                //AudioSourcePool.Instance.PlayOneShot(clip, audioSourceConfig);
                SoloAudioSource.Instance.PlayOneShot(clip, audioSourceConfig);
                break;
            default:
                SoloAudioSource.Instance.Play(clip, audioSourceConfig);
                break;
        }
    }
}
