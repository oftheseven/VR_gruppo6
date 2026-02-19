using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UI_InfoPanel : MonoBehaviour
{
    [Header("UI elements")]
    [SerializeField] private RawImage closeImage;

    [Header("Blinking")]
    [SerializeField] private float blinkSpeed = 2f;
    private float blinkTimer = 0f;

    private bool isOpen = false;
    public bool IsOpen => isOpen;
    private bool isFirstTime = true;
    public bool IsFirstTime => isFirstTime;

    void Awake()
    {
        this.gameObject.SetActive(false);
    }

    void Update()
    {
        if (isOpen && closeImage != null)
        {
            blinkTimer += Time.deltaTime * blinkSpeed;
            float alpha = Mathf.Lerp(0.2f, 1f, (Mathf.Sin(blinkTimer * Mathf.PI) + 1f) / 2f);
            var c = closeImage.color;
            c.a = alpha;
            closeImage.color = c;
        }
        else if (closeImage != null)
        {
            var c = closeImage.color;
            c.a = 1f;
            closeImage.color = c;
        }
    }

    public void HandleInfoPanel()
    {
        if (Keyboard.current.tabKey.wasPressedThisFrame && this != null && !this.IsOpen)
        {
            OpenInfoPanel();
        }
        else if (Keyboard.current.eKey.wasPressedThisFrame && this != null && this.IsOpen)
        {
            CloseInfoPanel();
        }
    }

    public void OpenInfoPanel()
    {
        // controllo che il pannello di info sia associato
        if (this.gameObject != null)
        {
            this.gameObject.SetActive(true);
            isOpen = true;
        }
        else if (this.gameObject == null)
        {
            Debug.LogWarning("Info panel GameObject is null.");
        }

        PlayerController.ShowCursor();
    }

    public void CloseInfoPanel()
    {
        if (this.gameObject != null)
        {
            this.gameObject.SetActive(false);
            isOpen = false;
            if (closeImage != null)
            {
                var c = closeImage.color;
                c.a = 1f;
                closeImage.color = c;
            }
        }

        PlayerController.HideCursor();
    }

    public void OnDeviceOpened()
    {
        if (isFirstTime)
        {
            // prima volta: mostro automaticamente il tutorial
            OpenInfoPanel();
            isFirstTime = false;
        }
        else
        {
            if (this.gameObject != null && this.gameObject.activeSelf)
            {
                this.gameObject.SetActive(false);
                isOpen = false;
            }
        }
    }

    public void OnDeviceClosed()
    {
        if (isOpen)
        {
            CloseInfoPanel();
        }
    }
}
