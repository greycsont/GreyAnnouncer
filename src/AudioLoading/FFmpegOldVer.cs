using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using FFmpeg.AutoGen;
using UnityEngine;


namespace GreyAnnouncer.AudioLoading;


[Obsolete("The reason of leave it here is only for review and study")]
public static class FFmpegSupportOldVer
{
    public static unsafe Task<AudioClip> DecodeAndLoad(string filePath)
    {
        return Task.Run( () =>
        {
            ffmpeg.RootPath = PathManager.GetCurrentPluginPath(); // 或者你手动指定 dll 路径
            LogManager.LogWarning($"ffmpeg version: {ffmpeg.av_version_info()}");
            ffmpeg.avformat_network_init();

            AVFormatContext* formatContext = ffmpeg.avformat_alloc_context();
            if (ffmpeg.avformat_open_input(&formatContext, filePath, null, null) != 0)
                throw new Exception("Could not open input file.");

            if (ffmpeg.avformat_find_stream_info(formatContext, null) != 0)
                throw new Exception("Could not find stream info.");

            // 查找音频流
            int audioStreamIndex = -1;
            for (int i = 0; i < formatContext->nb_streams; i++)
            {
                if (formatContext->streams[i]->codecpar->codec_type == AVMediaType.AVMEDIA_TYPE_AUDIO)
                {
                    audioStreamIndex = i;
                    break;
                }
            }
            if (audioStreamIndex == -1)
                throw new Exception("No audio stream found.");

            // 设置解码器
            AVCodecParameters* codecpar = formatContext->streams[audioStreamIndex]->codecpar;
            AVCodec* codec = ffmpeg.avcodec_find_decoder(codecpar->codec_id);
            AVCodecContext* codecCtx = ffmpeg.avcodec_alloc_context3(codec);
            ffmpeg.avcodec_parameters_to_context(codecCtx, codecpar);
            ffmpeg.avcodec_open2(codecCtx, codec, null);

            int channels = codecCtx->ch_layout.nb_channels;
            AVChannelLayout inLayout = codecCtx->ch_layout;
            AVChannelLayout outLayout;
            ffmpeg.av_channel_layout_default(&outLayout, channels);

            // swrCtx 初始化方式
            SwrContext* swrCtx = ffmpeg.swr_alloc();
            ffmpeg.av_opt_set_chlayout(swrCtx, "in_chlayout", &inLayout, 0);
            ffmpeg.av_opt_set_chlayout(swrCtx, "out_chlayout", &outLayout, 0);
            ffmpeg.av_opt_set_int(swrCtx, "in_sample_rate", codecCtx->sample_rate, 0);
            ffmpeg.av_opt_set_int(swrCtx, "out_sample_rate", codecCtx->sample_rate, 0);
            ffmpeg.av_opt_set_sample_fmt(swrCtx, "in_sample_fmt", codecCtx->sample_fmt, 0);
            ffmpeg.av_opt_set_sample_fmt(swrCtx, "out_sample_fmt", AVSampleFormat.AV_SAMPLE_FMT_FLT, 0);
            ffmpeg.swr_init(swrCtx);

            AVPacket* packet = ffmpeg.av_packet_alloc();
            AVFrame* frame = ffmpeg.av_frame_alloc();

            List<float> samples = new();
            while (ffmpeg.av_read_frame(formatContext, packet) >= 0)
            {
                if (packet->stream_index != audioStreamIndex)
                {
                    ffmpeg.av_packet_unref(packet);
                    continue;
                }

                ffmpeg.avcodec_send_packet(codecCtx, packet);
                while (ffmpeg.avcodec_receive_frame(codecCtx, frame) == 0)
                {
                    byte** convertedData = stackalloc byte*[1];
                    int outLinesize;

                    int outSamples = ffmpeg.av_samples_alloc_array_and_samples(
                        &convertedData, &outLinesize,
                        codecCtx->ch_layout.nb_channels,
                        frame->nb_samples,
                        AVSampleFormat.AV_SAMPLE_FMT_FLT,
                        0);

                    int samplesConverted = ffmpeg.swr_convert(swrCtx,
                        convertedData,
                        frame->nb_samples,
                        frame->extended_data,
                        frame->nb_samples);

                    int bufferSize = ffmpeg.av_samples_get_buffer_size(
                        null,
                        codecCtx->ch_layout.nb_channels,
                        samplesConverted,
                        AVSampleFormat.AV_SAMPLE_FMT_FLT,
                        1);

                    float[] buffer = new float[bufferSize / sizeof(float)];
                    Marshal.Copy((IntPtr)convertedData[0], buffer, 0, buffer.Length);
                    samples.AddRange(buffer);

                    ffmpeg.av_freep(&convertedData[0]);
                }

                ffmpeg.av_packet_unref(packet);
            }
            // 创建 Unity AudioClip
            float[] sampleArray = samples.ToArray();
            int channels114514 = codecCtx->ch_layout.nb_channels;
            int sampleRate = codecCtx->sample_rate;
            int sampleCount = sampleArray.Length / channels;
            
            ffmpeg.av_frame_free(&frame);
            ffmpeg.av_packet_free(&packet);
            ffmpeg.avcodec_free_context(&codecCtx);
            ffmpeg.avformat_close_input(&formatContext);
            ffmpeg.swr_free(&swrCtx);


            AudioClip clip = AudioClip.Create("decoded_clip", sampleCount, channels114514, sampleRate, false);
            clip.SetData(sampleArray, 0);

            return clip;
        });
    }
}