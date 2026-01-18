using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using GreyAnnouncer.Util.Ini;

namespace GreyAnnouncer.Util.Ini;

public static class IniMapper
{
    public static T FromIni<T>(IniDocument doc, string sectionPrefix = null) where T : new()
    {
        var obj = new T();
        var type = typeof(T);

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var attr = prop.GetCustomAttribute<IniKeyAttribute>();
            if (attr == null)
                continue;

            // 支持 General 或 Category
            IniSection section = null;
            if (sectionPrefix == null)
                doc.TryGetSection("General", out section);
            else
                doc.TryGetSection(sectionPrefix, out section);

            if (section == null)
                continue;

            object val = null;

            var allValues = section.GetValues(attr.KeyName)
                       .SelectMany(v => v.Split(',').Select(s => s.Trim()).Where(s => s.Length > 0))
                       .ToList();
                       
            if (prop.PropertyType == typeof(List<string>))
            {
                val = allValues;
            }
            else
            {
                var valueStr = allValues.LastOrDefault(); // 普通单值用最后一个
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

            if (val is List<string> list)
                section.Values[attr.KeyName] = new List<string> { string.Join(", ", list) };
            else
                section.Values[attr.KeyName] = new List<string> { val.ToString() };
        }

        return doc;
    }
}
