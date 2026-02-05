using UnityEngine;

public class InteractableLight : MonoBehaviour
{
    [Header("Light settings")]
    [SerializeField] private Light controlledLight;
    
    private bool isOn = true;

    void Start()
    {
        if (controlledLight == null)
        {
            controlledLight = GetComponent<Light>();
        }

        isOn = false;
        UpdateLight();
    }

    private void UpdateLight()
    {
        if (controlledLight != null)
        {
            controlledLight.enabled = isOn;
        }
    }
}
