using UnityEngine;


namespace greycsont.GreyAnnouncer
{
    /// <summary>
    /// A helper MonoBehaviour, used to start a coroutine to load audio
    /// </summary>
    public class CoroutineRunner : MonoBehaviour
    {
        private static CoroutineRunner _instance;
        public static CoroutineRunner  Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("CoroutineRunner");
                    _instance = go.AddComponent<CoroutineRunner>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }
    }
}