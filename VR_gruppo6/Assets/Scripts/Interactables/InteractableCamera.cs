using UnityEngine;

public class InteractableCamera : MonoBehaviour
{
    [Header("Interaction text")]
    [SerializeField] private string interactionText = "Premi E per eseguire l'interazione";

    [Header("Camera panel reference")]
    [SerializeField] private UI_CameraPanel cameraPanel;

    public void Interact()
    {
        Debug.Log("Interazione con " + this.gameObject.name);

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
