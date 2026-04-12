using System.IO;
using System.Linq;
using System.Collections.Generic;

using GreyAnnouncer.Util;

namespace GreyAnnouncer.AnnouncerCore;

public partial class AudioAnnouncer
{
    private PackConfig LoadConfig()
    {
        LogHelper.LogDebug($"Loading config for {title} from: {announcerPath}");

        if (!File.Exists(_configManager.ConfigPath(announcerPath))) {
            LogHelper.LogDebug($"No config found in {announcerPath}");
            return null;
        }

        var config = _configManager.Load(announcerPath);

        if (!IsCategoryMatch(config, category)) {
            LogHelper.LogError($"[{title}] AnnouncerConfig category mismatch — {configMismatchInfo}");
            isConfigLoaded = false;
            return null;
        }

        isConfigLoaded = true;
        return config;
    }

    private bool IsCategoryMatch(PackConfig config, List<string> expected)
    {
        var keys = config.CategorySetting.Keys;
        var missing = expected.Except(keys).ToList();
        var extra = keys.Except(expected).ToList();

        if (missing.Count == 0)
            return true;

        configMismatchInfo = $"Missing: [{string.Join(", ", missing)}], Extra: [{string.Join(", ", extra)}]";
        return false;
    }
}
