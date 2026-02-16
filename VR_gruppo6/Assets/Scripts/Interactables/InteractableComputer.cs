using UnityEngine;
using System.Collections.Generic;

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
        
        if (QuestManager.instance != null && QuestManager.instance.IsQuestActive(QuestManager.TutorialQuest.GreenScreen))
        {
            
        }
        
        // if (selector != null && selector.IsCompleted)
        // {
        //     Debug.Log($"{computerID} già completato!");
        //     return;
        // }

        // if (computerPanel != null)
        // {
        //     computerPanel.OpenComputer();
        // }

        // if (TutorialManager.instance != null)
        // {
        //     Debug.Log($"Computer trovato TutorialManager, chiamo OnComputerCompleted()");
        //     TutorialManager.instance.OnComputerCompleted();
        // }
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
        // if (selector != null && selector.IsCompleted)
        // {
        //     return "Computer completato";
        // }
        // return interactionText;

        int completed = 0;
        foreach (var gs in managedGreenScreens)
        {
            if (gs.isCompleted) completed++;
        }
        
        if (completed == managedGreenScreens.Length && managedGreenScreens.Length > 0)
        {
            return "Computer - Tutti i Green Screen completati";
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
