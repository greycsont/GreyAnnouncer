using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using GreyAnnouncer.Util.Ini;

namespace GreyAnnouncer.Util.Ini;

/// <summary>
/// Maps between strongly-typed objects and <see cref="IniDocument"/> sections
/// using <see cref="IniKeyAttribute"/> to identify which properties to serialize.
/// </summary>
public static class IniMapper
{
    /// <summary>
    /// Deserializes a section of an <see cref="IniDocument"/> into a new instance of <typeparamref name="T"/>.
    /// Only properties marked with <see cref="IniKeyAttribute"/> are populated.
    /// Supports string, bool, float, and List&lt;string&gt; property types.
    /// </summary>
    /// <param name="doc">The INI document to read from.</param>
    /// <param name="sectionPrefix">
    /// The section name to read from. If null, defaults to "General".
    /// </param>
    public static T FromIni<T>(IniDocument doc, string sectionPrefix = null) where T : new()
    {
        var obj = new T();
        var type = typeof(T);

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var attr = prop.GetCustomAttribute<IniKeyAttribute>();
            if (attr == null)
                continue;

            // Resolve target section: defaults to "General" if no prefix provided
            IniSection section = null;
            if (sectionPrefix == null)
                doc.TryGetSection("General", out section);
            else
                doc.TryGetSection(sectionPrefix, out section);

            if (section == null)
                continue;

            object val = null;

            // Flatten all stored values and re-split by comma to normalize multi-value entries
            var allValues = section.GetValues(attr.KeyName)
                       .SelectMany(v => v.Split(',').Select(s => s.Trim()).Where(s => s.Length > 0))
                       .ToList();
                       
            if (prop.PropertyType == typeof(List<string>))
            {
                val = allValues;
            }
            else
            {
                // For single-value types, use the last entry (last-write-wins)
                var valueStr = allValues.LastOrDefault();
                if (valueStr == null)
                    continue;

                if (prop.PropertyType == typeof(string))
                    val = valueStr;
                else if (prop.PropertyType == typeof(bool))
                    val = bool.TryParse(valueStr, out var b) ? b : false;
                else if (prop.PropertyType == typeof(float))
                    val = float.TryParse(valueStr, System.Globalization.NumberStyles.Float,
                                        System.Globalization.CultureInfo.InvariantCulture, out var f) ? f : 0f;
            }

            if (val != null)
                prop.SetValue(obj, val);
        }

        return obj;
    }

    /// <summary>
    /// Serializes an object into a section of an <see cref="IniDocument"/>.
    /// Only properties marked with <see cref="IniKeyAttribute"/> are written.
    /// Creates the section if it does not already exist.
    /// List&lt;string&gt; values are joined as a single comma-separated entry.
    /// </summary>
    /// <param name="doc">The INI document to write into.</param>
    /// <param name="obj">The object to serialize.</param>
    /// <param name="sectionName">The target section name.</param>
    public static IniDocument ToIni<T>(IniDocument doc, T obj, string sectionName)
    {
        if (!doc.Sections.TryGetValue(sectionName, out var section))
        {
            section = new IniSection(sectionName);
            doc.Sections[sectionName] = section;
        }

        foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var attr = prop.GetCustomAttribute<IniKeyAttribute>();
            if (attr == null)
                continue;

            var val = prop.GetValue(obj);
            if (val == null)
                continue;

            // Store List<string> as a single comma-separated line
            if (val is List<string> list)
                section.Values[attr.KeyName] = new List<string> { string.Join(", ", list) };
            else
                section.Values[attr.KeyName] = new List<string> { val.ToString() };
        }

        return doc;
    }
}