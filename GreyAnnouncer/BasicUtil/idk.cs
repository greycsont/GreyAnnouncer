using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;

using greycsont.GreyAnnouncer;  // Assuming you're using Unity's AudioClip

[Obsolete("A ")]
public class FLACDecoder
{
    public static AudioClip LoadFLACAsAudioClip(string path)
    {
        try
        {
            // 解码 FLAC 文件为 PCM 数据
            byte[] pcmData = DecodeFLACToPCM(path);
            if (pcmData == null)
            {
                return null;  // 解码失败返回 null
            }

            // 将 PCM 数据转换为 float 数组
            float[] floatData = ConvertPCMToFloat(pcmData);

            // 创建并返回 AudioClip
            AudioClip clip = AudioClip.Create(
                Path.GetFileNameWithoutExtension(path),
                floatData.Length,
                2, // 假设双声道
                44100, // 假设采样率为 44100
                false
            );

            // 设置 AudioClip 数据
            clip.SetData(floatData, 0);

            return clip;
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"FLAC loading failed: {ex.Message}");
            return null;  // 出错时返回 null
        }
    }

    private static byte[] DecodeFLACToPCM(string path)
    {
        string pluginDir = PathManager.GetCurrentPluginPath("flac.exe");

        if (!File.Exists(pluginDir))
        {
            Plugin.Log.LogError($"flac.exe not found : {pluginDir}");
            return null;
        }

        string outputWavFilePath = PathManager.GetCurrentPluginPath("temp.wav");

        ProcessStartInfo startInfo = new ProcessStartInfo()
        {
            FileName = pluginDir,
            Arguments = $"-d \"{path}\" -o \"{outputWavFilePath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try
        {
            using (Process process = Process.Start(startInfo))
            {
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    throw new Exception("FLAC decoding failed");
                }
            }

            // 读取 WAV 文件并返回字节数组
            byte[] data = File.ReadAllBytes(outputWavFilePath);

            try
            {
                File.Delete(outputWavFilePath);
            }
            catch (Exception delEx)
            {
                Plugin.Log.LogWarning($"delete temp.wav file failed: {delEx.Message}");
            }

            return data;
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"解码 FLAC 文件时出错: {ex.Message}");
            return null;  // 出错时返回 null
        }
    }

    private static float[] ConvertPCMToFloat(byte[] pcmData)
    {
        // 假设 PCM 数据是 16 位深度，转换为 float 数组
        int pcmLength = pcmData.Length / 2;  // 16 位 PCM 数据，每个样本 2 字节
        float[] floatData = new float[pcmLength];

        for (int i = 0; i < pcmLength; i++)
        {
            short pcmSample = BitConverter.ToInt16(pcmData, i * 2);
            floatData[i] = pcmSample / 32768f;  // 转换为 [-1.0f, 1.0f] 范围
        }

        return floatData;
    }
}
