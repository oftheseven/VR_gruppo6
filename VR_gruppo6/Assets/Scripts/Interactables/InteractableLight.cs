using UnityEngine;

public class InteractableLight : MonoBehaviour
{
    [Header("Light settings")]
    [SerializeField] private Light[] controlledLights;
    
    private bool isOn = true;

    void Start()
    {
        if (controlledLights == null || controlledLights.Length == 0)
        {
            controlledLights = new Light[] { GetComponent<Light>() };
        }

        isOn = false;
        UpdateLight();
    }

    public void Interact()
    {
        isOn = !isOn;
        UpdateLight();
    }

    private void UpdateLight()
    {
        if (controlledLights != null)
        {
            foreach (Light light in controlledLights)
            {
                if (light != null)
                {
                    light.enabled = isOn;
                }
            }
        }
    }

    public string GetInteractionText()
    {
        return isOn ? "Spegni la luce (E)" : "Accendi la luce (E)";
    }
}
