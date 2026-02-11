using UnityEngine;

public class PersistentObjects : MonoBehaviour
{
    private static PersistentObjects _instance;
    public static PersistentObjects instance => _instance;

    [Header("Objects to persist across scenes")]
    [SerializeField] private GameObject[] objectsToPersist;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
            foreach (var obj in objectsToPersist)
            {
                if (obj != null)
                {
                    DontDestroyOnLoad(obj);
                }
            }
        }
    }
}