using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
        hlg.childControlWidth = true;
        hlg.childControlHeight = false;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        hlg.spacing = 6;
        hlg.padding = new RectOffset(8, 8, 3, 3);
        gameObject.AddComponent<LayoutElement>().preferredHeight = 28;

        _titleText = AddText("CategoryTitle", "Category", 13, new Color(0f, 1f, 1f), 110);

        // Enabled
        AddText("EnabledLabel", "En", 12, Color.white, 18);
        enabledToggle = AddToggle();

        // Volume
        AddText("VolumeLabel", "Vol", 12, Color.white, 24);
        volumeSlider = AddSlider("VolumeSlider", 0f, 5f, 0.1f);
        _volumeLabel = AddText("VolumeValue", "1.0", 12, Color.white, 32);
        volumeSlider.onValueChanged.AddListener(v => _volumeLabel.text = v.ToString("F1"));

        // Cooldown
        AddText("CooldownLabel", "CD", 12, Color.white, 22);
        cooldownSlider = AddSlider("CooldownSlider", 0f, 10f, 0.1f);
        _cooldownLabel = AddText("CooldownValue", "3.0", 12, Color.white, 32);
        cooldownSlider.onValueChanged.AddListener(v => _cooldownLabel.text = v.ToString("F1"));
    }

    public void SetName(string name) => _titleText.text = name;

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

    // ── helpers ─────────────────────────────────────────────

    private TextMeshProUGUI AddText(string name, string text, int size, Color color, float width)
    {
        var obj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        obj.transform.SetParent(transform, false);
        var le = obj.AddComponent<LayoutElement>();
        le.preferredWidth = width;
        le.flexibleWidth = 0;
        var tmp = obj.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        return tmp;
    }

    private Toggle AddToggle()
    {
        var obj = DefaultControls.CreateToggle(new DefaultControls.Resources());
        obj.name = "EnabledToggle";
        obj.transform.SetParent(transform, false);
        var le = obj.AddComponent<LayoutElement>();
        le.preferredWidth = 24;
        le.flexibleWidth = 0;
        return obj.GetComponent<Toggle>();
    }

    private Slider AddSlider(string name, float min, float max, float step)
    {
        var obj = DefaultControls.CreateSlider(new DefaultControls.Resources());
        obj.name = name;
        obj.transform.SetParent(transform, false);
        obj.AddComponent<LayoutElement>().flexibleWidth = 1;
        var s = obj.GetComponent<Slider>();
        s.minValue = min;
        s.maxValue = max;
        s.wholeNumbers = false;
        s.onValueChanged.AddListener(v => s.SetValueWithoutNotify(Mathf.Round(v / step) * step));
        return s;
    }
}
