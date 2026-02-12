using UnityEngine;

public class PickableItem : MonoBehaviour
{
    [Header("Item settings")]
    [SerializeField] private string itemID = "Oggetto"; // ID univoco (DEVE CORRISPONDERE AL NOME DEL FILE .txt CON LA SUA DESCRIZIONE)
    [SerializeField] private string itemDisplayName = "Oggetto"; // nome visualizzato nell'inventario
    [SerializeField] private string interactionText = "Premi E per prendere l'oggetto";
    [SerializeField] private Sprite itemIcon;
    [SerializeField] private bool isAccumulable = true;
    [SerializeField] private bool isUsable = false;

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
    public bool IsUsable() => isUsable;
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

    public void Use()
    {
        if (!isUsable)
        {
            Debug.LogWarning($"{itemDisplayName} non è usabile!");
            return;
        }

        Debug.Log($"Usando {itemDisplayName}...");

        switch (itemID)
        {
            case "Ciak":
                UseCiak();
                break;

            default:
                Debug.LogWarning($"Nessuna azione USE definita per {itemID}");
                break;
        }
    }

    private void UseCiak()
    {
        Debug.Log("CIAK! Avvio Director Mode!");

        if (DirectorModeManager.instance != null && DirectorModeManager.instance.IsDirectorModeAvailable())
        {
            DirectorModeManager.instance.StartDirectorMode();
        }
        else if (DirectorModeManager.instance != null && !DirectorModeManager.instance.IsDirectorModeAvailable())
        {
            Debug.LogWarning("Director Mode non è ancora disponibile!");
        }
        else
        {
            Debug.LogError("DirectorModeManager non trovato nella scena");
        }
    }

    public GameObject GetPrefab()
    {
        return itemPrefab != null ? itemPrefab : gameObject;
    }
}
