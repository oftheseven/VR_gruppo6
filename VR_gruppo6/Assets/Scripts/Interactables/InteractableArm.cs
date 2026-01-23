using Unity.VisualScripting;
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

    [Header("Arm pivots references")]
    [SerializeField] private GameObject mechanicalArmPivot;
    [SerializeField] private GameObject pivot1;
    [SerializeField] private GameObject pivot2;

    [Header("Arm limits")]
    [SerializeField] private float rotationSpeed = 20f;
    [SerializeField] private float minPivot1X = -12f;
    [SerializeField] private float maxPivot1X = 12f;

    public float RotationSpeed => rotationSpeed;
    public GameObject MechanicalArmPivot => mechanicalArmPivot;
    public GameObject Pivot1 => pivot1;
    public float MinPivot1X => minPivot1X;
    public float MaxPivot1X => maxPivot1X;
    public GameObject Pivot2 => pivot2;

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
