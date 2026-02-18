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
    // [SerializeField] private TextMeshProUGUI positionText;
    // [SerializeField] private TextMeshProUGUI controlHintText; // hint per l'utente per capire quali controlli usare
    // [SerializeField] private Button resetButton;

    [Header("Recording UI")]
    [SerializeField] private Button startRecordingButton;
    [SerializeField] private Button stopRecordingButton;
    [SerializeField] private Button playButton;
    [SerializeField] private Button clearButton;
    // [SerializeField] private TextMeshProUGUI recordingStatusText;
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

        if (startRecordingButton != null)
        {
            startRecordingButton.onClick.AddListener(StartRecording);
        }

        if (stopRecordingButton != null)
        {
            stopRecordingButton.onClick.AddListener(StopRecording);
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
            UpdateRecordingButtonStates();
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
        currentSlider.gameObject.GetComponentInChildren<AudioListener>().enabled = true;
        this.gameObject.SetActive(true);

        isOpen = true;

        PlayerController.EnableMovement(false);
        PlayerController.ShowCursor();
        PlayerController.instance.gameObject.GetComponentInChildren<AudioListener>().enabled = false;
        PlayerController.SetBasePanelActive(false);

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

        currentSlider.gameObject.GetComponentInChildren<AudioListener>().enabled = false;

        currentSlider = null;

        PlayerController.HideCursor();
        PlayerController.EnableMovement(true);
        PlayerController.instance.gameObject.GetComponentInChildren<AudioListener>().enabled = true;
        

        StartCoroutine(CooldownAndHide());
        canInteract = true;
        PlayerController.SetBasePanelActive(true);
    }

    private void UpdateRecordingButtonStates()
    {
        if (currentSlider == null) return;

        bool isRecording = currentSlider.IsRecording;
        bool isPlaying = currentSlider.IsPlaying;
        bool hasRecording = currentSlider.CurrentRecording != null && currentSlider.CurrentRecording.GetKeyframeCount() > 0;

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

    private void StartRecording()
    {
        if (currentSlider == null) return;
        currentSlider.StartRecording();
        UpdateRecordingButtonStates();
    }

    private void StopRecording()
    {
        if (currentSlider == null) return;
        currentSlider.StopRecording();
        UpdateRecordingButtonStates();
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