using UnityEngine;

public class InteractableDolly : MonoBehaviour
{
    // singleton 
    private static InteractableDolly _instance;
    public static InteractableDolly instance => _instance;

    [Header("Interaction text")]
    [SerializeField] private string interactionText = "Premi E per muovere il dolly";

    [Header("Dolly panel reference")]
    [SerializeField] private UI_DollyPanel dollyPanel;

    // void Awake()
    // {
    //     if (_instance == null)
    //     {
    //         _instance = this;
    //     }
    //     else
    //     {
    //         Destroy(this.gameObject);
    //     }
    // }

    public void Interact()
    {
        Debug.Log("Interazione con " + this.name);

        if (dollyPanel != null)
        {
            dollyPanel.OpenDolly();
        }
    }

    public string GetInteractionText()
    {
        return interactionText;
    }

    public UI_DollyPanel GetDollyPanel()
    {
        return dollyPanel;
    }
}
