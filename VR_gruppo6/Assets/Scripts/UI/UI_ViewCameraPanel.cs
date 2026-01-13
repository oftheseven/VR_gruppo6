using UnityEngine;

public class UI_ViewCameraPanel : MonoBehaviour
{
    // singleton
    private static UI_ViewCameraPanel _instance;
    public static UI_ViewCameraPanel instance => _instance;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    void Start()
    {
        this.gameObject.SetActive(false);
    }
}