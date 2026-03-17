using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GreyAnnouncer.FrontEnd;

/// <summary>Shared factory methods and style constants for Unity-side UI.</summary>
public static class UIBuilder
{
    // ── Style palette ─────────────────────────────────────────
    public static readonly Color NavActive      = new(0.25f, 0.45f, 0.65f);
    public static readonly Color NavInactive    = new(0.22f, 0.22f, 0.22f);
    public static readonly Color SeparatorColor = new(0.40f, 0.40f, 0.40f);
    public static readonly Color SubLabelColor  = new(0.70f, 0.70f, 0.70f);

    // ── Layout ────────────────────────────────────────────────

    public static void SetFullStretch(RectTransform rt)
    {
        rt.anchorMin   = Vector2.zero;
        rt.anchorMax   = Vector2.one;
        rt.offsetMin   = Vector2.zero;
        rt.offsetMax   = Vector2.zero;
        rt.localScale  = Vector3.one;
    }

    /// <summary>Horizontal row with a LayoutElement height constraint.</summary>
    public static GameObject AddRow(Transform parent, string name, float height = 30, float spacing = 8)
    {
        var row = new GameObject(name, typeof(RectTransform));
        row.transform.SetParent(parent, false);
        var hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.childControlWidth     = true;
        hlg.childControlHeight    = true;
        hlg.childForceExpandWidth  = false;
        hlg.childForceExpandHeight = false;
        hlg.spacing = spacing;
        row.AddComponent<LayoutElement>().preferredHeight = height;
        return row;
    }

    // ── Text ──────────────────────────────────────────────────

    public static TextMeshProUGUI AddLabel(
        Transform parent, string text, int fontSize, Color color,
        float preferredHeight = -1, float preferredWidth = -1, float flexibleWidth = -1,
        TextAlignmentOptions alignment = TextAlignmentOptions.Left)
    {
        var obj = new GameObject("Label_" + text, typeof(RectTransform), typeof(TextMeshProUGUI));
        obj.transform.SetParent(parent, false);
        var le = obj.AddComponent<LayoutElement>();
        if (preferredHeight >= 0) le.preferredHeight = preferredHeight;
        if (preferredWidth  >= 0) le.preferredWidth  = preferredWidth;
        if (flexibleWidth   >= 0) le.flexibleWidth   = flexibleWidth;
        var tmp = obj.GetComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = fontSize;
        tmp.color     = color;
        tmp.alignment = alignment;
        return tmp;
    }

    // ── Button ────────────────────────────────────────────────

    public static Button AddButton(Transform parent, string label, int fontSize = 14, float height = 36)
    {
        var obj = DefaultControls.CreateButton(new DefaultControls.Resources());
        obj.name = "Btn_" + label;
        obj.transform.SetParent(parent, false);
        obj.AddComponent<LayoutElement>().preferredHeight = height;
        SetButtonLabel(obj, label, fontSize);
        return obj.GetComponent<Button>();
    }

    public static void StyleButton(GameObject btnObj, bool active)
    {
        if (btnObj.GetComponent<Image>() is { } img)
            img.color = active ? NavActive : NavInactive;
    }

    public static void SetButtonLabel(GameObject btnObj, string text, int fontSize)
    {
        if (btnObj.GetComponentInChildren<TextMeshProUGUI>() is { } tmp)
            { tmp.text = text; tmp.fontSize = fontSize; return; }
        if (btnObj.GetComponentInChildren<Text>() is { } legacy)
            { legacy.text = text; legacy.fontSize = fontSize; }
    }

    // ── Toggle ────────────────────────────────────────────────

    public static Toggle AddToggle(Transform parent, float preferredWidth = 24, float preferredHeight = -1)
    {
        var obj = DefaultControls.CreateToggle(new DefaultControls.Resources());
        obj.transform.SetParent(parent, false);
        var le = obj.AddComponent<LayoutElement>();
        le.preferredWidth = preferredWidth;
        le.flexibleWidth  = 0;
        if (preferredHeight >= 0) le.preferredHeight = preferredHeight;
        return obj.GetComponent<Toggle>();
    }

    // ── Slider ────────────────────────────────────────────────

