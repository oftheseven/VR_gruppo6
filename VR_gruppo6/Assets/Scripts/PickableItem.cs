using UnityEngine;

public class PickableItem : MonoBehaviour
{
    // [Header("Item settings")]
    // [SerializeField] private string itemName = "Oggetto";
    // [SerializeField] private string interactionText = "Premi E per prendere l'oggetto";

    private Rigidbody rb;
    // private Collider collider; 
    // private bool isInCart = false;
    // private InteractableCart currentCart = null;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // collider = GetComponent<Collider>();
    }
}
