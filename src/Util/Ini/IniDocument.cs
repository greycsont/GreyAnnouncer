using System;
using System.Collections.Generic;

namespace GreyAnnouncer.Util.Ini;

public sealed class IniDocument
{
    public Dictionary<string, IniSection> Sections { get; }
        = new Dictionary<string, IniSection>(StringComparer.OrdinalIgnoreCase);

    public bool TryGetSection(string name, out IniSection section)
        => Sections.TryGetValue(name, out section);
}

public sealed class IniSection
{
    public string Name { get; }

    // Key -> multiple values
    public Dictionary<string, List<string>> Values { get; }
        = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

    public IniSection(string name)
    {
        Name = name;
    }

    public IEnumerable<string> GetValues(string key)
        => Values.TryGetValue(key, out var list) ? list : Array.Empty<string>();

    public string GetLastValue(string key)
        => Values.TryGetValue(key, out var list) && list.Count > 0
            ? list[list.Count - 1]
            : null;
}

