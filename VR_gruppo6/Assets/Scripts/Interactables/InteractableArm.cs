using UnityEngine;

public class InteractableArm : MonoBehaviour
{
    // singleton
    private static InteractableArm _instance;
    public static InteractableArm instance => _instance;

    [Header("Interaction text")]
    [SerializeField] private string interactionText = "Premi E per gestire il braccio";

    [Header("Arm panel reference")]
    [SerializeField] private UI_ArmPanel armPanel;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void Interact()
    {
        Debug.Log("Interazione con " + this.name);

        if (armPanel != null)
        {
            armPanel.OpenArm();
        }
    }

    public string GetInteractionText()
    {
        return interactionText;
    }

    public UI_ArmPanel GetArmPanel()
    {
        return armPanel;
    }
}
