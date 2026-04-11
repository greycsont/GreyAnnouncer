/*
 * 基本上就是依靠 category 加载音频
 * 以及配套的获取 audioclip 的函数
 */

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel;
using UnityEngine;

using GreyAnnouncer.AudioLoading;
using GreyAnnouncer.Config;
using GreyAnnouncer.AudioSourceComponent;
using GreyAnnouncer.Util;

namespace GreyAnnouncer.AnnouncerCore;

public partial class AudioLoader : IAudioLoader
{
    private IAnnouncer _announcer;
    public AnnouncerConfig announcerConfig => _announcer?.announcerConfig;
    public string announcerPath => _announcer?.announcerPath;

    public static Action<string> onPluginConfiguratorLogUpdated;

    [Description("Q : Why do you using whole AnnouncerJsonSetting as input instead only CategoryAudioMap?" +
                 "A : For future, what kinds of future? idk.")]
    public AudioLoader() { }


    public async Task<Sound> GetAudioClip(string category)
    {
        (string, AudioClip) clipWithCategory;

        if (Setting.audioLoadingStrategy == 0)
        {
            var currentRequestId = ++AnnouncerManager.playRequestId;

            clipWithCategory = announcerConfig.RandomizeAudioOnPlay
                ? await GetRandomClipFromAllAvailableFiles()
                : await LoadAndGetSingleAudioClip(category);

            if (currentRequestId != AnnouncerManager.playRequestId && Setting.audioPlayOptions == 0)
                return null;
        }
        else
        {
            clipWithCategory = announcerConfig.RandomizeAudioOnPlay
                ? GetRandomClipFromAudioClips()
                : GetClipFromCache(category);
        }

        if (clipWithCategory.Item2 == null)
        {
            LogHelper.LogWarning($"No audio clip available for category 「{category}」, strategy: {Setting.audioLoadingStrategy}");
            return null;
        }

        return new Sound(
            clipWithCategory.Item1,
            clipWithCategory.Item2,
            announcerConfig.CategorySetting[clipWithCategory.Item1].VolumeMultiplier
        );
    }

    public void SetProvider(IAnnouncer provider)
    {
        _announcer = provider;
    }

    public async Task FindAvailableAudioAsync()
    {
        ClearCache();
        if (Setting.audioLoadingStrategy == 0) return;
        LogHelper.LogInfo("Starting to find available audio");
        await LoadAllCategoriesAsync();
        LogLoadingResults();
    }

    public void ClearCache()
    {
        ClearAudioClipCache();
        _categoryFailed.Clear();
    }


    private bool TryResolveAudioFiles(string category, out List<string> validFiles)
    {
        validFiles = null;

        if (!announcerConfig.CategorySetting.TryGetValue(category, out var categorySetting))
            return false;

        if (categorySetting.AudioFiles == null || categorySetting.AudioFiles.Count == 0)
            return false;

        validFiles = categorySetting.AudioFiles
            .Select(name => PathHelper.GetFile(announcerPath, name))
            .Where(File.Exists)
            .ToList();

        if (validFiles.Count == 0)
            return false;

        var missing = categorySetting.AudioFiles
            .Except(validFiles.Select(Path.GetFileName))
            .ToList();

        if (missing.Count > 0)
            LogHelper.LogWarning($"「{category}」 missing files: {string.Join(", ", missing)}");

        return true;
    }
}
