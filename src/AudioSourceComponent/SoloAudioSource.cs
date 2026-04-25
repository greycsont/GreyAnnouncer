using UnityEngine;

using GreyAnnouncer.Config;
using UnityEngine.Audio;

namespace GreyAnnouncer.AudioSourceComponent;

public sealed class SoloAudioSource : MonoBehaviour
{
    private AudioSource _audioSource
    {
        get
        {
            if (field == null)
                field = gameObject.GetComponent<AudioSource>() 
                    ?? gameObject.AddComponent<AudioSource>();
            return field;
        }

        set;
    }

    public static SoloAudioSource Instance
    {
        get
        {
            if (field == null)
            {
                var obj = new GameObject("GreyAnnouncer_SoloAudioSource");
                obj.transform.SetParent(MonoSingleton<CameraController>.Instance.transform);
                obj.transform.localPosition = Vector3.zero;
                field = obj.AddComponent<SoloAudioSource>();
            }
            return field;
        }
    }

    #region Public API
    public void Play(Sound sound)
    {
        _audioSource.Stop();

        _audioSource.ConfigureAndPlayAudioSource(sound);
    }

    public void PlayOneShot(Sound sound)
    {
        _audioSource.ConfigureAndPlayAudioSource(sound);
    }


    public void AddAudioLowPassFilter()
        => _audioSource.AddLowPassFilter();

    public void RemoveAudioLowPassFilter()
        => _audioSource.RemoveLowPassFilter();

    public void UpdateSoloAudioSourceVolume(float targetVolume, float duration = 0.35f)
    {
        if (_audioSource != null)
            _audioSource.FadeVolume(duration, targetVolume);
    }

    public void StopAudioSource()
    {
        _audioSource.Stop();
        Destroy(_audioSource.clip);
    }
    #endregion
}
