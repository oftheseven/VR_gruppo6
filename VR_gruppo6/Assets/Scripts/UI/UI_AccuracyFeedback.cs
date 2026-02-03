using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_AccuracyFeedback : MonoBehaviour
{
    // singleton
    private static UI_AccuracyFeedback _instance;
    public static UI_AccuracyFeedback instance => _instance;

    [Header("UI References")]
    [SerializeField] private GameObject feedbackPanel;
    [SerializeField] private TextMeshProUGUI waypointsText;
    [SerializeField] private TextMeshProUGUI accuracyText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button continueButton;

    [Header("Replay UI")]
    [SerializeField] private Button replayButton;

    private bool isOpen = false;
    public bool IsOpen => isOpen;

    private bool isReplaying = false;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (feedbackPanel != null)
        {
            feedbackPanel.SetActive(false);
        }
    }

    public void ShowResults(AccuracyResults results, float timeTaken)
    {
        if (feedbackPanel == null)
        {
            return;
        }
        
        PlayerController.EnableMovement(false);
        feedbackPanel.SetActive(true);
        isOpen = true;
        
        if (waypointsText != null)
        {
            waypointsText.text = $"Waypoint: {results.waypointsReached}/{results.totalWaypoints}";
        }
        
        if (accuracyText != null)
        {
            accuracyText.text = $"Accuratezza: {results.finalScore:F0}%";
        }

        if (timeText != null)
        {
            timeText.text = $"Tempo: {timeTaken:F1}s";
        }

        if (replayButton != null)
        {
            bool hasRecording = ArmMovementRecorder.instance != null 
                && ArmMovementRecorder.instance.SnapshotCount > 0;
            replayButton.interactable = hasRecording;
        }
    }

    public void OnReplayClicked()
    {
        if (ArmMovementRecorder.instance == null || ArmMovementPlayback.instance == null)
        {
            Debug.LogError("❌ Recorder o Playback non disponibili!");
            return;
        }
        
        if (ArmMovementRecorder.instance.SnapshotCount == 0)
        {
            Debug.LogWarning("⚠️ Nessun movimento registrato!");
            return;
        }
        
        Debug.Log("▶️ Avvio replay movimento...");
        
        if (retryButton != null) retryButton.interactable = false;
        if (continueButton != null) continueButton.interactable = false;
        if (replayButton != null) replayButton.interactable = false;

        isReplaying = true;
        
        ArmMovementPlayback.instance.StartPlayback(ArmMovementRecorder.instance.RecordedSnapshots);
    }

    public void OnPlaybackStarted()
    {
        Debug.Log("🎬 Playback avviato da UI");
    }

    public void OnPlaybackFinished()
    {
        Debug.Log("✅ Playback completato");
        
        isReplaying = false;
        
        if (retryButton != null) retryButton.interactable = true;
        if (continueButton != null) continueButton.interactable = true;
        if (replayButton != null) replayButton.interactable = true;
    }

    public void OnRetryClicked()
    {
        Close();
        
        if (InteractableArm.instance != null)
        {
            InteractableArm.instance.GetArmPanel().OpenArm();
        }
    }

    public void OnContinueClicked()
    {
        Close();
        PlayerController.EnableMovement(true);
    }

    public void Close()
    {
        if (isReplaying && ArmMovementPlayback.instance != null)
        {
            ArmMovementPlayback.instance.StopPlayback();
        }

        if (feedbackPanel != null)
        {
            feedbackPanel.SetActive(false);
        }
        isOpen = false;
        isReplaying = false;
    }
}