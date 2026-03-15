using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using GreyAnnouncer.Util.Ini;

namespace GreyAnnouncer.Util.Ini;

public static class IniWriter
{
    public static void Write(string path, IniDocument doc)
    {
        FileSystemUtil.EnsureDirectoryExists(path);

        if (!File.Exists(path))
        {
            WriteNew(path, doc);
            return;
        }

        var lines = File.ReadAllLines(path).ToList();
        var result = new List<string>();
        string currentSection = null;
        var writtenSections = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        int i = 0;
        while (i < lines.Count)
        {
            var trimmed = lines[i].Trim();

            // 注释或空行直接保留
            if (trimmed.Length == 0 || trimmed.StartsWith(";") || trimmed.StartsWith("#") || trimmed.StartsWith("//"))
            {
                result.Add(lines[i]);
                i++;
                continue;
            }

            // Section 头
            if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
            {
                var sectionName = trimmed.Substring(1, trimmed.Length - 2).Trim();

                // 如果这个 section 已经写过（重复 section），跳过直到下一个 section
                if (writtenSections.Contains(sectionName))
                {
                    i++;
                    while (i < lines.Count)
                    {
                        var t = lines[i].Trim();
                        if (t.StartsWith("[") && t.EndsWith("]")) break;
                        i++;
                    }
                    continue;
                }

                currentSection = sectionName;
                writtenSections.Add(sectionName);

                if (doc.Sections.TryGetValue(sectionName, out var section))
                {
                    // 写 section 头
                    result.Add($"[{sectionName}]");
                    i++;

                    // 跳过旧的 key/value 行，保留注释
                    while (i < lines.Count)
                    {
                        var t = lines[i].Trim();
                        if (t.StartsWith("[") && t.EndsWith("]")) break;
                        // 只保留注释行，不保留空行
                        if (t.StartsWith(";") || t.StartsWith("#") || t.StartsWith("//"))
                            result.Add(lines[i]);
                        i++;
                    }

                    // 写新的 key/value
                    foreach (var kv in section.Values)
                        foreach (var v in kv.Value)
                            result.Add($"{kv.Key} = {v}");

                    result.Add("");
                }
                else
                {
                    // doc 里没有这个 section，原样保留
                    result.Add(lines[i]);
                    i++;
                }
                continue;
            }

            result.Add(lines[i]);
            i++;
        }

        // doc 里有但文件里没有的 section，追加到末尾
        foreach (var sectionPair in doc.Sections)
        {
            if (!writtenSections.Contains(sectionPair.Key))
            {
                result.Add($"[{sectionPair.Key}]");
                foreach (var kv in sectionPair.Value.Values)
                    foreach (var v in kv.Value)
                        result.Add($"{kv.Key} = {v}");
                result.Add("");
            }
        }

        File.WriteAllLines(path, result);
    }

    private static int FindSectionEnd(List<string> lines, string sectionName)
    {
        bool inSection = false;
        for (int i = 0; i < lines.Count; i++)
        {
            var trimmed = lines[i].Trim();
            if (trimmed == $"[{sectionName}]")
            {
                inSection = true;
                continue;
            }
            if (inSection && trimmed.StartsWith("["))
                return i; // 下一个 section 开始前
        }
        return lines.Count; // 文件末尾
    }

    private static void WriteNew(string path, IniDocument doc)
    {
        using var writer = new StreamWriter(path, false);
        foreach (var sectionPair in doc.Sections)
        {
            writer.WriteLine($"[{sectionPair.Key}]");
            foreach (var kv in sectionPair.Value.Values)
                foreach (var v in kv.Value)
                    writer.WriteLine($"{kv.Key} = {v}");
            writer.WriteLine();
        }
    }
}
