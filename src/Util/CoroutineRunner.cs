using UnityEngine;


namespace GreyAnnouncer.Util;

/// <summary>
/// A helper MonoBehaviour, used to start a coroutine to load audio
/// </summary>
public sealed class CoroutineRunner : MonoBehaviour
{
    public static CoroutineRunner Instance
    {
        get
        {
            if (field == null)
            {
                GameObject go = new GameObject("CoroutineRunner");
                field = go.AddComponent<CoroutineRunner>();
                DontDestroyOnLoad(go);
            }
            return field;
        }
    }

}
