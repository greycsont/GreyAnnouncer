using System;
using System.Globalization;
using System.Collections.Generic;
using GreyAnnouncer.Util.Ini;

namespace GreyAnnouncer.AnnouncerAPI;

public static class AnnouncerIniMapper
{
    public static AnnouncerConfig FromIni(IniDocument doc)
    {
        var config = new AnnouncerConfig();
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
            var audioFiles = section.GetValues("AudioFiles");
            foreach (var line in audioFiles)
            {
                foreach (var part in line.Split(','))
                {
                    var file = part.Trim();
                    if (file.Length > 0)
                        cat.AudioFiles.Add(file);
                }
            }
            config.AddCategory(key, cat);
        }

        return config;
    }

    public static IniDocument ToIni(IniDocument doc, AnnouncerConfig config)
    {
        // -------- General --------
        if (!doc.Sections.TryGetValue("General", out var general))
        {
            general = new IniSection("General");
            doc.Sections["General"] = general;
        }
        general.Values["RandomizeAudioOnPlay"] = new System.Collections.Generic.List<string>
        {
            config.RandomizeAudioOnPlay.ToString()
        };

        // -------- Categories --------
        foreach (var kv in config.CategorySetting)
        {
            var sectionName = $"Category:{kv.Key}";
            if (!doc.Sections.TryGetValue(sectionName, out var section))
            {
                section = new IniSection(sectionName);
                doc.Sections[sectionName] = section;
            }

            var cat = kv.Value;
            section.Values["Enabled"] = new System.Collections.Generic.List<string> { cat.Enabled.ToString() };
            section.Values["VolumeMultiplier"] = new System.Collections.Generic.List<string>
            {
                cat.VolumeMultiplier.ToString(CultureInfo.InvariantCulture)
            };
            section.Values["Cooldown"] = new System.Collections.Generic.List<string>
            {
                cat.Cooldown.ToString(CultureInfo.InvariantCulture)
            };

            if (cat.AudioFiles != null && cat.AudioFiles.Count > 0)
            {
                section.Values["AudioFiles"] = new System.Collections.Generic.List<string>
                {
                    string.Join(",", cat.AudioFiles)
                };
            }
        }

        return doc;
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
