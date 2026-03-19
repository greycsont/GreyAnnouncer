using UnityEngine;
using System;
using System.Text;
using System.Collections.Generic;

using PluginConfig.API;
using PluginConfig.API.Fields;
using PluginConfig.API.Decorators;
using PluginConfig.API.Functionals;

using GreyAnnouncer.AnnouncerAPI;
using GreyAnnouncer.AudioSourceComponent;
using GreyAnnouncer.Config;
using System.IO;


namespace GreyAnnouncer.FrontEnd;

[EntryPoint]
public static class MainPanelBuilder
{
    private static readonly Color m_greyColour = new UnityEngine.Color(0.85f, 0.85f, 0.85f, 1f);

    private static readonly Color m_CyanColour = new UnityEngine.Color(0f, 1f, 1f, 1f);

    private static readonly Color m_OrangeColour = new UnityEngine.Color(1f, 0.6f, 0.2f, 1f);

    private static readonly Color m_RedColour = new UnityEngine.Color(1f, 0f, 0f, 1f);

    private static readonly Color m_PurpleColour = new UnityEngine.Color(1f, 0f, 1f, 1f);

    private static PluginConfigurator m_pluginConfigurator;

    public static ConfigHeader audioLoaderLogHeader;

    private static ConfigPanel advancedPanel;

    private static ConfigPanel creditPanel;

    private static List<RegistedAnnouncerPage> _pages = new List<RegistedAnnouncerPage>();

    private static FloatSliderField volumeSlider;

    private static EnumField<AudioPlayOptions> playOption;

    private static EnumField<AudioLoadOptions> loadOption;

    private static BoolField lowpassToggle;

    private static BoolField ffmpegToggle;


    [EntryPoint]
    public static void Build(PluginConfigurator config)
    {
        m_pluginConfigurator = config;

        CreateMainSettingSectionTitle();
        CreateAnnouncerPage();
        CreateAudioControls();
        CreateAdvancedOptionPanel();
        CreateCreditsPanel();
        CreateAnnouncerSection();
        

        CreateDelegateTextFromBackEnd();

        AnnouncerManager.OnRegistered += AddAnnouncerPage;
        Setting.syncUI += SyncUI;
    }

    public static void SyncUI()
    {
        volumeSlider.value = Setting.audioSourceVolume;
        playOption.value = (AudioPlayOptions)Setting.audioPlayOptions;
        loadOption.value = (AudioLoadOptions)Setting.audioLoadingStrategy;
        lowpassToggle.value = Setting.isLowPassFilterEnabled;
        ffmpegToggle.value = Setting.isFFmpegSupportEnabled;
    }

    public static void CreateAnnouncerPage()
    {
        foreach (var announcer in AnnouncerManager.announcers)
        {
            var newPage = new RegistedAnnouncerPage(announcer);
            _pages.Add(newPage);
        }
    }

    private static void CreateMainSettingSectionTitle()
    {
        new ConfigSpace(m_pluginConfigurator.rootPanel, 15f);

        ConfigHeader mainHeader = new ConfigHeader(m_pluginConfigurator.rootPanel, "Main Settings");
        mainHeader.textColor = m_CyanColour;

    }


    private static void CreateAudioControls()
    {

        volumeSlider = new FloatSliderField(
            m_pluginConfigurator.rootPanel,
            "Master Volume",
            "Audio_Volume",
            Tuple.Create(0f, 1f),
            Setting.audioSourceVolume,
            2   // 2nd decimal
        );
        volumeSlider.defaultValue = 1f;
        volumeSlider.onValueChange += e =>
            Setting.audioSourceVolume = e.newValue;

        // It worked, but not working great as there's ton of audio when from low rank directly to the high rank
        // May be add a short cooldown as limitation

        playOption = new EnumField<AudioPlayOptions>(
            m_pluginConfigurator.rootPanel,
            "Audio Play Strategy",
            "Audio_Play_Strategy",
            (AudioPlayOptions)Setting.audioPlayOptions
        );
        playOption.defaultValue = (AudioPlayOptions)0;
        playOption.onValueChange += e =>
            Setting.audioPlayOptions = (int)e.value;

        loadOption = new EnumField<AudioLoadOptions>(
            m_pluginConfigurator.rootPanel,
            "Audio Load Strategy",
            "Audio_Load_Strategy",
            (AudioLoadOptions)Setting.audioLoadingStrategy
        );
        loadOption.defaultValue = (AudioLoadOptions)0;
        loadOption.onValueChange += e =>
            Setting.audioLoadingStrategy = (int)e.value;

        new ConfigSpace(m_pluginConfigurator.rootPanel, 7f);

        var audioButtonArray = new ButtonArrayField(
            m_pluginConfigurator.rootPanel,
            "audio_button_array",
            3,
            new float[] { 0.4f, 0.4f, 0.2f },
            new string[] { "Reload", "Advance", "Credit" }
        );
        audioButtonArray.OnClickEventHandler(0).onClick += () 
            => AnnouncerManager.ReloadAllAnnouncers();

        audioButtonArray.OnClickEventHandler(1).onClick += () 
            => advancedPanel.OpenPanel();

        audioButtonArray.OnClickEventHandler(2).onClick += ()
            => creditPanel.OpenPanel();


    }

