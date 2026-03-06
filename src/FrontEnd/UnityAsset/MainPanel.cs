using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace GreyAnnouncer.FrontEnd;

public class MainPanel : MonoBehaviour
{
    public RectTransform rightPanelContent;
    public VideoPlayer videoPlayer;

    public void Awake()
    {
        // 1. 设置主面板全屏铺满
        var rt = GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>();
        SetFullStretch(rt);

        // 2. 创建左侧面板 (占 40%)
        CreateSubPanel("LeftPanel", 0.0f, 0.4f, new Color(0.2f, 0.2f, 0.2f, 0.8f));

        // 3. 创建右侧面板容器 (占 60%)
        GameObject rightPanel = CreateSubPanel("RightPanel", 0.4f, 1.0f, Color.black);
        
        // 4. 在右侧面板里初始化滚动系统
        SetupScrollView(rightPanel.transform);
        
        // 测试：往滚动列表里加点东西
        for(int i = 0; i < 20; i++) 
        {
            CreateTestItem($"Item {i}");
        }
    }

    private void SetupScrollView(Transform parent)
    {
        // --- A. 创建 ScrollRect 根物体 ---
        GameObject scrollObj = new GameObject("ScrollView", typeof(RectTransform), typeof(ScrollRect));
        scrollObj.transform.SetParent(parent, false);
        var scrollRT = scrollObj.GetComponent<RectTransform>();
        SetFullStretch(scrollRT); // 铺满右面板
        var scrollRect = scrollObj.GetComponent<ScrollRect>();

        // --- B. 创建 Viewport (遮罩层) ---
        GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D), typeof(Image));
        viewport.transform.SetParent(scrollObj.transform, false);
        var viewRT = viewport.GetComponent<RectTransform>();
        SetFullStretch(viewRT);
        // 稍微给右边留点空位放滚动条
        viewRT.offsetMax = new Vector2(-25, 0); 
        viewport.GetComponent<Image>().color = new Color(0,0,0,0); // 透明背景

        // --- C. 创建 Content (内容承载区) ---
        GameObject content = new GameObject("Content", typeof(RectTransform));
        content.transform.SetParent(viewport.transform, false);
        rightPanelContent = content.GetComponent<RectTransform>();
        
        // 设置 Content 顶部对齐
        rightPanelContent.anchorMin = new Vector2(0, 1);
        rightPanelContent.anchorMax = new Vector2(1, 1);
        rightPanelContent.pivot = new Vector2(0.5f, 1);
        rightPanelContent.offsetMin = Vector2.zero;
        rightPanelContent.offsetMax = Vector2.zero;

        // 【关键】让 Content 自动随子物体高度增长
        var vlg = content.AddComponent<VerticalLayoutGroup>();
        vlg.childControlHeight = true;
        vlg.childForceExpandHeight = false;
        vlg.spacing = 10;
        content.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // --- D. 创建 Scrollbar (那个你想要的拉条) ---
        GameObject sbObj = new GameObject("Scrollbar", typeof(RectTransform), typeof(Scrollbar), typeof(Image));
        sbObj.transform.SetParent(scrollObj.transform, false);
        var sbRT = sbObj.GetComponent<RectTransform>();
        sbRT.anchorMin = new Vector2(1, 0); // 靠右对齐
        sbRT.anchorMax = new Vector2(1, 1);
        sbRT.offsetMin = new Vector2(-20, 0); // 宽度 20
        sbRT.offsetMax = Vector2.zero;
        
        var sb = sbObj.GetComponent<Scrollbar>();
        sb.direction = Scrollbar.Direction.BottomToTop;
        sbObj.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.5f); // 槽的颜色

        // --- E. 创建 Scrollbar Handle (滑块) ---
        GameObject handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        handle.transform.SetParent(sbObj.transform, false);
        var handleRT = handle.GetComponent<RectTransform>();
        SetFullStretch(handleRT); // 初始化，Scrollbar 会接管它
        handle.GetComponent<Image>().color = Color.white; // 那个“能拉的东西”的颜色

        // --- F. 绑定引用 ---
        scrollRect.content = rightPanelContent;
        scrollRect.viewport = viewRT;
        scrollRect.verticalScrollbar = sb;
        sb.handleRect = handleRT;
        
        scrollRect.horizontal = false; // 关掉横向滚动
        scrollRect.vertical = true;
    }

    private void CreateTestItem(string text)
    {
        GameObject item = new GameObject(text, typeof(RectTransform), typeof(Image));
        item.transform.SetParent(rightPanelContent, false);
        item.GetComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f);
        item.AddComponent<LayoutElement>().minHeight = 50; // 每个条目高 50
    }

    private GameObject CreateSubPanel(string name, float anchorMinX, float anchorMaxX, Color color)
    {
        GameObject panelObj = new GameObject(name, typeof(RectTransform), typeof(Image));
        panelObj.transform.SetParent(transform, false);
        var rt = panelObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(anchorMinX, 0);
        rt.anchorMax = new Vector2(anchorMaxX, 1);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        panelObj.GetComponent<Image>().color = color;
        return panelObj;
    }

    private void SetFullStretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.localScale = Vector3.one;
    }
}