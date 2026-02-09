using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;

public class UI_SliderPanel : MonoBehaviour
{
    [Header("Slider reference")]
    private InteractableSlider currentSlider;

    [Header("UI references")]
    [SerializeField] private TextMeshProUGUI positionText;
    [SerializeField] private TextMeshProUGUI percentageText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Button resetButton;

    [Header("Movement controls")]
    [SerializeField] private float keyboardMoveSpeed = 0.5f;

    [Header("Hold to close")]
    [SerializeField] private GameObject holdIndicator;
    [SerializeField] private Image holdFillImage;
    [SerializeField] private float holdTimeToClose = 2f;

    [Header("Info panel")]
    [SerializeField] private UI_InfoPanel infoPanel;

    private bool isOpen = false;
    public bool IsOpen => isOpen;
    private bool canInteract = true;
    public bool CanInteract => canInteract;
    private float holdTimer = 0f;

    void Start()
    {
        this.gameObject.SetActive(false);

        if (holdIndicator != null)
        {
            holdIndicator.SetActive(false);
        }

        if (resetButton != null)
        {
            resetButton.onClick.AddListener(ResetToCenter);
        }
    }

    void Update()
    {
        if (isOpen)
        {
            HandleKeyboardMovement();
            HandlePanelClose();
            UpdateUI();
        }
    }

    public void OpenPanel(InteractableSlider slider)
    {
        if (slider == null)
        {
            Debug.LogError("InteractableSlider è null!");
            return;
        }

        currentSlider = slider;
        this.gameObject.SetActive(true);
        isOpen = true;

        PlayerController.EnableMovement(false);
        PlayerController.ShowCursor();

        // Attiva camera slider
        if (PlayerController.instance != null)
        {
            PlayerController.instance.playerCamera.gameObject.SetActive(false);
        }

        if (currentSlider.SliderCamera != null)
        {
            currentSlider.SliderCamera.gameObject.SetActive(true);
        }

        if (infoPanel != null)
        {
            infoPanel.OnDeviceOpened();
        }

        UpdateUI();
    }

    public void ClosePanel()
    {
        isOpen = false;
        holdTimer = 0f;

        if (holdIndicator != null)
        {
            holdIndicator.SetActive(false);
        }

        if (infoPanel != null)
        {
            infoPanel.OnDeviceClosed();
        }

        if (currentSlider != null && currentSlider.SliderCamera != null)
        {
            currentSlider.SliderCamera.gameObject.SetActive(false);
        }

        if (PlayerController.instance != null)
        {
            PlayerController.instance.playerCamera.gameObject.SetActive(true);
        }

        currentSlider = null;

        PlayerController.HideCursor();
        PlayerController.EnableMovement(true);

        StartCoroutine(CooldownAndHide());
    }

    private void UpdateUI()
    {
        if (currentSlider == null) return;

        if (nameText != null)
        {
            nameText.text = currentSlider.SliderName;
        }

        if (positionText != null)
        {
            float distance = currentSlider.GetDistanceInMeters();
            float total = currentSlider.GetTotalLength();
            positionText.text = $"{distance:F1}m / {total:F1}m";
        }

        if (percentageText != null)
        {
            percentageText.text = $"{(currentSlider.CurrentPosition * 100):F0}%";
        }
    }

    private void HandleKeyboardMovement()
    {
        if (currentSlider == null) return;
        if (infoPanel != null && infoPanel.IsOpen) return;

        float movement = 0f;

        if (Keyboard.current.aKey.isPressed)
        {
            movement = -keyboardMoveSpeed * Time.deltaTime;
        }
        else if (Keyboard.current.dKey.isPressed)
        {
            movement = keyboardMoveSpeed * Time.deltaTime;
        }

        if (movement != 0f)
        {
            currentSlider.MoveSlider(movement);
        }
    }

    private void ResetToCenter()
    {
        if (currentSlider == null) return;
        currentSlider.SetPosition(0.5f);
    }

    public void HandlePanelClose()
    {
        if (Keyboard.current.eKey.isPressed && (infoPanel == null || !infoPanel.IsOpen))
        {
            holdTimer += Time.deltaTime;

            if (holdIndicator != null && !holdIndicator.activeSelf)
            {
                holdIndicator.SetActive(true);
            }

            if (holdFillImage != null)
            {
                holdFillImage.fillAmount = Mathf.Clamp01(holdTimer / holdTimeToClose);
            }

            if (holdTimer >= holdTimeToClose)
            {
                ClosePanel();
            }
        }
        else
        {
            holdTimer = 0f;

            if (holdIndicator != null)
            {
                holdIndicator.SetActive(false);
            }

            if (holdFillImage != null)
            {
                holdFillImage.fillAmount = 0;
            }
        }
    }

    private IEnumerator CooldownAndHide()
    {
        canInteract = false;
        this.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.1f);
        canInteract = true;
    }
}