using UnityEngine;

namespace GreyAnnouncer.AudioSourceComponent;

public static class AudioDispatcher
{
    public static void SendClipToAudioSource(AudioClip clip, 
                                             AudioSourceSetting audioSourceConfig, 
                                             int flag,
                                             float volumeMultiplier = 1f)
    {
        switch (flag)
        {
            case 0:
                SoloAudioSource.Instance.Play(clip, audioSourceConfig, volumeMultiplier);
                break;
            case 1:
                //AudioSourcePool.Instance.PlayOneShot(clip, audioSourceConfig);
                SoloAudioSource.Instance.PlayOneShot(clip, audioSourceConfig, volumeMultiplier);
                break;
            default:
                SoloAudioSource.Instance.Play(clip, audioSourceConfig, volumeMultiplier);
                break;
        }
    }
}
