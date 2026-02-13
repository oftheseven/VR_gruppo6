using UnityEngine;

public class InteractableCamera : MonoBehaviour
{
    [Header("Interaction text")]
    [SerializeField] private string interactionText = "Premi E per gestire la camera";

    [Header("Camera panel reference")]
    [SerializeField] private UI_CameraPanel cameraPanel;

    [Header("Camera lenses")]
    [SerializeField] private GameObject[] cameraLenses;

    [Header("Interaction settings")]
    [SerializeField] private float interactionCooldown = 1f;

    private float lastInteractionTime = -999f;

    private Camera viewCamera; // camera associata all'InteractableCamera (viene usata dall'utente nell'UI_CameraPanel)
    public Camera ViewCamera => viewCamera;

    void Awake()
    {
        viewCamera = GetComponentInChildren<Camera>();

        if (viewCamera == null)
        {
            Debug.LogError("Nessuna camera trovata come figlia di " + this.name);
        }
        else
        {
            // Debug.Log("Camera trovata in " + this.name);
            viewCamera.gameObject.SetActive(false); // disattivo la camera all'inizio
        }
    }

    void Start()
    {
        foreach (GameObject lens in cameraLenses)
        {
            lens.SetActive(false);
        }
        cameraLenses[0].SetActive(true); // all'inizio attivo solo la prima lente
        cameraLenses[0].GetComponent<CameraLens>().ApplyToCamera(viewCamera); // applico la lente di default alla camera
    }

    public bool CanInteract()
    {
        if (Time.time - lastInteractionTime < interactionCooldown)
        {
            Debug.Log($"{this.name} in cooldown: {interactionCooldown - (Time.time - lastInteractionTime):F1}s rimasti");
            return false;
        }

        if (cameraPanel != null && !cameraPanel.CanInteract)
        {
            Debug.Log($"{this.name} camera panel in cooldown");
            return false;
        }

        if (cameraPanel != null && cameraPanel.IsOpen)
        {
            Debug.Log($"{this.name} camera panel già aperto");
            return false;
        }

        return true;
    }

    public void Interact()
    {
        // Debug.Log("Interazione con " + this.name);

        if (cameraPanel != null)
        {
            cameraPanel.OpenCamera();
            lastInteractionTime = Time.time;
        }

        if (TutorialManager.instance != null)
        {
            TutorialManager.instance.OnCameraCompleted();
        }
    }

    public string GetInteractionText()
    {
        if (!CanInteract())
        {
            return "";
        }
        
        return interactionText;
    }

    public UI_CameraPanel GetCameraPanel()
    {
        return cameraPanel;
    }

    public void ResetInteractionCooldown()
    {
        lastInteractionTime = Time.time;
        Debug.Log($"{this.name}: Cooldown resettato");
    }
}