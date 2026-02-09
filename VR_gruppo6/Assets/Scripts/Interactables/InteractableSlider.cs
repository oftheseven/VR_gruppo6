using UnityEngine;

public class InteractableSlider : MonoBehaviour
{
    [Header("Slider references")]
    [SerializeField] private Transform sliderCart;
    [SerializeField] private Camera sliderCamera;
    [SerializeField] private Transform railStart;
    [SerializeField] private Transform railEnd;

    [Header("Slider settings")]
    [SerializeField] private string sliderName = "Slider tutorial";
    [SerializeField] private float currentPosition = 0.5f;

    [Header("Interaction settings")]
    [SerializeField] private string interactionText = "Premi E per interagire con lo slider";
    [SerializeField] private UI_SliderPanel sliderPanel;

    [Header("Camera settings")]
    [SerializeField] private bool lockCameraRotation = false;
    [SerializeField] private Transform cameraTarget;

    private bool canInteract = true;
    public bool CanInteract => canInteract;
    
    public Camera SliderCamera => sliderCamera;
    public float CurrentPosition => currentPosition;
    public string SliderName => sliderName;

    void Start()
    {
        UpdateSliderPosition(currentPosition);

        if (sliderCamera != null)
        {
            sliderCamera.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (sliderPanel != null && sliderPanel.IsOpen)
        {
            UpdateSliderPosition(currentPosition);
            
            if (cameraTarget != null && sliderCamera != null && !lockCameraRotation)
            {
                sliderCamera.transform.LookAt(cameraTarget);
            }
        }
    }

    public void Interact()
    {
        if (!canInteract)
        {
            return;
        }

        // Debug.Log($"Interazione con {sliderName}");

        if (sliderPanel != null)
        {
            sliderPanel.OpenPanel(this);
        }
        else
        {
            Debug.LogError("UI_SliderPanel non assegnato!");
        }

        if (TutorialManager.instance != null)
        {
            TutorialManager.instance.OnSliderCompleted();
        }
    }

    public void SetPosition(float normalizedPosition)
    {
        currentPosition = Mathf.Clamp01(normalizedPosition);
        UpdateSliderPosition(currentPosition);
    }

    public void MoveSlider(float delta)
    {
        currentPosition = Mathf.Clamp01(currentPosition + delta);
        UpdateSliderPosition(currentPosition);
    }

    private void UpdateSliderPosition(float t)
    {
        if (sliderCart == null || railStart == null || railEnd == null)
        {
            Debug.LogWarning("Slider references non configurate!");
            return;
        }

        Vector3 newPosition = Vector3.Lerp(railStart.position, railEnd.position, t);
        sliderCart.position = newPosition;
        
        // Debug.Log($"Slider posizione: {t:F2} ({newPosition})");
    }

    public float GetDistanceInMeters()
    {
        if (railStart == null || railEnd == null) return 0f;
        
        float totalLength = Vector3.Distance(railStart.position, railEnd.position);
        return totalLength * currentPosition;
    }

    public float GetTotalLength()
    {
        if (railStart == null || railEnd == null) return 0f;
        return Vector3.Distance(railStart.position, railEnd.position);
    }
    
    public string GetInteractionText()
    {
        if (!canInteract)
        {
            return $"{sliderName} (Non disponibile)";
        }
        return interactionText;
    }

    public UI_SliderPanel GetSliderPanel()
    {
        return sliderPanel;
    }

    public string GetSliderName()
    {
        return sliderName;
    }
}