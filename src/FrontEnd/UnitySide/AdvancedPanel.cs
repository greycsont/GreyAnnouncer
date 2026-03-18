using UnityEngine;
using UnityEngine.UI;
using GreyAnnouncer.AudioSourceComponent;
using GreyAnnouncer.Config;

namespace GreyAnnouncer.FrontEnd;

public class AdvancedPanel : MonoBehaviour
{
    private Toggle _lpToggle;
    private Toggle _ffToggle;

    public void SyncUI()
    {
        _lpToggle.SetIsOnWithoutNotify(Setting.isLowPassFilterEnabled);
        _ffToggle.SetIsOnWithoutNotify(Setting.isFFmpegSupportEnabled);
    }

    public void Awake()
    {
        UIBuilder.SetFullStretch(GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>());

        var (_, content) = UIBuilder.BuildScrollView(transform, padding: new RectOffset(12, 12, 12, 12));

        UIBuilder.AddLabel(content, "Advanced", 26, new Color(0.5f, 0.8f, 1f), preferredHeight: 23);
        UIBuilder.AddSeparator(content);
        UIBuilder.AddSpace(content, 6);

        // LowPass Filter toggle
        var lpRow = UIBuilder.AddRow(content, "LowPass_Row", height: 28).transform;
        UIBuilder.AddLabel(lpRow, "LowPass Filter when under water", 13, Color.white, flexibleWidth: 1);
        _lpToggle = UIBuilder.AddToggle(lpRow, preferredWidth: 30);
        _lpToggle.isOn = Setting.isLowPassFilterEnabled;
        _lpToggle.onValueChanged.AddListener(v => Setting.isLowPassFilterEnabled = v);

        UIBuilder.AddSpace(content, 4);

        // FFmpeg toggle
        var ffRow = UIBuilder.AddRow(content, "FFmpeg_Row", height: 28).transform;
        UIBuilder.AddLabel(ffRow, "FFmpeg Support", 13, Color.white, flexibleWidth: 1);
        _ffToggle = UIBuilder.AddToggle(ffRow, preferredWidth: 30);
        _ffToggle.isOn = Setting.isFFmpegSupportEnabled;
        _ffToggle.onValueChanged.AddListener(v => Setting.isFFmpegSupportEnabled = v);

        UIBuilder.AddLabel(content,
            "Enables loading unknown audio/video via FFmpeg.\nMake sure 'ffmpeg' is available in PATH.",
            11, UIBuilder.SubLabelColor, preferredHeight: 38);

        UIBuilder.AddSpace(content, 10);
        UIBuilder.AddSeparator(content);
        UIBuilder.AddSpace(content, 4);

        UIBuilder.AddLabel(content, "Emergency", 13, new Color(1f, 0.3f, 0.3f), preferredHeight: 20);
        UIBuilder.AddButton(content, "Stop All Audio Sources", fontSize: 13)
            .onClick.AddListener(AudioSourceManager.StopAllAudioSource);

        Setting.syncUI += SyncUI;
    }

    private void OnDestroy()
    {
        Setting.syncUI -= SyncUI;
    }
}
