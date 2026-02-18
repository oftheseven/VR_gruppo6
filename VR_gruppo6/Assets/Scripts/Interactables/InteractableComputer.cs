using UnityEngine;

public class InteractableComputer : MonoBehaviour
{
    [Header("Computer configuration")]
    [SerializeField] private string interactionText = "[E] per usare il computer";

    [Header("Green Screens Gestiti")]
    [SerializeField] private GreenScreenTarget[] managedGreenScreens;

    [Header("UI Panels")]
    [SerializeField] private UI_GreenScreenPanel greenScreenPanel; // reference al pannello UI specifico per questo computer

    void Awake()
    {
        foreach (var gs in managedGreenScreens)
        {
            if (gs != null && gs.previewCamera != null)
            {
                gs.previewCamera.gameObject.SetActive(false);
                gs.previewRenderer.gameObject.SetActive(false);
            }
        }
    }

    void Start()
    {
        if (greenScreenPanel != null)
        {
            greenScreenPanel.SetGreenScreens(managedGreenScreens);
        }
    }

    public void Interact()
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayInteractionSFX();
        }

        if (greenScreenPanel != null)
        {
            greenScreenPanel.OpenPanel();
        }
        else
        {
            Debug.LogError("GreenScreenPanel non assegnato!");
        }
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

    public UI_GreenScreenPanel GetGreenScreenPanel()
    {
        return greenScreenPanel;
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