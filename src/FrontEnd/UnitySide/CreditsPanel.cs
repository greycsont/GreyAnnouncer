using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GreyAnnouncer.FrontEnd;

public class CreditsPanel : MonoBehaviour
{
    public void Awake()
    {
        UIBuilder.SetFullStretch(GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>());

        var (_, content) = UIBuilder.BuildScrollView(transform, padding: new RectOffset(12, 12, 12, 12));

        UIBuilder.AddLabel(content, "Credits", 26, new Color(1f, 0.3f, 1f), preferredHeight: 23);
        UIBuilder.AddSeparator(content);
        UIBuilder.AddSpace(content, 6);

        UIBuilder.AddLabel(content, BuildCreditText(), 13, Color.white,
            alignment: TextAlignmentOptions.TopLeft);
    }

    private static string BuildCreditText()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Thanks:");
        sb.AppendLine("https://unsplash.com/photos/white-textile-on-brown-wooden-table-_kUxT8WkoeY");
        sb.AppendLine();
        sb.AppendLine("- OSU!");
        sb.AppendLine("  (https://osu.ppy.sh/) some UX are copied from lazer (YES EDIT EXTERNALLY),");
        sb.AppendLine("  use .ini as config cuz `skin.ini` is pretty easy to edit");
        sb.AppendLine();
        sb.AppendLine("- Artless Games");
        sb.AppendLine("  (https://space.bilibili.com/1237125233) 14 Minesweeper variants");
        sb.AppendLine();
        sb.AppendLine("- Announcer Mod");
        sb.AppendLine("  (https://www.nexusmods.com/ultrakill/mods/54) I started this project just");
        sb.AppendLine("  because the mod wasn't updated a few days after revamp");
        sb.AppendLine();
        sb.AppendLine("- PluginConfigurator doc");
        sb.AppendLine("  (https://github.com/eternalUnion/UKPluginConfigurator/wiki)");
        sb.AppendLine("  Easy to use tbh (except changing font)");
        sb.AppendLine();
        sb.AppendLine("- Maxwell's puzzling demon");
        sb.AppendLine("  (https://store.steampowered.com/app/2770160/) Fun puzzle game");
        sb.AppendLine();
        sb.AppendLine("- 快乐萨卡兹厨, jamcame, 夕柱.");
        sb.AppendLine();
        sb.AppendLine("- Everyone who used this mod.");
        sb.AppendLine();
        sb.AppendLine();
        sb.AppendLine("- FUCK YOU UNITY.");
        sb.AppendLine();
        sb.AppendLine();
        sb.AppendLine("Turn up the bass!");
        sb.AppendLine("THWWWWOOOOMPPPP DHGDHGDHGDHGDHGDHG BWOBWOBWOBWOB BWOBWOBWOBWOB BWOBWOBWOBWOBWOB");
        sb.AppendLine("BWAHH BWAHH BWAHH HMRMRMRMRMRMRMRMMR WOH WOH WOH- H H H HHHHH");
        sb.AppendLine("THWWOOMPPPP DHGDHGDHGDHGDHGDHG BWOBWOBWOBWOB.. BWOBWOBWOBWOB..");
        sb.AppendLine("BWOBWOBWOBWOBWOB.... HRMMMMMMMMMMMMMMMMRRR- HRRRRRRR HRRRRRRRR HRRRRRR HRRRRRRRR");
        sb.AppendLine("HRRR HR-YOOOH!!!!!!");
        sb.AppendLine();
        sb.AppendLine("- shadow of cats");
        return sb.ToString();
    }
}
