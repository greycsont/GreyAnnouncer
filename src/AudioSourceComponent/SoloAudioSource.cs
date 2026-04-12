using UnityEngine;

using GreyAnnouncer.Config;
using UnityEngine.Audio;

namespace GreyAnnouncer.AudioSourceComponent;

public sealed class SoloAudioSource : MonoBehaviour
{
    private AudioSource _audioSource;

    private static SoloAudioSource _instance;

    public static SoloAudioSource Instance
    {
        get
        {
            if (_instance == null)
            {
                var obj = new GameObject("GreyAnnouncer_SoloAudioSource");
                obj.transform.SetParent(MonoSingleton<CameraController>.Instance.transform);
                obj.transform.localPosition = Vector3.zero;
                _instance = obj.AddComponent<SoloAudioSource>();
            }
            return _instance;
        }
    }

    #region Public API
    public void Play(Sound sound)
    {
        if (TryFindAudioSource() == false) return;

        _audioSource.Stop();

        ConfigureAndPlayAudioSource(sound, _audioSource);
    }

    public void PlayOneShot(Sound sound)
    {
        if (TryFindAudioSource() == false) return;

        ConfigureAndPlayAudioSource(sound, _audioSource);
    }

    private void ConfigureAndPlayAudioSource(Sound sound, AudioSource audioSource)
    {
        audioSource.SetSpatialBlend(Mathf.Clamp01(Setting.spatialBlend));
        audioSource.priority = sound.priority;
        audioSource.clip = sound.clip;
        audioSource = UnderwaterController_inWater_Instance.GetAudioSourceWithLowPassFilter(_audioSource);
        audioSource.volume = Setting.audioSourceVolume;
        audioSource.PlayOneShot(sound.clip, sound.volume, true);
    }

    public void AddAudioLowPassFilter()
        => _audioSource = AudioSourceManager.AddLowPassFilter(_audioSource);

    public void RemoveAudioLowPassFilter()
        => _audioSource = AudioSourceManager.RemoveLowPassFilter(_audioSource);

    public void UpdateSoloAudioSourceVolume(float targetVolume, float duration = 0.35f)
    {
        if (_audioSource != null)
            StartCoroutine(AudioSourceManager.FadeVolume(_audioSource, duration, targetVolume));
    }

    public void StopAudioSource()
    {
        _audioSource.Stop();
        Destroy(_audioSource.clip);
    }

    private bool TryFindAudioSource()
    {
        if (_audioSource == null)
            _audioSource = gameObject.AddComponent<AudioSource>();
        return _audioSource != null;
    }
    #endregion
}
