using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GreyAnnouncer.FrontEnd;

public class Category : MonoBehaviour
{
    public Toggle enabledToggle;
    public Slider volumeSlider;
    public Slider cooldownSlider;

    private TextMeshProUGUI _titleText;
    private TextMeshProUGUI _volumeLabel;
    private TextMeshProUGUI _cooldownLabel;

    public void Awake()
    {
        gameObject.AddComponent<Image>().color = new Color(0.12f, 0.12f, 0.12f, 1f);

        var hlg = gameObject.AddComponent<HorizontalLayoutGroup>();
        hlg.childControlWidth      = true;
        hlg.childControlHeight     = false;
        hlg.childForceExpandWidth  = false;
        hlg.childForceExpandHeight = false;
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.spacing = 6;
        hlg.padding = new RectOffset(8, 8, 3, 3);
        gameObject.AddComponent<LayoutElement>().preferredHeight = 28;

        _titleText = UIBuilder.AddLabel(transform, "Category", 13, new Color(0f, 1f, 1f),
            preferredWidth: 110, flexibleWidth: 0,
            alignment: TextAlignmentOptions.MidlineLeft);

        // Enabled
        UIBuilder.AddLabel(transform, "En", 12, Color.white,
            preferredWidth: 18, flexibleWidth: 0,
            alignment: TextAlignmentOptions.MidlineLeft);
        enabledToggle = UIBuilder.AddToggle(transform, preferredWidth: 24);

        // Volume
        UIBuilder.AddLabel(transform, "Vol", 12, Color.white,
            preferredWidth: 24, flexibleWidth: 0,
            alignment: TextAlignmentOptions.MidlineLeft);
        volumeSlider  = AddSteppedSlider("VolumeSlider",  0f, 5f,  0.1f);
        _volumeLabel  = UIBuilder.AddLabel(transform, "1.0", 12, Color.white,
            preferredWidth: 32, flexibleWidth: 0,
            alignment: TextAlignmentOptions.MidlineLeft);
        volumeSlider.onValueChanged.AddListener(v => _volumeLabel.text = v.ToString("F1"));

        // Cooldown
        UIBuilder.AddLabel(transform, "CD", 12, Color.white,
            preferredWidth: 22, flexibleWidth: 0,
            alignment: TextAlignmentOptions.MidlineLeft);
        cooldownSlider  = AddSteppedSlider("CooldownSlider", 0f, 10f, 0.1f);
        _cooldownLabel  = UIBuilder.AddLabel(transform, "3.0", 12, Color.white,
            preferredWidth: 32, flexibleWidth: 0,
            alignment: TextAlignmentOptions.MidlineLeft);
        cooldownSlider.onValueChanged.AddListener(v => _cooldownLabel.text = v.ToString("F1"));
    }

    public void SetName(string name)   => _titleText.text = name;

    public void SetVolume(float v)
    {
        volumeSlider.SetValueWithoutNotify(v);
        _volumeLabel.text = v.ToString("F1");
    }

    public void SetCooldown(float v)
    {
        cooldownSlider.SetValueWithoutNotify(v);
        _cooldownLabel.text = v.ToString("F1");
    }

    // ── helpers ──────────────────────────────────────────────

    /// <summary>Slider that snaps to <paramref name="step"/> increments.</summary>
    private Slider AddSteppedSlider(string name, float min, float max, float step)
    {
        var s = UIBuilder.AddSlider(transform, min, min, max);
        s.gameObject.name  = name;
        s.wholeNumbers     = false;
        s.onValueChanged.AddListener(v => s.SetValueWithoutNotify(Mathf.Round(v / step) * step));
        return s;
    }
}
