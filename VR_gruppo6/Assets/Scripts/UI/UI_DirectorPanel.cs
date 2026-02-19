using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class UI_DirectorPanel : MonoBehaviour
{
    // singleton
    private static UI_DirectorPanel _instance;
    public static UI_DirectorPanel instance => _instance;

    [Header("UI References")]
    [SerializeField] private GameObject panelContainer;
    [SerializeField] private Texture2D camera1Image;
    [SerializeField] private Texture2D camera2Image;
    [SerializeField] private TextMeshProUGUI timerText;

    private float sceneStartTime;
    private float sceneDuration;
    private bool isActive = false;
    private List<int> availableCameras = new List<int>();

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

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
    }

    public void HidePanel()
    {
        isActive = false;

        if (panelContainer != null)
        {
            panelContainer.SetActive(false);
        }
    }

    public void SetCameraPanel(int panel)
    {
        switch(panel)
        {
            case 1:
                if (camera1Image != null)
                {
                    panelContainer.GetComponent<RawImage>().texture = camera1Image;
                }
                break;
            case 2:
                if (camera2Image != null)
                {
                    panelContainer.GetComponent<RawImage>().texture = camera2Image;
                }
                break;
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