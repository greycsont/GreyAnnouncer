using UnityEngine;


namespace greycsont.GreyAnnouncer;

/// <summary>
/// A helper MonoBehaviour, used to start a coroutine to load audio
/// </summary>
public sealed class CoroutineRunner : MonoBehaviour
{
    private static CoroutineRunner m_instance;
    public static CoroutineRunner  Instance
    {
        get
        {
            if (m_instance == null)
            {
                GameObject go = new GameObject("CoroutineRunner");
                m_instance    = go.AddComponent<CoroutineRunner>();
                DontDestroyOnLoad(go);
            }
            return m_instance;
        }
    }
}
