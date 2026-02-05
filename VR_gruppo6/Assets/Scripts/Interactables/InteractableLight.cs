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
    private Color currentColor = Color.white;
    private float currentTemperature = 6500f;

    public bool IsOn => isOn;
    public float CurrentIntensity => currentIntensity;
    public Color CurrentColor => currentColor;
    public float CurrentTemperature => currentTemperature;
    public Light[] ControlledLights => controlledLights;

    void Start()
    {
        if (controlledLights == null || controlledLights.Length == 0)
        {
            controlledLights = new Light[] { GetComponent<Light>() };
        }

        if (controlledLights != null && controlledLights.Length > 0 && controlledLights[0] != null)
        {
            isOn = controlledLights[0].enabled;
            currentIntensity = controlledLights[0].intensity;
            currentColor = controlledLights[0].color;

            currentTemperature = EstimateTemperatureFromColor(currentColor);
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
    }

    public void SetLightState(bool state)
    {
        isOn = state;
        UpdateLights();
    }

    public void SetIntensity(float intensity)
    {
        currentIntensity = Mathf.Clamp(intensity, 0f, 8f);
        UpdateLights();
    }

    public void SetTemperature(float temperature)
    {
        currentTemperature = Mathf.Clamp(temperature, 1000f, 20000f);
        currentColor = KelvinToColor(currentTemperature);
        UpdateLights();
    }

    public void SetColor(Color color)
    {
        currentColor = color;
        currentTemperature = EstimateTemperatureFromColor(color);
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

        // Calcolo RED
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

        // Calcolo GREEN
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

        // Calcolo BLUE
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

    private float EstimateTemperatureFromColor(Color color)
    {
        // Approssimazione semplice basata sul rapporto R/B
        float ratio = color.b > 0.01f ? color.r / color.b : 1f;
        
        if (ratio > 1.5f)
        {
            // Toni caldi (arancione/giallo)
            return Mathf.Lerp(1000f, 4000f, 1f - (ratio - 1.5f) / 2f);
        }
        else if (ratio < 0.7f)
        {
            // Toni freddi (blu)
            return Mathf.Lerp(6500f, 10000f, (0.7f - ratio) / 0.7f);
        }
        else
        {
            // Toni neutri
            return 6500f;
        }
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
