using UnityEngine;

public class InteractableComputer : MonoBehaviour
{
    [Header("Computer configuration")]
    [SerializeField] private string computerID = "computer1";
    [SerializeField] private string interactionText = "Premi E per usare il computer";
    [SerializeField] private UI_ComputerPanel computerPanel;

    private GreenScreenSelector selector;

    void Start()
    {
        if (computerPanel != null)
        {
            computerPanel.SetComputerID(computerID);
            selector = computerPanel.GetComponent<GreenScreenSelector>();
        }
    }

    public void Interact()
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayInteractionSFX();
        }
        
        if (selector != null && selector.IsCompleted)
        {
            Debug.Log($"{computerID} già completato!");
            return;
        }

        if (computerPanel != null)
        {
            computerPanel.OpenComputer();
        }

        if (TutorialManager.instance != null)
        {
            Debug.Log($"Computer trovato TutorialManager, chiamo OnComputerCompleted()");
            TutorialManager.instance.OnComputerCompleted();
        }
    }

    public string getInteractionText()
    {
        if (selector != null && selector.IsCompleted)
        {
            return "Computer completato";
        }
        return interactionText;
    }

    public UI_ComputerPanel GetComputerPanel()
    {
        return computerPanel;
    }
}
