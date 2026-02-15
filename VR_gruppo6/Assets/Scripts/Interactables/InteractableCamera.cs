using UnityEngine;

public class InteractableCamera : MonoBehaviour
{
    [Header("Interaction text")]
    [SerializeField] private string interactionText = "Premi E per gestire la camera";

    [Header("Camera panel reference")]
    [SerializeField] private UI_CameraPanel cameraPanel; // camera associata all'InteractableCamera (viene usata dall'utente nell'UI_CameraPanel)
    [SerializeField] private Camera viewCamera;

    [Header("Camera lenses")]
    [SerializeField] private GameObject[] cameraLenses;

    public Camera ViewCamera => viewCamera;

    void Start()
    {
        foreach (GameObject lens in cameraLenses)
        {
            lens.SetActive(false);
        }
        cameraLenses[0].SetActive(true); // all'inizio attivo solo la prima lente
        viewCamera.gameObject.SetActive(false); // disattivo la camera all'inizio
    }

    public void Interact()
    {
        // Debug.Log("Interazione con " + this.name);

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