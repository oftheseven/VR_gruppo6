using UnityEngine;

public class InteractableCiak : MonoBehaviour
{
    [Header("Interaction text")]
    [SerializeField] private string interactionText = "Premi E per prendere il ciak";

    public void Interact()
    {
        // Debug.Log("Interazione con " + this.gameObject.name);
    }

    public string getInteractionText()
    {
        return interactionText;
    }
}
