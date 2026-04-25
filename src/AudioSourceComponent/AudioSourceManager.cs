using UnityEngine;
using System.Collections;
using System;

namespace GreyAnnouncer.AudioSourceComponent;

public static class AudioSourceManager
{
    public static void StopAllAudioSource()
        => SoloAudioSource.Instance.StopAudioSource();

}
