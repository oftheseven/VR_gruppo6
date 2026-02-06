using UnityEngine;

public class InteractableLight : MonoBehaviour
{
    [Header("Interaction text")]
    [SerializeField] private string interactionText = "Premi E per gestire la luce";
    
    [Header("Panel reference")]
    [SerializeField] private UI_LightPanel lightPanel;

    [Header("Light settings")]
    [SerializeField] private Light[] controlledLights;
    
    private bool isOn = true;
    private float currentIntensity = 1f;
    private float maxIntensity = 8f;
    private Color currentColor = Color.white;
    private float currentTemperature = 6500f;
    private bool isTemperatureMode = false;

    public bool IsOn => isOn;
    public float CurrentIntensity => currentIntensity;
    public float MaxIntensity => maxIntensity;
    public Color CurrentColor => currentColor;
    public float CurrentTemperature => currentTemperature;
    public bool IsTemperatureMode => isTemperatureMode;
    public Light[] ControlledLights => controlledLights;

    void Start()
    {
        if (controlledLights == null || controlledLights.Length == 0)
        {
            controlledLights = new Light[] { GetComponent<Light>() };
        }

        if (controlledLights != null && controlledLights.Length > 0 && controlledLights[0] != null)
        {
            Light mainLight = controlledLights[0];
            
            isOn = mainLight.enabled;
            currentIntensity = mainLight.intensity;
            maxIntensity = mainLight.intensity;
            currentColor = mainLight.color;
            
            // Debug.Log($"💡 {name}: Intensità corrente={currentIntensity}, Max={maxIntensity}");
        }
        
        UpdateLights();
    }

    public void Interact()
    {
        if (lightPanel != null)
        {
            lightPanel.OpenPanel(this);
        }
        else
        {
            Debug.LogError("LightPanel non assegnato su " + this.name);
        }

        if (TutorialManager.instance != null)
        {
            TutorialManager.instance.OnLightCompleted();
        }

        if (GameManager.instance != null)
        {
            GameManager.instance.OnLightCompleted();
        }
    }

    public void SetLightState(bool state)
    {
        isOn = state;
        UpdateLights();
    }

    public void SetIntensity(float intensity)
    {
        currentIntensity = Mathf.Clamp(intensity, 0f, maxIntensity);
        UpdateLights();
    }

    public void SetTemperature(float temperature)
    {
        if (!isTemperatureMode) return;
        
        currentTemperature = Mathf.Clamp(temperature, 1000f, 12000f);
        currentColor = KelvinToColor(currentTemperature);
        UpdateLights();
    }

    public void SetColor(Color color)
    {
        if (isTemperatureMode) return;
        
        currentColor = color;
        UpdateLights();
    }

    public void SetColorMode(bool useTemperature)
    {
        isTemperatureMode = useTemperature;
        
        if (isTemperatureMode)
        {
            currentColor = Color.white;
            currentTemperature = 6500f;
            // Debug.Log("Modalità Temperatura attivata - Reset a 6500K");
        }
        // else
        // {
        //     Debug.Log("Modalità RGB attivata");
        // }
        
        UpdateLights();
    }

    private void UpdateLights()
    {
        if (controlledLights != null)
        {
            foreach (Light light in controlledLights)
            {
                if (light != null)
                {
                    light.enabled = isOn;
                    light.intensity = currentIntensity;
                    light.color = currentColor;
                }
            }
        }
    }

    private Color KelvinToColor(float kelvin)
    {
        float temp = kelvin / 100f;
        float r, g, b;

        // RED
        if (temp <= 66f)
        {
            r = 255f;
        }
        else
        {
            r = temp - 60f;
            r = 329.698727446f * Mathf.Pow(r, -0.1332047592f);
            r = Mathf.Clamp(r, 0f, 255f);
        }

        // GREEN
        if (temp <= 66f)
        {
            g = temp;
            g = 99.4708025861f * Mathf.Log(g) - 161.1195681661f;
            g = Mathf.Clamp(g, 0f, 255f);
        }
        else
        {
            g = temp - 60f;
            g = 288.1221695283f * Mathf.Pow(g, -0.0755148492f);
            g = Mathf.Clamp(g, 0f, 255f);
        }

        // BLUE
        if (temp >= 66f)
        {
            b = 255f;
        }
        else if (temp <= 19f)
        {
            b = 0f;
        }
        else
        {
            b = temp - 10f;
            b = 138.5177312231f * Mathf.Log(b) - 305.0447927307f;
            b = Mathf.Clamp(b, 0f, 255f);
        }

        return new Color(r / 255f, g / 255f, b / 255f);
    }

    public string GetInteractionText()
    {
        return interactionText;
    }

    public UI_LightPanel GetLightPanel()
    {
        return lightPanel;
    }
}
