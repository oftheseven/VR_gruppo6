using UnityEngine;
using TMPro;
using System.Collections.Generic;

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
    [SerializeField] private Color camera3Color = Color.magenta;

    private float sceneStartTime;
    private float sceneDuration;
    private bool isActive = false;
    private List<int> availableCameras = new List<int>();

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

    public void ShowPanel(float duration, List<int> cameras)
    {
        sceneDuration = duration;
        sceneStartTime = Time.time;
        isActive = true;
        availableCameras = new List<int>(cameras);

        if (panelContainer != null)
        {
            panelContainer.SetActive(true);
        }

        if (controlsText != null)
        {
            controlsText.text = GetControlsText();
        }

        if (sceneStatusText != null)
        {
            sceneStatusText.text = "REGISTRAZIONE IN CORSO";
        }

        UpdateCameraDisplay(cameras[0]);
    }

    private string GetControlsText()
    {
        List<string> controls = new List<string>();

        if (availableCameras.Contains(1))
        {
            controls.Add("1: Camera Slider");
        }

        if (availableCameras.Contains(2))
        {
            controls.Add("2: Camera Treppiede");
        }

        if (availableCameras.Contains(3))
        {
            controls.Add("3: Braccio Meccanico");
        }

        return string.Join(" | ", controls);
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
            string cameraName = "";
            Color color = Color.white;

            switch (cameraIndex)
            {
                case 1:
                    cameraName = "CAMERA 1: SLIDER";
                    color = camera1Color;
                    break;
                case 2:
                    cameraName = "CAMERA 2: TREPPIEDE";
                    color = camera2Color;
                    break;
                case 3:
                    cameraName = "CAMERA 3: BRACCIO MECCANICO";
                    color = camera3Color;
                    break;
            }

            cameraNameText.text = cameraName;
            cameraNameText.color = color;
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