using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GreyAnnouncer.Util.Ini;

public static class IniReader
{
    public static IniDocument Read(string path)
    {
        var doc = new IniDocument();

        if (!File.Exists(path))
            return doc;

        IniSection currentSection = null;

        foreach (var rawLine in File.ReadLines(path))
        {
            var line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line))
                continue;

            // Section: [SectionName]
            if (line.StartsWith("[") && line.EndsWith("]"))
            {
                var sectionName = line.Substring(1, line.Length - 2).Trim();
                if (sectionName.Length == 0)
                    continue;

                if (!doc.Sections.TryGetValue(sectionName, out currentSection))
                {
                    currentSection = new IniSection(sectionName);
                    doc.Sections[sectionName] = currentSection;
                }

                continue;
            }

            // 如果还没 section，就忽略
            if (currentSection == null)
                continue;

            // Key: Value 或 Key=Value
            int separatorIndex = line.IndexOf(':');
            if (separatorIndex < 0)
                separatorIndex = line.IndexOf('=');

            if (separatorIndex <= 0)
                continue;

            var key = line.Substring(0, separatorIndex).Trim();
            var value = line.Substring(separatorIndex + 1).Trim();

            if (key.Length == 0)
                continue;

            if (!currentSection.Values.TryGetValue(key, out var list))
            {
                list = new List<string>();
                currentSection.Values[key] = list;
            }

            if (value.Length == 0)
                continue;

            // 用逗号拆分成多值
            var parts = value.Split(',')
                             .Select(x => x.Trim())
                             .Where(x => x.Length > 0);

            list.AddRange(parts);
        }

        return doc;
    }
}
