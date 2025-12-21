using UnityEngine;

public class Computer : MonoBehaviour, Interactable
{
    [SerializeField] private string interactionText = "Premi E per usare il computer";

    // IMPLEMENENTARE L'INTERAZIONE CON IL COMPUTER
    public void Interact()
    {
        Debug.Log("Interazione con computer");
    }

    public string getInteractionText()
    {
        return interactionText;
    }
}
