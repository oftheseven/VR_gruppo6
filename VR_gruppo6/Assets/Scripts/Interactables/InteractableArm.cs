using UnityEngine;

public class InteractableArm : MonoBehaviour
{
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
    [SerializeField] private float minPivot2X = -12f;
    [SerializeField] private float maxPivot2X = 12f;

    public float RotationSpeed => rotationSpeed;
    public GameObject MechanicalArmPivot => mechanicalArmPivot;
    public GameObject Pivot1 => pivot1;
    public float MinPivot1X => minPivot1X;
    public float MaxPivot1X => maxPivot1X;
    public GameObject Pivot2 => pivot2;
    public float MinPivot2X => minPivot2X;
    public float MaxPivot2X => maxPivot2X;

    public void Interact()
    {
        // Debug.Log("Interazione con " + this.name);

        if (armPanel != null)
        {
            armPanel.OpenArm();
        }

        if (TutorialManager.instance != null)
        {
            TutorialManager.instance.OnArmCompleted();
        }

        // if (GameManager.instance != null)
        // {
        //     GameManager.instance.OnArmCompleted();
        // }
    }

    public void OnAccuracyAchieved(float accuracy)
    {
        Debug.Log($"Accuracy ottenuta: {accuracy:F1}%");

        if (TortaInTestaManager.instance != null)
        {
            TortaInTestaManager.instance.OnArmAccuracyAchieved(accuracy);
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
