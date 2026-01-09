using UnityEngine;

public class InteractableComputer : MonoBehaviour
{
    [Header("Interaction text")]
    [SerializeField] private string interactionText = "Premi E per usare il computer";

    [Header("Computer panel reference")]
    [SerializeField] private UI_ComputerPanel computerPanel;

    public void Interact()
    {
        Debug.Log("Interazione con " + this.gameObject.name);

        if (computerPanel != null)
        {
            computerPanel.OpenComputer();
        }
    }

    public string getInteractionText()
    {
        return interactionText;
    }

    public UI_ComputerPanel GetComputerPanel()
    {
        return computerPanel;
    }
}
