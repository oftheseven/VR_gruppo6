using UnityEngine;

public class InteractableCamera : MonoBehaviour
{
    [Header("Interaction text")]
    [SerializeField] private string interactionText = "Premi E per gestire la camera";

    [Header("Camera panel reference")]
    [SerializeField] private UI_CameraPanel cameraPanel;

    [Header("Camera lenses")]
    [SerializeField] private GameObject[] cameraLenses;

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

    public void Interact()
    {
        Debug.Log("Interazione con " + this.name);

        if (cameraPanel != null)
        {
            cameraPanel.OpenCamera();
        }

        if (TutorialManager.instance != null)
        {
            TutorialManager.instance.OnCameraCompleted();
        }
    }

    public string GetInteractionText()
    {
        return interactionText;
    }

    public UI_CameraPanel GetCameraPanel()
    {
        return cameraPanel;
    }
}