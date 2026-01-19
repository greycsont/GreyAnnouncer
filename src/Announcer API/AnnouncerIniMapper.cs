using System;
using System.Globalization;
using System.Collections.Generic;
using GreyAnnouncer.Util.Ini;

namespace GreyAnnouncer.AnnouncerAPI;

public static class AnnouncerIniMapper
{
    public static AnnouncerConfig FromIni(IniDocument doc)
    {
        var config = new AnnouncerConfig
        {
            CategoryAudioMap = new Dictionary<string, CategorySetting>()
        };

        // -------- General --------
        if (doc.TryGetSection("General", out var general))
        {
            config.RandomizeAudioOnPlay = GetBool(general, "RandomizeAudioOnPlay", false);
        }

        // -------- Categories --------
        foreach (var pair in doc.Sections)
        {
            var sectionName = pair.Key;
            var section = pair.Value;

            if (!sectionName.StartsWith("Category:", StringComparison.OrdinalIgnoreCase))
                continue;

            var key = sectionName.Substring("Category:".Length).Trim();
            if (key.Length == 0)
                continue;

            var cat = new CategorySetting
            {
                Enabled = GetBool(section, "Enabled", true),
                VolumeMultiplier = GetFloat(section, "VolumeMultiplier", 1.0f),
                Cooldown = GetFloat(section, "Cooldown", 1.5f)
            };

            // AudioFiles: a.wav, b.wav, c.wav
            var audioLine = section.GetLastValue("AudioFiles");
            if (!string.IsNullOrEmpty(audioLine))
            {
                foreach (var part in audioLine.Split(','))
                {
                    var file = part.Trim();
                    if (file.Length > 0)
                        cat.AudioFiles.Add(file);
                }
            }
            config.CategoryAudioMap[key] = cat;
        }

        return config;
    }

    // -------- helpers --------

    private static bool GetBool(IniSection s, string key, bool def)
    {
        var v = s.GetLastValue(key);
        bool b;
        return v != null && bool.TryParse(v, out b) ? b : def;
    }

    private static float GetFloat(IniSection s, string key, float def)
    {
        var v = s.GetLastValue(key);
        float f;
        return v != null && float.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out f)
            ? f : def;
    }
}
