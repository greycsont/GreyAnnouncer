using System.IO;
using System.Linq;
using GreyAnnouncer.Util.Ini;

namespace GreyAnnouncer.Util.Ini;

public static class IniWriter
{
    public static void Write(string path, IniDocument doc)
    {
        PathHelper.EnsureDirectoryExists(path);
        
        using var writer = new StreamWriter(path, false);

        foreach (var sectionPair in doc.Sections)
        {
            var section = sectionPair.Value;

            // 写 Section
            writer.WriteLine($"[{section.Name}]");

            // 写 Key: Value
            foreach (var kv in section.Values)
            {
                foreach (var val in kv.Value)
                {
                    writer.WriteLine($"{kv.Key}: {val}");
                }
            }

            writer.WriteLine();
        }
    }
}
