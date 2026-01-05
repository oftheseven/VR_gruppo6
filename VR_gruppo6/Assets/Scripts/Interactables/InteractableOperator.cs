using UnityEngine;

public class InteractableOperator : MonoBehaviour
{
    [Header("Interaction text")]
    [SerializeField] private string interactionText = "Premi E per eseguire l'interazione";

    public void Interact()
    {
        Debug.Log("Interazione con " + this.gameObject.name);
    }

    public string getInteractionText()
    {
        return interactionText;
    }
}
