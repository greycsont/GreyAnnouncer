using UnityEngine;

namespace GreyAnnouncer.AudioSourceComponent;

public static class AudioDispatcher
{
    public static void SendClipToAudioSource(AudioClip clip, AudioSourceSetting m_audioSourceConfig)
    {
        switch (InstanceConfig.audioPlayOptions.Value)
        {
            case 0:
                SoloAudioSource.Instance.PlayOneShot(clip, m_audioSourceConfig);
                break;
            case 1:
                AudioSourcePool.Instance.PlayOneShot(clip, m_audioSourceConfig);
                break;
            default:
                SoloAudioSource.Instance.PlayOneShot(clip, m_audioSourceConfig);
                break;
        }
    }
}
