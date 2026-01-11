using System.Collections.Generic;
using UnityEngine;

// script dell'inventario attaccato al player
public class Inventory : MonoBehaviour
{
    // singleton
    private static Inventory _instance;
    public static Inventory instance => _instance;

    private List<PickableItem> items = new List<PickableItem>(); // lista degli oggetti nell'inventario (ovviamente questi oggetti possono essere solo di tipo PickableItem)
    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    public bool AddItem(PickableItem item)
    {
        // if (item != null && !items.Contains(item))
        // {
        //     items.Add(item);
        //     item.gameObject.SetActive(false); // disattiva l'oggetto nella scena
        //     Debug.Log("Oggetto " + item.name + " aggiunto all'inventario. Totale oggetti: " + items.Count);
        //     return true;
        // }
        // return false;

        if (item == null)
        {
            Debug.LogWarning("Tentativo di aggiungere item null");
            return false;
        }

        if (items.Contains(item))
        {
            Debug.LogWarning($"Item {item.GetDisplayName()} già nell'inventario");
            return false;
        }

        items. Add(item);
        item.gameObject.SetActive(false);
        Debug.Log($"Oggetto {item.GetDisplayName()} aggiunto all'inventario.");

        NotifyInventoryChanged();

        return true;
    }

    public void RemoveItem(PickableItem item)
    {
        if (item != null && items.Contains(item))
        {
            items.Remove(item);
            item.transform.position = this.transform.position + this.transform.forward; // posiziona l'oggetto davanti al player
            item.gameObject.SetActive(true); // riattiva l'oggetto nella scena
            Debug.Log("Oggetto " + item.name + " rimosso dall'inventario. Totale oggetti: " + items.Count);

            NotifyInventoryChanged();

            // if (items.Count == 0)
            // {
            //     Debug.Log("L'inventario è vuoto.");
            //     UI_InventoryPanel.instance.CloseInventory();
            // }
        }

        else
        {
            Debug.LogWarning("Impossibile rimuovere l'oggetto " + item.name + " dall'inventario perché non è presente.");
        }
    }

    private void NotifyInventoryChanged()
    {
        if (UI_InventoryPanel.instance != null)
        {
            UI_InventoryPanel.instance.OnInventoryChanged();
        }
    }

    public List<PickableItem> GetItems()
    {
        return new List<PickableItem>(items); // restituisce una copia della lista degli oggetti
    }
}
