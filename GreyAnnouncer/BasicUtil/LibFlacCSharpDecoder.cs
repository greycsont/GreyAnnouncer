using LibFlacSharp;
using LibFlacSharp.Decoders;
using LibFlacSharp.Metadata;
using System.Collections.Generic;
using System;
using System.Linq;


public class LibFlacCSharpDecoder
{
    public static float[] DecodeFlac(string filePath, out int sampleRate, out int channels)
    {
        using var decoder = new FlacFileDecoder();

        List<short> samples = new();
        sampleRate = 0;
        channels = 0;

        decoder.MetadataCallback += metadata =>
        {
            if (metadata.Type == MetadataType.StreamInfo)
            {
                var info = (StreamInfo)metadata.Data;
                sampleRate = info.SampleRate;
                channels = info.Channels;
            }
        };

        decoder.WriteCallback += (frame, buffer) =>
        {
            int blockSize = frame.BlockSize;
            int channelCount = buffer.Length;

            // Interleave the samples
            for (int i = 0; i < blockSize; i++)
            {
                for (int ch = 0; ch < channelCount; ch++)
                {
                    samples.Add((short)buffer[ch][i]); // Assuming 16-bit
                }
            }

            return WriteStatus.Continue;
        };

        var status = decoder.Init(filePath);
        if (status != InitStatus.Ok)
            throw new Exception("FLAC decoder init failed");

        decoder.ProcessUntilEndOfStream();
        decoder.Finish();

        // Convert to float array
        float[] floats = samples.Select(s => s / 32768f).ToArray();
        return floats;
    }
}

