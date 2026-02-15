using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

public class UI_ArmPanel : MonoBehaviour
{
    [Header("Arm reference")]
    [SerializeField] private InteractableArm interactableArm;
    
    [Header("UI elements")]
    [SerializeField] private TextMeshProUGUI waypointCountText;
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private TextMeshProUGUI playbackProgressText;
    [SerializeField] private Button startRecordingButton;
    [SerializeField] private Button stopRecordingButton;
    [SerializeField] private Button clearButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button playbackButton;

    [Header("Control settings")]
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private Camera armCamera;
    [SerializeField] private Camera armCameraView;
    [SerializeField] private ArmCameraOrbit armCameraOrbit;

    [Header("Hold to close UI")]
    [SerializeField] private float holdTimeToClose = 2f;
    [SerializeField] private float cooldownTime = 1f;
    [SerializeField] private GameObject holdIndicator; // container del cerchio
    [SerializeField] private Image holdFillImage; // image con fill radial

    private bool isOpen = false;
    public bool IsOpen => isOpen;
    
    private bool canInteract = true;
    public bool CanInteract => canInteract;

    private float holdTimer = 0f;

    
    void Awake()
    {
        this.gameObject.SetActive(false);
        
        if (startRecordingButton != null)
            startRecordingButton.onClick.AddListener(OnStartRecording);
        
        if (stopRecordingButton != null)
            stopRecordingButton.onClick.AddListener(OnStopRecording);
        
        if (clearButton != null)
            clearButton.onClick.AddListener(OnClear);
        
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseArmPanel);

        if (playbackButton != null)
            playbackButton.onClick.AddListener(OnPlayback);

