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
    [SerializeField] private TextMeshProUGUI controlHintText; // hint per l'utente per capire quali controlli usare
    [SerializeField] private Button resetButton;

    [Header("Movement controls")]
    [SerializeField] private float keyboardMoveSpeed = 0.5f;
    [SerializeField] private float angleThreshold = 45f;

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
            HandleCameraRotation();
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
        canInteract = true;
    }

    private void UpdateUI()
    {
        if (currentSlider == null) return;

        if (positionText != null)
        {
            float distance = currentSlider.GetDistanceInMeters();
            float total = currentSlider.GetTotalLength();
            positionText.text = $"{distance:F1}m / {total:F1}m";
        }

        if (controlHintText != null && currentSlider.SliderCamera != null)
        {
            bool useWSControls = ShouldUseWSControls();
            controlHintText.text = useWSControls ? "W/S: Muovi slider | ←→: Ruota camera" : "A/D: Muovi slider | ←→: Ruota camera";
        }
    }

    private void HandleCameraRotation()
    {
        if (currentSlider == null || currentSlider.SliderCamera == null) return;
        if (infoPanel != null && infoPanel.IsOpen) return;

        float horizontal = 0f;
        float vertical = 0f;

        if (Keyboard.current.leftArrowKey.isPressed)
        {
            horizontal = -1f;
        }
        else if (Keyboard.current.rightArrowKey.isPressed)
        {
            horizontal = 1f;
        }

        if (Keyboard.current.upArrowKey.isPressed)
        {
            vertical = -1f;
        }
        else if (Keyboard.current.downArrowKey.isPressed)
        {
            vertical = 1f;
        }

        if (horizontal != 0f || vertical != 0f)
        {
            currentSlider.RotateCamera(horizontal, vertical);
        }
    }

    private void HandleKeyboardMovement()
    {
        if (currentSlider == null) return;
        if (infoPanel != null && infoPanel.IsOpen) return;

        float movement = 0f;
        
        bool useWSControls = ShouldUseWSControls();

        if (useWSControls)
        {
            // camera parallela ai binari => W/S
            if (Keyboard.current.wKey.isPressed)
            {
                movement = keyboardMoveSpeed * Time.deltaTime;
            }
            else if (Keyboard.current.sKey.isPressed)
            {
                movement = -keyboardMoveSpeed * Time.deltaTime;
            }
        }
        else
        {
            // camera ortogonale ai binari => A/D
            if (Keyboard.current.dKey.isPressed)
            {
                movement = keyboardMoveSpeed * Time.deltaTime;
            }
            else if (Keyboard.current.aKey.isPressed)
            {
                movement = -keyboardMoveSpeed * Time.deltaTime;
            }
        }

        if (movement != 0f)
        {
            currentSlider.MoveSlider(movement);
        }
    }

    private bool ShouldUseWSControls()
    {
        if (currentSlider == null || currentSlider.SliderCamera == null)
        {
            return true;
        }

        Vector3 cameraForward = currentSlider.SliderCamera.transform.forward;
        cameraForward.y = 0f;
        cameraForward.Normalize();

        Vector3 railDirection = currentSlider.RailDirection();
        railDirection.y = 0f;
        railDirection.Normalize();

        float angle = Vector3.Angle(cameraForward, railDirection);


        return angle < angleThreshold || angle > (180f - angleThreshold);
    }

    private void ResetToCenter()
    {
        if (currentSlider == null) return;
        currentSlider.SetPosition(0.5f);
        if (currentSlider.SliderCamera != null)
        {
            currentSlider.SliderCamera.transform.localRotation = currentSlider.CameraStartingRotation();
        }
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
    }
}