    public static Slider AddSlider(Transform parent, float value, float min, float max,
        float preferredHeight = 20, float preferredWidth = -1)
    {
        var obj = DefaultControls.CreateSlider(new DefaultControls.Resources());
        obj.transform.SetParent(parent, false);
        var le = obj.AddComponent<LayoutElement>();
        le.preferredHeight = preferredHeight;
        if (preferredWidth >= 0) { le.preferredWidth = preferredWidth; le.flexibleWidth = 0; }
        else le.flexibleWidth = 1;
        var s = obj.GetComponent<Slider>();
        s.minValue = min;
        s.maxValue = max;
        s.value    = value;
        return s;
    }

    // ── Dropdown ──────────────────────────────────────────────

    public static Dropdown AddDropdown(Transform parent, List<string> options, float height = 36)
    {
        var obj = DefaultControls.CreateDropdown(new DefaultControls.Resources());
        obj.transform.SetParent(parent, false);
        obj.AddComponent<LayoutElement>().preferredHeight = height;
        var dd = obj.GetComponent<Dropdown>();
        dd.ClearOptions();
        dd.AddOptions(options);
        return dd;
    }

    // ── Space / Separator ─────────────────────────────────────

    public static void AddSpace(Transform parent, float height)
    {
        var obj = new GameObject("Space", typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        obj.AddComponent<LayoutElement>().preferredHeight = height;
    }

    public static void AddSeparator(Transform parent)
    {
        var obj = new GameObject("Separator", typeof(RectTransform), typeof(Image));
        obj.transform.SetParent(parent, false);
        obj.AddComponent<LayoutElement>().preferredHeight = 1;
        obj.GetComponent<Image>().color = SeparatorColor;
    }

    // ── Scroll view ───────────────────────────────────────────

    public static (ScrollRect scrollRect, Transform content) BuildScrollView(
        Transform parent, RectOffset padding = null, bool addScrollbar = true)
    {
        const float sbWidth = 20;

        var scrollObj = new GameObject("ScrollView", typeof(RectTransform), typeof(ScrollRect));
        scrollObj.transform.SetParent(parent, false);
        SetFullStretch(scrollObj.GetComponent<RectTransform>());
        var scrollRect = scrollObj.GetComponent<ScrollRect>();

        var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D), typeof(Image));
        viewport.transform.SetParent(scrollObj.transform, false);
        var viewRT = viewport.GetComponent<RectTransform>();
        SetFullStretch(viewRT);
        if (addScrollbar) viewRT.offsetMax = new Vector2(-sbWidth, 0);
        viewport.GetComponent<Image>().color = new Color(0, 0, 0, 0);

        var contentObj = new GameObject("Content", typeof(RectTransform));
        contentObj.transform.SetParent(viewport.transform, false);
        var contentRT = contentObj.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1);
        contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot     = new Vector2(0.5f, 1);
        contentRT.offsetMin = Vector2.zero;
        contentRT.offsetMax = Vector2.zero;

        var vlg = contentObj.AddComponent<VerticalLayoutGroup>();
        vlg.childControlWidth      = true;
        vlg.childControlHeight     = true;
        vlg.childForceExpandWidth  = true;
        vlg.childForceExpandHeight = false;
        vlg.spacing = 8;
        vlg.padding = padding ?? new RectOffset(12, 12, 12, 12);
        contentObj.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.content   = contentRT;
        scrollRect.viewport  = viewRT;
        scrollRect.horizontal = false;
        scrollRect.vertical   = true;

        if (addScrollbar)
        {
            var sbObj = new GameObject("Scrollbar", typeof(RectTransform), typeof(Scrollbar), typeof(Image));
            sbObj.transform.SetParent(scrollObj.transform, false);
            var sbRT = sbObj.GetComponent<RectTransform>();
            sbRT.anchorMin = new Vector2(1, 0);
            sbRT.anchorMax = new Vector2(1, 1);
            sbRT.offsetMin = new Vector2(-sbWidth, 0);
            sbRT.offsetMax = Vector2.zero;
            sbObj.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.6f);

            var sb = sbObj.GetComponent<Scrollbar>();
            sb.direction = Scrollbar.Direction.BottomToTop;

            var handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handle.transform.SetParent(sbObj.transform, false);
            SetFullStretch(handle.GetComponent<RectTransform>());
            handle.GetComponent<Image>().color = new Color(0.6f, 0.6f, 0.6f);

            scrollRect.verticalScrollbar = sb;
            sb.handleRect = handle.GetComponent<RectTransform>();
        }

        return (scrollRect, contentObj.transform);
    }
}
