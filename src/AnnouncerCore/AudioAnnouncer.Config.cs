using System.IO;
using System.Linq;
using System.Collections.Generic;

using GreyAnnouncer.Util;

namespace GreyAnnouncer.AnnouncerCore;

public partial class AudioAnnouncer
{
    private AnnouncerConfig InitializeConfig(List<string> category)
    {
        LogHelper.LogDebug($"Loading config for {title} from: {announcerPath}");

        var defaultConfig = new AnnouncerConfig().SetCategorySettingMap(
            category.ToDictionary(cat => cat, cat => new CategorySetting())
        );

        var config = _configManager.Load(announcerPath);
        if (config == null) {
            LogHelper.LogDebug($"No config found, writing default to: {announcerPath}");
            _configManager.Save(announcerPath, defaultConfig);
            isConfigLoaded = false;
            return defaultConfig;
        }

        if (!IsCategoryMatch(config, category)) {
            LogHelper.LogError($"[{title}] AnnouncerConfig category mismatch — {configMismatchInfo}");
            isConfigLoaded = false;
            return null;
        }

        isConfigLoaded = true;
        return config;
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
}
