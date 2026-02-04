using UnityEngine;

public class UI_BasePanel : MonoBehaviour
{
    // singleton
    private static UI_BasePanel _instance;
    public static UI_BasePanel instance => _instance;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
        }
    }
}