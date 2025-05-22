using UnityEngine;
using System.Threading.Tasks;

public interface IAudioClipLoader
{
    public Task<AudioClip> LoadAudioClipAsync(string path);
}