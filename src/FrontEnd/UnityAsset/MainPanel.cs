

using UnityEngine;

namespace GreyAnnouncer.FrontEnd;

public class MainPanel : MonoBehaviour
{
    public void Awake()
    {
        var rt = GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        
        rt.localScale = Vector3.one;
        rt.localPosition = Vector3.zero;
    }
}