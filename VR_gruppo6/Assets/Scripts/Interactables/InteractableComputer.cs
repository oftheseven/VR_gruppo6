using UnityEngine;

public class InteractableComputer : MonoBehaviour
{
    [Header("Computer configuration")]
    [SerializeField] private string computerID = "computer1";
    [SerializeField] private string interactionText = "Premi E per usare il computer";

    [Header("Green Screens Gestiti")]
    [SerializeField] private GreenScreenTarget[] managedGreenScreens;

    [Header("UI Panels")]
    [SerializeField] private UI_GreenScreenPickerPanel pickerPanel;
    [SerializeField] private UI_ComputerPanel computerPanel; 

    private GreenScreenSelector selector;

    void Start()
    {
        if (computerPanel != null)
        {
            computerPanel.SetComputerID(computerID);
        }
    }

    public void Interact()
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayInteractionSFX();
        }

        if (pickerPanel != null)
        {
            pickerPanel.OpenPicker(this, managedGreenScreens);
        }
        else
        {
            Debug.LogError("PickerPanel non assegnato!");
        }
        
        if (QuestManager.instance != null && QuestManager.instance.IsQuestActive(QuestManager.MainQuest.TutorialGreenScreen))
        {
            
        }
    }

    public void OpenComputerPanel(GreenScreenTarget targetGreenScreen)
    {
        if (computerPanel == null)
        {
            Debug.LogError("ComputerPanel non assegnato!");
            return;
        }
        
        if (targetGreenScreen == null || !targetGreenScreen.IsValid())
        {
            Debug.LogError("Green Screen target invalido!");
            return;
        }
        
        computerPanel.OpenComputer(targetGreenScreen);
    }

    public string getInteractionText()
    {
        int completed = 0;
        foreach (var gs in managedGreenScreens)
        {
            if (gs.isCompleted) completed++;
        }
        
        return interactionText;
    }

    public UI_ComputerPanel GetComputerPanel()
    {
        return computerPanel;
    }

    public GreenScreenTarget GetGreenScreenByID(string id)
    {
        foreach (var gs in managedGreenScreens)
        {
            if (gs.id == id)
            {
                return gs;
            }
        }
        return null;
    }
}
