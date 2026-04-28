using UnityEngine;
using GreyAnnouncer.AudioClipLoad;
using System.Runtime.InteropServices;

namespace GreyAnnouncer.Tests;

public class StringExtensionTests
{
    [Theory]
    [InlineData("audio.wav",  AudioType.WAV)]
    [InlineData("audio.mp3",  AudioType.MPEG)]
    [InlineData("audio.ogg",  AudioType.OGGVORBIS)]
    [InlineData("audio.aac",  AudioType.ACC)]
    [InlineData("audio.aiff", AudioType.AIFF)]
    [InlineData("audio.aif",  AudioType.AIFF)]
    [InlineData("audio.aifc", AudioType.AIFF)]
    [InlineData("audio.mpeg", AudioType.MPEG)]
    [InlineData("audio.mpga", AudioType.MPEG)]
    [InlineData("audio.it",   AudioType.IT)]
    [InlineData("audio.mod",  AudioType.MOD)]
    [InlineData("audio.s3m",  AudioType.S3M)]
    [InlineData("audio.xm",   AudioType.XM)]
    [InlineData("audio.xma",  AudioType.XMA)]
    [InlineData("audio.vag",  AudioType.VAG)]
    public void TryGetAudioType_KnownExtension_ReturnsCorrectType(string path, AudioType expected)
    {
        Assert.Equal(expected, path.TryGetAudioType());
    }

    [Theory]
    [InlineData("audio.ogg.mp114514",  AudioType.OGGVORBIS)]
    [InlineData("audio.wav.unknown",  AudioType.WAV)]
    [InlineData("audio.mp3.ogg.mp3",  AudioType.MPEG)]
    [InlineData("audio.aac.aiff.mp3.old", AudioType.MPEG)]
    public void TryGetAudioType_ASSNaming_ReturnsCorrectType(string path, AudioType expected)
    {
        Assert.Equal(expected, path.TryGetAudioType());
    }

    [Theory]
    [InlineData("audio.flac")]
    [InlineData("audio.m4a")]
    [InlineData("audio")]
    public void TryGetAudioType_UnknownExtension_ReturnsUnknown(string path)
    {
        Assert.Equal(AudioType.UNKNOWN, path.TryGetAudioType());
    }

    // extension matching is case-sensitive — "audio.WAV" returns UNKNOWN
    [Theory]
    [InlineData("audio.WAV")]
    [InlineData("audio.MP3")]
    [InlineData("audio.OGG")]
    public void TryGetAudioType_UppercaseExtension_ReturnsUnknown(string path)
    {
        Assert.Equal(AudioType.UNKNOWN, path.TryGetAudioType());
    }

    // multi-dot names: rightmost known extension wins
    [Theory]
    [InlineData("kill.ogg.mp3",  AudioType.MPEG)]
    [InlineData("kill.mp3.ogg",  AudioType.OGGVORBIS)]
    [InlineData("sfx.unknown.wav", AudioType.WAV)]
    public void TryGetAudioType_MultiDotName_RightmostKnownWins(string path, AudioType expected)
    {
        Assert.Equal(expected, path.TryGetAudioType());
    }

    // full path with dots in directory name
    [Theory]
    [InlineData(@"C:\path.with.dots\audio.wav", AudioType.WAV)]
    [InlineData(@"C:\my.sounds\kill.mp3",       AudioType.MPEG)]
    public void TryGetAudioType_FullPath_FindsExtension(string path, AudioType expected)
    {
        Assert.Equal(expected, path.TryGetAudioType());
    }
}