    private static void CreateAdvancedOptionPanel()
    {
        advancedPanel = new ConfigPanel(m_pluginConfigurator.rootPanel, "Advanced", "Advanced_Option");
        advancedPanel.hidden = true;
        new ConfigSpace(advancedPanel, 15f);

        lowpassToggle = new BoolField(
            advancedPanel,
            "LowPassFilter when under water",
            "LowPassFilter_Enabled",
            Setting.isLowPassFilterEnabled
        );
        lowpassToggle.defaultValue = true;
        lowpassToggle.onValueChange += (e) =>
            Setting.isLowPassFilterEnabled = e.value;

        ffmpegToggle = new BoolField(
            advancedPanel,
            "FFmpeg Support",
            "FFmpeg_Support",
            Setting.isFFmpegSupportEnabled
        );
        ffmpegToggle.defaultValue = false;
        ffmpegToggle.onValueChange += (e) =>
            Setting.isFFmpegSupportEnabled = e.value;

        var ffmpegLogHeader = new ConfigHeader(advancedPanel, "To Endabled this will try to load unknown audio/video to AudioClip via FFmpeg\n makesure there's a executable in environment path `ffmpeg` or `PATH`\n\n ");
        ffmpegLogHeader.tmpAnchor = TMPro.TextAlignmentOptions.Top;
        ffmpegLogHeader.textSize = 12;
        ffmpegLogHeader.textColor = m_greyColour;

        new ConfigSpace(m_pluginConfigurator.rootPanel, 15f);

        var emergencyHeader = new ConfigHeader(advancedPanel, "Emergency");
        emergencyHeader.textColor = m_RedColour;

        var stopAudioSourceButton = new ButtonField(advancedPanel, "Stop All Audio Source", "Stop_All_Audio_Source");
        stopAudioSourceButton.onClick += () 
            => AudioSourceManager.StopAllAudioSource();
    }
    
    private static void CreateCreditsPanel()
    {
        creditPanel = new ConfigPanel(m_pluginConfigurator.rootPanel, "Credits", "Credits");
        creditPanel.hidden = true;
        new ConfigSpace(creditPanel, 15f);
        var CreditTitle = new ConfigHeader(creditPanel, "Credits");
        CreditTitle.textColor = m_PurpleColour;

        new ConfigSpace(creditPanel, 20f);

        var credits = new ConfigHeader(creditPanel, "");
        credits.tmpAnchor = TMPro.TextAlignmentOptions.TopLeft;
        credits.textSize = 20;

        var creditText = new StringBuilder();
        creditText.AppendLine("Thanks:\nhttps://unsplash.com/photos/white-textile-on-brown-wooden-table-_kUxT8WkoeY?utm_source=unsplash&utm_medium=referral&utm_content=creditShareLink");
        creditText.AppendLine("\n- OSU! \n(https://osu.ppy.sh/) some UX are copied from lazer(YES EDIT EXTERNALLY), use .ini as config cuz `skin.ini` is pretty easy to edit");
        creditText.AppendLine("\n- Artless Games \n(https://space.bilibili.com/1237125233) 14 Minesweeper variants");
        creditText.AppendLine("\n- Announcer Mod \n(https://www.nexusmods.com/ultrakill/mods/54) I started of this project is just because the mod isn't update in the few days after revamp");
        creditText.AppendLine("\n- PluginConfigurator doc \n(https://github.com/eternalUnion/UKPluginConfigurator/wiki) Easy to use tbh (except changing font)");
        creditText.AppendLine("\n- Maxwell's puzzling demon \n(https://store.steampowered.com/app/2770160/) Fun puzzle game");
        creditText.AppendLine("\n- A mod that currently WIP \n(Advert Placeholder) Pretty FUN I would say");
        creditText.AppendLine("\n- 快乐萨卡兹厨, jamcame, 夕柱.");
        creditText.AppendLine("\n");
        creditText.AppendLine("\n- Everyone who used this mod.");
        creditText.AppendLine("\n\n\n\n");
        creditText.AppendLine("\n- FUCK YOU UNITY.");
        creditText.AppendLine("\n\n\n\n\n\n\n\n\n");
        creditText.AppendLine("Turn up the bass!\nTHWWWWOOOOOMPPPP DHGDHGDHGDHGDHGDHG BWOBWOBWOBWOB BWOBWOBWOBWOB BWOBWOBWOBWOBWOB BWAHH BWAHH BWAHH HMRMRMRMRMRMRMRMMR WOH WOH WOH- H H H HHHHH THWWOOMPPPP DHGDHGDHGDHGDHGDHG BWOBWOBWOBWOB.. BWOBWOBWOBWOB.. BWOBWOBWOBWOBWOB.... HRMMMMMMMMMMMMMMMMRRR- HRRRRRRR HRRRRRRRR HRRRRRR HRRRRRRRR HRRR HR-YOOOH!!!!!!\n\n- shadow of cats");
        credits.text = creditText.ToString();
    }

    private static void CreateAnnouncerSection()
    {
        var announcerHeader = new ConfigHeader(m_pluginConfigurator.rootPanel, "Announcer Section");
        announcerHeader.textColor = m_OrangeColour;

        audioLoaderLogHeader = new ConfigHeader(m_pluginConfigurator.rootPanel, "");
        audioLoaderLogHeader.tmpAnchor = TMPro.TextAlignmentOptions.TopLeft;
        audioLoaderLogHeader.textSize = 12;
        audioLoaderLogHeader.textColor = m_CyanColour;
    }

    // Currently this thing is not working
    private static void CreateDelegateTextFromBackEnd()
    {
        AudioLoader.onPluginConfiguratorLogUpdated = log =>
        {
            audioLoaderLogHeader.text = log + "\n";
        };
    }

    private static void AddAnnouncerPage(IAnnouncer announcer)
    {
        var newPage = new RegistedAnnouncerPage(announcer);
        _pages.Add(newPage);
    }
}

