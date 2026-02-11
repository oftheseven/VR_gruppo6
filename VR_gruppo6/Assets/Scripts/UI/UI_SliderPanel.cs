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

    [Header("Recording UI")]
    [SerializeField] private Button recordButton;
    [SerializeField] private Button playButton;
    [SerializeField] private Button clearButton;
    [SerializeField] private TextMeshProUGUI recordingStatusText;
    [SerializeField] private Color recordingColor = Color.red;
    [SerializeField] private Color playingColor = Color.green;

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

        if (recordButton != null)
        {
            recordButton.onClick.AddListener(ToggleRecording);
        }

        if (playButton != null)
        {
            playButton.onClick.AddListener(TogglePlayback);
        }

        if (clearButton != null)
        {
            clearButton.onClick.AddListener(ClearRecording);
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
            UpdateRecordingUI();
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
            currentSlider.SliderCamera.gameObject.GetComponentInChildren<Camera>().enabled = true;
        }

        if (infoPanel != null)
        {
            infoPanel.OnDeviceOpened();
        }

        UpdateUI();
        UpdateRecordingButtonStates();
    }

    public void ClosePanel()
    {
        if (currentSlider != null)
        {
            if (currentSlider.IsRecording)
            {
                currentSlider.StopRecording();
            }
            if (currentSlider.IsPlaying)
            {
                currentSlider.StopPlayback();
            }
        }

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
            currentSlider.SliderCamera.gameObject.GetComponentInChildren<Camera>().enabled = false;
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

        if (controlHintText != null)
        {
            if (currentSlider.IsPlaying)
            {
                controlHintText.text = "Riproduzione in corso...";
            }
            else
            {
                controlHintText.text = "W/S: Muovi slider | ←→↑↓: Ruota camera";
            }
        }
    }

    private void UpdateRecordingUI()
    {
        if (currentSlider == null) return;

        if (recordingStatusText != null)
        {
            if (currentSlider.IsRecording)
            {
                recordingStatusText.text = "REGISTRAZIONE IN CORSO";
            }
            else if (currentSlider.IsPlaying)
            {
                recordingStatusText.text = "RIPRODUZIONE IN CORSO";
            }
            else if (currentSlider.CurrentRecording != null)
            {
                int keyframes = currentSlider.CurrentRecording.GetKeyframeCount();
                float duration = currentSlider.CurrentRecording.duration;
                recordingStatusText.text = $"Recording salvata: {duration:F1}s ({keyframes} frames)";
            }
            else
            {
                recordingStatusText.text = "Nessuna registrazione";
            }
        }

        UpdateRecordingButtonStates();
    }

    private void UpdateRecordingButtonStates()
    {
        if (currentSlider == null) return;

        bool isRecording = currentSlider.IsRecording;
        bool isPlaying = currentSlider.IsPlaying;
        bool hasRecording = currentSlider.CurrentRecording != null && currentSlider.CurrentRecording.GetKeyframeCount() > 0;

        if (recordButton != null)
        {
            var buttonText = recordButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = isRecording ? "Stop" : "Rec";
            }
        }

        if (playButton != null)
        {
            playButton.interactable = hasRecording && !isRecording;
            
            var buttonText = playButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = isPlaying ? "Stop" : "Play";
            }
        }

        if (clearButton != null)
        {
            clearButton.interactable = hasRecording && !isRecording && !isPlaying;
        }
    }

    private void ToggleRecording()
    {
        if (currentSlider == null) return;

        if (currentSlider.IsRecording)
        {
            currentSlider.StopRecording();
        }
        else
        {
            currentSlider.StartRecording();
        }
    }

    private void TogglePlayback()
    {
        if (currentSlider == null) return;

        if (currentSlider.IsPlaying)
        {
            currentSlider.StopPlayback();
        }
        else
        {
            currentSlider.StartPlayback();
        }
    }

    private void ClearRecording()
    {
        if (currentSlider == null) return;

        currentSlider.ClearRecording();
    }

    private void HandleCameraRotation()
    {
        if (currentSlider == null || currentSlider.SliderCamera == null) return;
        if (infoPanel != null && infoPanel.IsOpen) return;
        if (currentSlider.IsPlaying) return;

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
        if (currentSlider.IsPlaying) return;

        float movement = 0f;
        
        if (Keyboard.current.wKey.isPressed)
        {
            movement = keyboardMoveSpeed * Time.deltaTime;
        }
        else if (Keyboard.current.sKey.isPressed)
        {
            movement = -keyboardMoveSpeed * Time.deltaTime;
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