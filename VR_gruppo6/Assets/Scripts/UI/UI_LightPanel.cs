using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;

public class UI_LightPanel : MonoBehaviour
{
    [Header("Close timer settings")]
    [SerializeField] private float holdTimeToClose = 2f;
    [SerializeField] private float cooldownTime = 0.1f;

    [Header("UI references")]
    [SerializeField] private Slider intensitySlider;
    [SerializeField] private Button onOffButton;
    [SerializeField] private TextMeshProUGUI buttonText;

    [Header("Optional color control")]
    [SerializeField] private Slider redSlider;
    [SerializeField] private Slider greenSlider;
    [SerializeField] private Slider blueSlider;

    [Header("Hold to close UI")]
    [SerializeField] private GameObject holdIndicator;
    [SerializeField] private Image holdFillImage;

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

        UpdateUI();

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

        StartCoroutine(CooldownAndHide());
        this.gameObject.SetActive(false);
        canInteract = true;
        PlayerController.EnableMovement(true);

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

    public void ToggleLight()
    {
        if (currentLight == null) return;

        currentLight.SetLightState(!currentLight.IsOn);
        UpdateUI();
    }

    private void OnIntensityChanged(float value)
    {
        if (currentLight == null) return;

        currentLight.SetIntensity(value);
        UpdateUI();
    }

    private void OnColorChanged()
    {
        if (currentLight == null) return;

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