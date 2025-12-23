using Unity.VisualScripting;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    [Header("Interaction text")]
    [SerializeField] private string interactionText = "Premi E per eseguire l'interazione";

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
