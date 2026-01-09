using UnityEngine;

public class InteractableCart : MonoBehaviour
{
    [Header("Interaction text")]
    [SerializeField] private string interactionText = "Premi E per muovere il carrello";

    public void Interact()
    {
        Debug.Log("Interazione con " + this.gameObject.name);
    }

    public string getInteractionText()
    {
        return interactionText;
    }
}
