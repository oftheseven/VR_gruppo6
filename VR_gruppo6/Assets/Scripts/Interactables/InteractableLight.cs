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

    public bool IsOn => isOn;
    public float CurrentIntensity => currentIntensity;
    public Color CurrentColor => currentColor;
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

    public void SetColor(Color color)
    {
        currentColor = color;
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

    public string GetInteractionText()
    {
        return interactionText;
    }

    public UI_LightPanel GetLightPanel()
    {
        return lightPanel;
    }
}
