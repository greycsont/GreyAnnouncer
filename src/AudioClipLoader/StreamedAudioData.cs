using System;
using System.Collections.Generic;

public class StreamedAudioData
{
    private const int _CHUNK_SIZE = 8192; // 8KB chunks
    private readonly LinkedList<float[]> _chunks = new LinkedList<float[]>();
    private float[] _currentChunk = new float[_CHUNK_SIZE];
    private int _currentPos;

    public int Channels { get; set; }
    public int SampleRate { get; set; }
    public int TotalSamples { get; private set; }

    public void AddSamples(float[] samples)
    {
        int srcOffset = 0;
        int remaining = samples.Length;

        while (remaining > 0) {
            int space = _CHUNK_SIZE - _currentPos;
            int toCopy = Math.Min(space, remaining);

            Array.Copy(samples, srcOffset, _currentChunk, _currentPos, toCopy);
            _currentPos += toCopy;
            srcOffset += toCopy;
            remaining -= toCopy;
            TotalSamples += toCopy;

            if (_currentPos >= _CHUNK_SIZE) {
                _chunks.AddLast(_currentChunk);
                _currentChunk = new float[_CHUNK_SIZE];
                _currentPos = 0;
            }
        }
    }

    public float[] GetAllSamples()
    {
        var result = new float[TotalSamples];
        int pos = 0;

        foreach (var chunk in _chunks) {
            Array.Copy(chunk, 0, result, pos, chunk.Length);
            pos += chunk.Length;
        }

        if (_currentPos > 0) {
            Array.Copy(_currentChunk, 0, result, pos, _currentPos);
        }

        return result;
    }

}