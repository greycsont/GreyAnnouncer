


using UnityEngine;

public class Category : MonoBehaviour
{
    public void Awake()
    {
        var rt = GetComponent<RectTransform>() ?? gameObject.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0.3f);
        rt.anchorMax = new Vector2(1f, 0.7f);
        
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        
        rt.localScale = Vector3.one;
        rt.localPosition = Vector3.zero;
    }
}