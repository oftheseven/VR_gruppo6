using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;

public class UI_LightPanel : MonoBehaviour
{
    [Header("Info panel reference")]
    [SerializeField] private UI_InfoPanel infoPanel;

    [Header("UI references")]
    [SerializeField] private Slider intensitySlider;
    [SerializeField] private Button onOffButton;
    [SerializeField] private TextMeshProUGUI buttonText;

    [Header("RGB light color slider")]
    [SerializeField] private GameObject rgbPanel;
    [SerializeField] private Slider redSlider;
    [SerializeField] private Slider greenSlider;
    [SerializeField] private Slider blueSlider;

    [Header("Light temperature slider")]
    [SerializeField] private GameObject temperaturePanel;
    [SerializeField] private Slider temperatureSlider;
    [SerializeField] private TextMeshProUGUI temperatureText;

    [Header("Mode switch")]
    [SerializeField] private Button toggleModeButton;
    [SerializeField] private TextMeshProUGUI modeButtonText;

    [Header("Hold to close UI")]
    [SerializeField] private GameObject holdIndicator;
    [SerializeField] private Image holdFillImage;

    [Header("Close timer settings")]
    [SerializeField] private float holdTimeToClose = 2f;
    [SerializeField] private float cooldownTime = 0.1f;

    private InteractableLight currentLight;
    private bool isOpen = false;
    public bool IsOpen => isOpen;
    private bool canInteract = true;
    public bool CanInteract => canInteract;
    private float holdTimer = 0f;

    void Start()
    {
        this.gameObject.SetActive(false);
        canInteract = true;

        if (holdIndicator != null)
        {
            holdIndicator.SetActive(false);
        }

        if (holdFillImage != null)
        {
            holdFillImage.fillAmount = 0;
        }

        if (intensitySlider != null)
        {
            intensitySlider.onValueChanged.AddListener(OnIntensityChanged);
        }
        
        // slider RGB
        if (redSlider != null)
        {
            redSlider.onValueChanged.AddListener(_ => OnColorChanged());
        }
        if (greenSlider != null)
        {
            greenSlider.onValueChanged.AddListener(_ => OnColorChanged());
        }
        if (blueSlider != null)
        {
            blueSlider.onValueChanged.AddListener(_ => OnColorChanged());
        }

        // slider temperatura
        if (temperatureSlider != null)
        {
            temperatureSlider.onValueChanged.AddListener(OnTemperatureChanged);
        }

        // bottone per la modalità
        if (toggleModeButton != null)
        {
            toggleModeButton.onClick.AddListener(ToggleColorMode);
        }
    }

    void Update()
    {
        if (isOpen)
        {
            HandlePanelClose();
        }
    }

    public void OpenPanel(InteractableLight light)
    {
        if (light == null)
        {
            Debug.LogError("InteractableLight è null!");
            return;
        }

        currentLight = light;
        this.gameObject.SetActive(true);
        isOpen = true;
        PlayerController.EnableMovement(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (infoPanel != null)
        {
            infoPanel.OnDeviceOpened();
        }

        if (intensitySlider != null)
        {
            intensitySlider.maxValue = currentLight.MaxIntensity;
        }

        if (temperatureSlider != null)
        {
            temperatureSlider.minValue = 1000f;
            temperatureSlider.maxValue = 8000f;
        }

        UpdateUI();
        UpdateColorModeUI();

        // Debug.Log($"Pannello luce aperto per: {currentLight.name}");
    }

    public void ClosePanel()
    {
        isOpen = false;
        holdTimer = 0f;
        currentLight = null;

        if (holdIndicator != null)
        {
            holdIndicator.SetActive(false);
        }

        if (infoPanel != null)
        {
            infoPanel.OnDeviceClosed();
        }

        StartCoroutine(CooldownAndHide());
        this.gameObject.SetActive(false);
        canInteract = true;
        PlayerController.EnableMovement(true);
    
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Debug.Log("Pannello luce chiuso");
    }

    private void UpdateUI()
    {
        if (currentLight == null) return;

        if (buttonText != null)
        {
            buttonText.text = currentLight.IsOn ? "Spegni" : "Accendi";
        }

        if (intensitySlider != null)
        {
            intensitySlider.SetValueWithoutNotify(currentLight.CurrentIntensity);
        }

        if (currentLight.IsTemperatureMode)
        {
            // temperature mode
            if (temperatureSlider != null)
            {
                temperatureSlider.SetValueWithoutNotify(currentLight.CurrentTemperature);
            }
            if (temperatureText != null)
            {
                temperatureText.text = $"{currentLight.CurrentTemperature:F0}K";
            }
        }
        else
        {
            // RGB mode
            if (redSlider != null)
            {
                redSlider.SetValueWithoutNotify(currentLight.CurrentColor.r);
            }
            if (greenSlider != null)
            {
                greenSlider.SetValueWithoutNotify(currentLight.CurrentColor.g);
            }
            if (blueSlider != null)
            {
                blueSlider.SetValueWithoutNotify(currentLight.CurrentColor.b);
            }
        }
    }

    public void ToggleLight()
    {
        if (currentLight == null) return;

        currentLight.SetLightState(!currentLight.IsOn);
        UpdateUI();
    }

    private void ToggleColorMode()
    {
        if (currentLight == null) return;

        bool newMode = !currentLight.IsTemperatureMode;
        currentLight.SetColorMode(newMode);
        
        UpdateUI();
        UpdateColorModeUI();
    }

    private void UpdateColorModeUI()
    {
        if (currentLight == null) return;

        bool isTempMode = currentLight.IsTemperatureMode;

        if (rgbPanel != null)
        {
            rgbPanel.SetActive(!isTempMode);
        }

        if (temperaturePanel != null)
        {
            temperaturePanel.SetActive(isTempMode);
        }

        if (modeButtonText != null)
        {
            modeButtonText.text = isTempMode ? "Usa RGB" : "Usa Temperatura";
        }
    }

    private void OnIntensityChanged(float value)
    {
        if (currentLight == null) return;

        currentLight.SetIntensity(value);
        UpdateUI();
    }

    private void OnTemperatureChanged(float value)
    {
        if (currentLight == null || !currentLight.IsTemperatureMode) return;

        currentLight.SetTemperature(value);
        
        if (temperatureText != null)
        {
            temperatureText.text = $"{value:F0}K";
        }
    }

    private void OnColorChanged()
    {
        if (currentLight == null || currentLight.IsTemperatureMode) return;

        if (redSlider != null && greenSlider != null && blueSlider != null)
        {
            Color newColor = new Color(
                redSlider.value,
                greenSlider.value,
                blueSlider.value
            );
            currentLight.SetColor(newColor);
        }
    }

    public void HandlePanelClose()
    {
        if (Keyboard.current.eKey.isPressed && !infoPanel.IsOpen)
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
        yield return new WaitForSeconds(cooldownTime);
        canInteract = true;
    }
}