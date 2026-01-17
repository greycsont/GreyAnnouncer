using System.IO;
using System.Collections.Generic;

namespace GreyAnnouncer.Util.Ini;

public static class IniReader
{
    public static IniDocument Read(string path)
    {
        var doc = new IniDocument();

        if (!File.Exists(path))
            return doc;

        IniSection currentSection = null;
        string currentListKey = null;

        foreach (var rawLine in File.ReadLines(path))
        {
            // 不 TrimStart：我们需要缩进判断
            var line = rawLine.TrimEnd();

            if (string.IsNullOrWhiteSpace(line))
            {
                currentListKey = null;
                continue;
            }

            var trimmed = line.Trim();

            // -------- Section --------
            if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
            {
                var sectionName = trimmed.Substring(1, trimmed.Length - 2).Trim();
                if (sectionName.Length == 0)
                    continue;

                if (!doc.Sections.TryGetValue(sectionName, out currentSection))
                {
                    currentSection = new IniSection(sectionName);
                    doc.Sections[sectionName] = currentSection;
                }

                currentListKey = null;
                continue;
            }

            // 没 section，忽略
            if (currentSection is null)
                continue;

            // -------- List continuation --------
            if (currentListKey != null && IsIndented(rawLine))
            {
                currentSection.Values[currentListKey].Add(trimmed);
                continue;
            }

            currentListKey = null;

            // -------- Key: Value --------
            int colonIndex = trimmed.IndexOf(':');
            if (colonIndex <= 0)
                continue;

            var key = trimmed.Substring(0, colonIndex).Trim();
            var value = trimmed.Substring(colonIndex + 1).Trim();

            if (key.Length == 0)
                continue;

            if (!currentSection.Values.TryGetValue(key, out var list))
            {
                list = new List<string>();
                currentSection.Values[key] = list;
            }

            // 空 value → 可能是 list 起始
            if (value.Length == 0)
            {
                currentListKey = key;
            }
            else
            {
                list.Add(value);
            }
        }

        return doc;
    }

    private static bool IsIndented(string line)
        => line.Length > 0 && (line[0] == ' ' || line[0] == '\t');
}
