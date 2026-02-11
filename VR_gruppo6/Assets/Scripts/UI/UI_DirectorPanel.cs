using UnityEngine;
using TMPro;

public class UI_DirectorPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject panelContainer;
    [SerializeField] private TextMeshProUGUI cameraNameText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI controlsText;
    [SerializeField] private TextMeshProUGUI sceneStatusText;

    [Header("Visual Settings")]
    [SerializeField] private Color camera1Color = Color.cyan;
    [SerializeField] private Color camera2Color = Color.yellow;

    private float sceneStartTime;
    private float sceneDuration;
    private bool isActive = false;

    void Start()
    {
        if (panelContainer != null)
        {
            panelContainer.SetActive(false);
        }
    }

    void Update()
    {
        if (isActive)
        {
            UpdateTimer();
        }
    }

    public void ShowPanel(float duration)
    {
        sceneDuration = duration;
        sceneStartTime = Time.time;
        isActive = true;

        if (panelContainer != null)
        {
            panelContainer.SetActive(true);
        }

        if (controlsText != null)
        {
            controlsText.text = "1: Camera Slider | 2: Camera Treppiede";
        }

        if (sceneStatusText != null)
        {
            sceneStatusText.text = "REGISTRAZIONE IN CORSO";
        }

        UpdateCameraDisplay(1);
    }

    public void HidePanel()
    {
        isActive = false;

        if (panelContainer != null)
        {
            panelContainer.SetActive(false);
        }
    }

    public void UpdateCameraDisplay(int cameraIndex)
    {
        if (cameraNameText != null)
        {
            string cameraName = cameraIndex == 1 ? "CAMERA 1: SLIDER" : "CAMERA 2: TREPPIEDE";
            cameraNameText.text = cameraName;
            
            cameraNameText.color = cameraIndex == 1 ? camera1Color : camera2Color;
        }
    }

    private void UpdateTimer()
    {
        float elapsed = Time.time - sceneStartTime;
        float remaining = Mathf.Max(0, sceneDuration - elapsed);

        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(remaining / 60f);
            int seconds = Mathf.FloorToInt(remaining % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }
    }
}