        if (armCameraOrbit == null && armCamera != null)
        {
            armCameraOrbit = armCamera.GetComponent<ArmCameraOrbit>();
            if (armCameraOrbit != null)
            {
                Debug.Log("ArmCameraOrbit auto-detected");
            }
        }
    }

    void Start()
    {
        canInteract = true;

        if (holdIndicator != null)
        {
            holdIndicator.SetActive(false);
        }

        if (holdFillImage != null)
        {
            holdFillImage.fillAmount = 0;
        }
    }

    void Update()
    {
        if (!isOpen) return;
        
        UpdateUI();

        if (ArmWaypointPlayback.instance != null && ArmWaypointPlayback.instance.IsPlayingBack)
        {
            UpdatePlaybackProgress();
        }

        if (interactableArm.IsRecording)
        {
            HandleArmControl();
        }
        
        if (interactableArm.IsRecording && Keyboard.current.enterKey.wasPressedThisFrame)
        {
            interactableArm.AddWaypoint();
        }
        
        if (isOpen)
        {
            HandleArmClose();
        }
    }

    private void HandleArmControl()
    {
        if (Keyboard.current == null) return;

        float baseRotation = 0f;
        float jointRotation = 0f;

        if (Keyboard.current.leftArrowKey.isPressed)
        {
            baseRotation -= rotationSpeed * Time.deltaTime;
        }
        if (Keyboard.current.rightArrowKey.isPressed)
        {
            baseRotation += rotationSpeed * Time.deltaTime;
        }

        if (Keyboard.current.upArrowKey.isPressed)
        {
            jointRotation -= rotationSpeed * Time.deltaTime;
        }
        if (Keyboard.current.downArrowKey.isPressed)
        {
            jointRotation += rotationSpeed * Time.deltaTime;
        }
        
        if (Mathf.Abs(baseRotation) > 0.01f)
        {
            interactableArm.RotateBase(baseRotation);
        }

        if (Mathf.Abs(jointRotation) > 0.01f)
        {
            interactableArm.RotateJoint(jointRotation);
        }
    }

    public void OpenArmPanel()
    {
        this.gameObject.SetActive(true);
        isOpen = true;

        PlayerController.EnableMovement(false);
        PlayerController.ShowCursor();
        PlayerController.instance.playerCamera.gameObject.SetActive(false);

        if (armCamera != null)
        {
            armCamera.enabled = true;
        }

        if (armCameraView != null)
        {
            armCameraView.enabled = false;
        }
        
        if (armCameraOrbit != null)
        {
            armCameraOrbit.EnableOrbit();
        }
        else
        {
            Debug.LogWarning("ArmCameraOrbit non trovato su armCamera!");
        }

        if (interactableArm.VisualFeedback != null)
        {
            interactableArm.VisualFeedback.EnableVisuals();
        }

        UpdateUI();
    }

    public void CloseArmPanel()
    {
        if (interactableArm.IsRecording)
        {
            interactableArm.StopRecording();
        }

        if (ArmWaypointPlayback.instance != null && ArmWaypointPlayback.instance.IsPlayingBack)
        {
            ArmWaypointPlayback.instance.StopPlayback();
        }

        isOpen = false;
        holdTimer = 0f;
        
        if (armCamera != null)
        {
            armCamera.enabled = false;
            Debug.Log("armCamera (supporto) disabilitata");
        }

        if (armCameraOrbit != null)
        {
            armCameraOrbit.DisableOrbit();
        }

        if (interactableArm.VisualFeedback != null)
        {
            interactableArm.VisualFeedback.DisableVisuals();
        }

        if (holdIndicator != null)
        {
            holdIndicator.SetActive(false);
        }

        StartCoroutine(CooldownAndHide());
        this.gameObject.SetActive(false);
        canInteract = true;
        PlayerController.EnableMovement(true);
        PlayerController.instance.playerCamera.gameObject.SetActive(true);
        PlayerController.HideCursor();
    }
    
    private void UpdateUI()
    {
        if (waypointCountText != null)
        {
            waypointCountText.text = $"Waypoint: {interactableArm.WaypointCount}";
        }
        
        if (instructionText != null)
        {
            if (ArmWaypointPlayback.instance != null && ArmWaypointPlayback.instance.IsPlayingBack)
            {
                instructionText.text = "PLAYBACK IN CORSO...";
            }
            else if (interactableArm.IsRecording)
            {
                instructionText.text = "← → per ruotare\nINVIO = salva waypoint\n'Fine' = termina";
            }
            else if (interactableArm.WaypointCount > 0)
            {
                float duration = interactableArm.GetPlaybackDuration();
                instructionText.text = $"Recording completo!\nDurata: {duration:F1}s\nPremi 'Play' per testare";
            }
            else
            {
                instructionText.text = "Premi 'Inizia' per registrare";
            }
        }
        
        bool isPlayingBack = ArmWaypointPlayback.instance != null && ArmWaypointPlayback.instance.IsPlayingBack;
        
        if (startRecordingButton != null)
            startRecordingButton.interactable = !interactableArm.IsRecording && !isPlayingBack;
        
        if (stopRecordingButton != null)
            stopRecordingButton.interactable = interactableArm.IsRecording;
        
        if (playbackButton != null)
            playbackButton.interactable = interactableArm.WaypointCount >= 2 && !interactableArm.IsRecording && !isPlayingBack;
        
        if (clearButton != null)
            clearButton.interactable = interactableArm.WaypointCount > 0 && !interactableArm.IsRecording && !isPlayingBack;
    }

    private void UpdatePlaybackProgress()
    {
        if (playbackProgressText != null && ArmWaypointPlayback.instance != null)
        {
            float progress = ArmWaypointPlayback.instance.PlaybackProgress * 100f;
            playbackProgressText.text = $"Progresso: {progress:F0}%";
            playbackProgressText.gameObject.SetActive(true);
        }
    }
    
    private void OnStartRecording()
    {
        interactableArm.StartRecording();
        Debug.Log("Recording iniziato");
    }
    
    private void OnStopRecording()
    {
        interactableArm.StopRecording();
        Debug.Log("Recording fermato");

        if (interactableArm.WaypointCount >= 2)
        {
            if (TutorialManager.instance != null)
            {
                TutorialManager.instance.OnArmCompleted();
                Debug.Log("Task ARM completato per tutorial!");
            }
        }
    }
    
    private void OnClear()
    {
        interactableArm.ClearWaypoints();
        Debug.Log("Waypoint cancellati");
    }

    private void OnPlayback()
    {
        Debug.Log("Bottone PLAYBACK cliccato!");
        
        if (ArmWaypointPlayback.instance != null)
        {
            ArmWaypointPlayback.instance.StartPlayback(interactableArm);
        }
        else
        {
            Debug.LogError("ArmWaypointPlayback.instance è NULL!");
        }
        
        if (playbackProgressText != null)
        {
            playbackProgressText.gameObject.SetActive(false);
        }
    }
    
    public UI_ArmPanel GetArmPanel()
    {
        return this;
    }

    public void HandleArmClose()
    {
        if (Keyboard.current.eKey.isPressed)
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
                CloseArmPanel();
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
        yield return new WaitForSeconds(cooldownTime);
        canInteract = true;
    }
}