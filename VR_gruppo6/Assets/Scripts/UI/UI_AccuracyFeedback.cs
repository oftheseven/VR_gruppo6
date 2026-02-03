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

    private bool isOpen = false;
    public bool IsOpen => isOpen;

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
        if (feedbackPanel != null)
        {
            feedbackPanel.SetActive(false);
        }
        isOpen = false;
    }
}