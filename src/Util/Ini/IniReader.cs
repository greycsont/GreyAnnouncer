using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GreyAnnouncer.Util.Ini;

public static class IniReader
{
    /// <summary>
    /// Reads an INI file and returns an <see cref="IniDocument"/>.
    /// Supports ; # // comments, inline comments, and multi-value keys via comma separation.
    /// Returns an empty document if the file does not exist.
    /// </summary>
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

            // Skip full-line comments (;, #, //)
            if (line.StartsWith(";") || line.StartsWith("#") || line.StartsWith("//"))
                continue;

            // Section header: [SectionName]
            if (line.StartsWith("[") && line.EndsWith("]"))
            {
                var sectionName = line.Substring(1, line.Length - 2).Trim();
                if (sectionName.Length == 0)
                    continue;

                // Reuse existing section if already present (handles duplicate section headers)
                if (!doc.Sections.TryGetValue(sectionName, out currentSection))
                {
                    currentSection = new IniSection(sectionName);
                    doc.Sections[sectionName] = currentSection;
                }

                continue;
            }

            // Ignore keys before any section header
            if (currentSection == null)
                continue;

            // Strip inline comments before parsing key/value
            int commentIdx = IndexOfInlineComment(line);
            if (commentIdx > 0)
                line = line.Substring(0, commentIdx).Trim();

            // Support both Key = Value and Key: Value formats
            int separatorIndex = line.IndexOf('=');
            if (separatorIndex < 0)
                separatorIndex = line.IndexOf(':');

            if (separatorIndex <= 0)
                continue;

            var key = line.Substring(0, separatorIndex).Trim();
            var value = line.Substring(separatorIndex + 1).Trim();

            if (key.Length == 0)
                continue;

            // Get or create the value list for this key
            if (!currentSection.Values.TryGetValue(key, out var list))
            {
                list = new List<string>();
                currentSection.Values[key] = list;
            }

            if (value.Length == 0)
                continue;

            // Split comma-separated values and append to the key's list
            var parts = value.Split(',')
                             .Select(x => x.Trim())
                             .Where(x => x.Length > 0);

            list.AddRange(parts);
        }

        return doc;
    }

    /// <summary>
    /// Returns the index of the first inline comment character (; or #)
    /// that is not inside a double-quoted string, or -1 if none found.
    /// </summary>
    private static int IndexOfInlineComment(string line)
    {
        bool inQuote = false;
        for (int i = 0; i < line.Length; i++)
        {
            if (line[i] == '"') inQuote = !inQuote;
            if (!inQuote && (line[i] == ';' || line[i] == '#'))
                return i;
        }
        return -1;
    }
}