using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

using GreyAnnouncer.Util;
using GreyAnnouncer.Util.Ini;

namespace GreyAnnouncer.AnnouncerAPI;

public partial class AudioAnnouncer
{
    /// <summary>A reference to config.ini's path</summary>
    private string iniPath => Path.Combine(announcerPath, "config.ini");

    private AnnouncerConfig InitializeConfig(List<string> category)
    {
        LogHelper.LogDebug($"current iniPath of {title}: {iniPath}");

        var defaultConfig = new AnnouncerConfig().SetCategorySettingMap(
            category.ToDictionary(cat => cat, cat => new CategorySetting())
        );

        if (!File.Exists(iniPath))
        {
            LogHelper.LogDebug($"Initialize new config.ini in: {iniPath}");
            WriteConfigToIni(defaultConfig);
            isConfigLoaded = false;
            return defaultConfig;
        }

        var iniConfig = ReadConfigFromIni();
        if (iniConfig == null || !IsCategoryMatch(iniConfig, category))
        {
            LogHelper.LogError($"[{title}] AnnouncerConfig category mismatch — {configMismatchInfo}");
            isConfigLoaded = false;
            return null;
        }

        isConfigLoaded = true;
        return iniConfig;
    }

    private bool IsCategoryMatch(AnnouncerConfig config, List<string> expected)
    {
        var keys = config.CategorySetting.Keys;
        var missing = expected.Except(keys).ToList();
        var extra = keys.Except(expected).ToList();

        if (missing.Count == 0)
            return true;

        configMismatchInfo = $"Missing: [{string.Join(", ", missing)}], Extra: [{string.Join(", ", extra)}]";
        return false;
    }

    private void WriteConfigToIni(AnnouncerConfig announcerConfig)
    {
        LogHelper.LogDebug($"[WriteConfigToIni] called by {new StackTrace().GetFrame(1).GetMethod().Name}");
        var doc = new IniDocument();

        doc = AnnouncerIniMapper.ToIni(doc, announcerConfig);

        IniWriter.Write(iniPath, doc);
    }

    private AnnouncerConfig ReadConfigFromIni()
    {
        if (!File.Exists(iniPath))
            return null;

        var doc = IniReader.Read(iniPath);

        var announcerConfig = AnnouncerIniMapper.FromIni(doc);
        CheckConfigUpdate(ref announcerConfig);
        return announcerConfig;
    }

    private void CheckConfigUpdate(ref AnnouncerConfig announcerConfig)
    {

    }
}
