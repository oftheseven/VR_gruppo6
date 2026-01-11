using UnityEngine;

public class PickableItem : MonoBehaviour
{
    [Header("Item settings")]
    [SerializeField] private string itemID = "Oggetto"; // ID univoco (DEVE CORRISPONDERE AL NOME DEL FILE .txt CON LA SUA DESCRIZIONE)
    [SerializeField] private string itemDisplayName = "Oggetto"; // nome visualizzato nell'inventario
    [SerializeField] private string interactionText = "Premi E per prendere l'oggetto";
    [SerializeField] private Sprite itemIcon;
    [SerializeField] private bool isAccumulable = true;

    [Header("Prefab Reference")]
    [SerializeField] private GameObject itemPrefab;

    private Rigidbody rb;
    private int quantity = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (itemPrefab == null)
        {
            Debug.LogWarning($"{itemDisplayName}:  itemPrefab non assegnato.  Verrà usato questo GameObject.");
        }
    }

    public string GetItemID() => itemID;
    public string GetDisplayName() => itemDisplayName;
    public string GetInteractionText() => interactionText;
    public Sprite GetItemIcon() => itemIcon;
    public bool IsAccumulable() => isAccumulable;
    public int GetQuantity() => quantity;

    public void AddQuantity()
    {
        quantity++;
        Debug.Log($"Quantità di {itemDisplayName}: {quantity}");
    }

    public void RemoveQuantity()
    {
        if (quantity > 0)
        {
            quantity--;
        }
        Debug.Log($"Quantità di {itemDisplayName}: {quantity}");
    }

    public void ResetQuantity()
    {
        quantity = 0;
    }

    public GameObject SpawnInWorld(Vector3 position, Quaternion rotation)
    {
        if (itemPrefab == null)
        {
            Debug.LogError($"Prefab mancante per {itemDisplayName}!");
            return null;
        }

        GameObject spawned = Instantiate(itemPrefab, position, rotation);
        spawned.SetActive(true);
        return spawned;
    }

    public GameObject GetPrefab()
    {
        return itemPrefab != null ? itemPrefab : gameObject;
    }
}
