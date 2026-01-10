using UnityEngine;

public class InteractableCamera : MonoBehaviour
{
    [Header("Interaction text")]
    [SerializeField] private string interactionText = "Premi E per gestire la camera";

    [Header("Camera panel reference")]
    [SerializeField] private UI_CameraPanel cameraPanel;

    [Header("Camera lenses")]
    [SerializeField] private GameObject[] cameraLenses;

    void Start()
    {
        foreach (GameObject lens in cameraLenses)
        {
            lens.SetActive(false);
        }
        cameraLenses[0].SetActive(true); // all'inizio attivo solo la prima lente
    }

    public void Interact()
    {
        Debug.Log("Interazione con " + this.name);

        if (cameraPanel != null)
        {
            cameraPanel.OpenCamera();
        }
    }

    public string getInteractionText()
    {
        return interactionText;
    }

    public UI_CameraPanel GetCameraPanel()
    {
        return cameraPanel;
    }
}
