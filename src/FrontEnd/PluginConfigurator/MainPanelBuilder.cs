using UnityEngine;
using System;
using System.Text;

using PluginConfig.API;
using PluginConfig.API.Fields;
using PluginConfig.API.Decorators;
using PluginConfig.API.Functionals;

using GreyAnnouncer.AnnouncerAPI;
using GreyAnnouncer.AudioSourceComponent;
using GreyAnnouncer.Config;


namespace GreyAnnouncer.FrontEnd;

// Chaotic
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


    public static void Build(PluginConfigurator config)
    {
        m_pluginConfigurator = config;

        CreateMainSettingSectionTitle();
        CreateAudioControls();
        CreateAdvancedOptionPanel();
        CreateCreditsPanel();
        CreateAnnouncerSection();
        

        CreateDelegateTextFromBackEnd();
    }

    private static void CreateMainSettingSectionTitle()
    {
        new ConfigSpace(m_pluginConfigurator.rootPanel, 15f);

        ConfigHeader mainHeader = new ConfigHeader(m_pluginConfigurator.rootPanel, "Main Settings");
        mainHeader.textColor = m_CyanColour;

    }


    private static void CreateAudioControls()
    {

        var volumeSlider = new FloatSliderField(
            m_pluginConfigurator.rootPanel,
            "Master Volume",
            "Audio_Volume",
            Tuple.Create(0f, 1f),
            BepInExConfig.audioSourceVolume.Value,
            2   // 2nd decimal
        );
        volumeSlider.defaultValue = BepInExConfig.DEFAULT_AUDIO_SOURCE_VOLUME;
        volumeSlider.onValueChange += e =>
        {
            BepInExConfig.audioSourceVolume.Value = e.newValue;
            SoloAudioSource.Instance.UpdateSoloAudioSourceVolume(e.newValue);
        };

        // It worked, but not working great as there's ton of audio when from low rank directly to the high rank
        // May be add a short cooldown as limitation

        var playOption = new EnumField<PlayOptions>(
            m_pluginConfigurator.rootPanel,
            "Audio Play Strategy",
            "Audio_Play_Strategy",
            (PlayOptions)BepInExConfig.audioPlayOptions.Value
        );
        playOption.defaultValue = (PlayOptions)BepInExConfig.DEFAULT_AUDIO_PLAY_OPTIONS;
        playOption.onValueChange += e =>
        {
            BepInExConfig.audioPlayOptions.Value = (int)e.value;
        };

        var loadingOption = new EnumField<audioLoadingOptions>(
            m_pluginConfigurator.rootPanel,
            "Audio Load Strategy",
            "Audio_Load_Strategy",
            (audioLoadingOptions)BepInExConfig.audioLoadingStategy.Value
        );
        loadingOption.defaultValue = (audioLoadingOptions)BepInExConfig.DEFAULT_AUDIO_LOADING_OPTIONS;
        loadingOption.onValueChange += e =>
        {
            BepInExConfig.audioLoadingStategy.Value = (int)e.value;
            if (e.value.Equals((audioLoadingOptions.Load_then_Play)))
            {
                LogHelper.LogInfo("Clear audio clip cache");
                ClearAudioClipsCache();
            }
            if (e.value.Equals(audioLoadingOptions.Preload_and_Play))
            {
                LogHelper.LogInfo("Reloading all announcer audio");
                ReloadAllAnnouncers();
            }
        };

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

        var lowpassToggle = new BoolField(
            advancedPanel,
            "LowPassFilter when under water",
            "LowPassFilter_Enabled",
            BepInExConfig.isLowPassFilterEnabled.Value
        );
        lowpassToggle.defaultValue = true;
        lowpassToggle.onValueChange += (e) =>
        {
            BepInExConfig.isLowPassFilterEnabled.Value = e.value;
            UnderwaterController_inWater_Instance.CheckIsInWater();
        };

        var ffmpegToggle = new BoolField(
            advancedPanel,
            "FFmpeg Support",
            "FFmpeg_Support",
            BepInExConfig.isFFmpegSupportEnabled.Value
        );
        ffmpegToggle.defaultValue = false;
        ffmpegToggle.onValueChange += (e) =>
        {
            BepInExConfig.isFFmpegSupportEnabled.Value = e.value;
            LogHelper.LogInfo($"Switched FFmpeg support : {e.value}");
        };

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

    #region enum
    private enum PlayOptions
    {
        Override_Last = 0,
        Independent = 1
    }

    private enum audioLoadingOptions
    {
        Load_then_Play = 0,
        Preload_and_Play = 1
    }
    #endregion


    #region function
    private static void ReloadAllAnnouncers()
    {
        audioLoaderLogHeader.text = string.Empty;
        AnnouncerManager.ReloadAllAnnouncers();
    }

    private static void ClearAudioClipsCache()
    {
        audioLoaderLogHeader.text = string.Empty;
        AnnouncerManager.ClearAudioClipsCache();
    }
    #endregion
}

