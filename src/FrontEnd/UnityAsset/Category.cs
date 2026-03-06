using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Category : MonoBehaviour
{
    // 这里的变量会在创建后被赋值，方便后续引用
    public TextMeshProUGUI titleText;
    public Toggle boolToggle;
    public Slider floatSlider1;
    public Slider floatSlider2;

    public void Awake()
    {
        // 1. 设置自身容器 (Category) 的布局
        var rt = GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0.3f);
        rt.anchorMax = new Vector2(1f, 0.7f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // 2. 创建标题 (TextMeshPro)
        GameObject titleObj = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleObj.transform.SetParent(transform, false);
        titleText = titleObj.GetComponent<TextMeshProUGUI>();
        titleText.text = "Mod Category Title";
        titleText.fontSize = 24;
        titleText.alignment = TextAlignmentOptions.Center;
        SetRect(titleObj.GetComponent<RectTransform>(), new Vector2(0.5f, 0.9f), new Vector2(200, 50));

        // 3. 创建布尔开关 (Toggle)
        // 使用内置工具创建基础 UI 结构
        DefaultControls.Resources res = new DefaultControls.Resources(); 
        GameObject toggleObj = DefaultControls.CreateToggle(res);
        toggleObj.transform.SetParent(transform, false);
        boolToggle = toggleObj.GetComponent<Toggle>();
        SetRect(toggleObj.GetComponent<RectTransform>(), new Vector2(0.5f, 0.7f), new Vector2(100, 30));

        // 4. 创建两个滑动条 (Slider)
        floatSlider1 = CreateSlider("Slider_1", new Vector2(0.5f, 0.5f));
        floatSlider2 = CreateSlider("Slider_2", new Vector2(0.5f, 0.3f));
    }

    // 辅助方法：创建一个 Slider 并设置位置
    private Slider CreateSlider(string name, Vector2 anchorPos)
    {
        DefaultControls.Resources res = new DefaultControls.Resources();
        GameObject sliderObj = DefaultControls.CreateSlider(res);
        sliderObj.name = name;
        sliderObj.transform.SetParent(transform, false);
        
        RectTransform rt = sliderObj.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = anchorPos;
        rt.sizeDelta = new Vector2(160, 20);
        
        return sliderObj.GetComponent<Slider>();
    }

    // 辅助方法：快速设置 RectTransform
    private void SetRect(RectTransform rt, Vector2 anchorPos, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = anchorPos;
        rt.sizeDelta = size;
        rt.anchoredPosition = Vector2.zero;
    }
}