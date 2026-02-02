using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    // singleton
    private static GameManager _instance;
    public static GameManager instance => _instance;

    [Header("Dont destroy on load objects")]
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject allCanvas;
    [SerializeField] private GameObject[] doors;

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
            DontDestroyOnLoad(player);
            DontDestroyOnLoad(EventSystem.current.gameObject);
            DontDestroyOnLoad(allCanvas);
            foreach (GameObject door in doors)
            {
                DontDestroyOnLoad(door);
            }
        }
    }
}
