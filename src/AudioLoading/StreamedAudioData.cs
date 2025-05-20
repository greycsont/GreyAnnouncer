using System;
using System.Collections.Generic;

public class StreamedAudioData
{
    private const int CHUNK_SIZE = 8192; // 8KB chunks
    private readonly List<float[]> chunks = new List<float[]>();
    private float[] currentChunk = new float[CHUNK_SIZE];
    private int currentPos;

    public int Channels { get; set; }
    public int SampleRate { get; set; }
    public int TotalSamples { get; private set; }

    public void AddSamples(float[] samples)
    {
        foreach (var sample in samples)
        {
            currentChunk[currentPos++] = sample;
            TotalSamples++;

            if (currentPos >= CHUNK_SIZE)
            {
                chunks.Add(currentChunk);
                currentChunk = new float[CHUNK_SIZE];
                currentPos = 0;
            }
        }
    }

    public float[] GetAllSamples()
    {
        if (currentPos > 0)
        {
            Array.Resize(ref currentChunk, currentPos);
            chunks.Add(currentChunk);
            currentChunk = Array.Empty<float>();
        }

        var result = new float[TotalSamples];
        int pos = 0;

        foreach (var chunk in chunks)
        {
            Array.Copy(chunk, 0, result, pos, chunk.Length);
            pos += chunk.Length;
        }

        return result;
    }
}