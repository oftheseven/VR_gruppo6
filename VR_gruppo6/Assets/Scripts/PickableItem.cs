using UnityEngine;

public class PickableItem : MonoBehaviour
{
    [Header("Item settings")]
    [SerializeField] private string itemID = "Oggetto"; // ID univoco (DEVE CORRISPONDERE AL NOME DEL FILE .txt CON LA SUA DESCRIZIONE)
    [SerializeField] private string itemDisplayName = "Oggetto"; // nome visualizzato nell'inventario
    [SerializeField] private string interactionText = "Premi E per prendere l'oggetto";

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public string GetItemID()
    {
        return itemID;
    }

    public string GetDisplayName()
    {
        return itemDisplayName;
    }

    public string GetInteractionText()
    {
        return interactionText;
    }
}
