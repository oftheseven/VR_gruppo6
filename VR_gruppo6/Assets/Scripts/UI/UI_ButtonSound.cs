using UnityEngine;
using UnityEngine.UI;

public class UI_ButtonSound : MonoBehaviour
{
    [Header("Custom Sounds (opzionale)")]
    [SerializeField] private AudioClip customClickSound;
    
    private Button button;
    
    void Awake()
    {
        button = GetComponent<Button>();
        
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClick);
        }
    }
    
    private void OnButtonClick()
    {
        if (AudioManager.instance != null)
        {
            if (customClickSound != null)
            {
                AudioManager.instance.PlayUISound(customClickSound);
            }
            else
            {
                AudioManager.instance.PlayUIClick();
            }
        }
    }
}