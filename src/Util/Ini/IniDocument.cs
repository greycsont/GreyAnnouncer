using System;
using System.Collections.Generic;

namespace GreyAnnouncer.Util.Ini;

/// <summary>
/// Represents a parsed INI file as a collection of named sections.
/// Section name lookup is case-insensitive.
/// </summary>
public sealed class IniDocument
{
    /// <summary>All sections in the document, keyed by section name.</summary>
    public Dictionary<string, IniSection> Sections { get; }
        = new Dictionary<string, IniSection>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Attempts to retrieve a section by name.
    /// Returns false if the section does not exist.
    /// </summary>
    public bool TryGetSection(string name, out IniSection section)
        => Sections.TryGetValue(name, out section);
}

/// <summary>
/// Represents a single section in an INI file.
/// Each key can hold multiple values to support comma-separated or repeated entries.
/// Key lookup is case-insensitive.
/// </summary>
public sealed class IniSection
{
    /// <summary>The name of this section as it appears in the INI file.</summary>
    public string Name { get; }

    /// <summary>Key-to-values mapping. Each key may have one or more associated values.</summary>
    public Dictionary<string, List<string>> Values { get; }
        = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

    public IniSection(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Returns all values associated with the given key,
    /// or an empty enumerable if the key does not exist.
    /// </summary>
    public IEnumerable<string> GetValues(string key)
        => Values.TryGetValue(key, out var list) ? list : Array.Empty<string>();

    /// <summary>
    /// Returns the last value associated with the given key,
    /// or null if the key does not exist or has no values.
    /// Used for single-value keys where the last entry takes precedence.
    /// </summary>
    public string GetLastValue(string key)
        => Values.TryGetValue(key, out var list) && list.Count > 0
            ? list[list.Count - 1]
            : null;
}