using System.Collections.Generic;
using UnityEngine;

// script dell'inventario attaccato al player
public class Inventory : MonoBehaviour
{
    // singleton
    private static Inventory _instance;
    public static Inventory instance => _instance;

    // PROBABILMENTE SERVE FARE UN DIZIONARIO PER GLI OGGETTI, ALTRIMENTI E' IMPOSSIBILE GESTIRLI

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

    public void AddItem(PickableItem item)
    {
        if (item != null && !items.Contains(item))
        {
            items.Add(item);
            item.gameObject.SetActive(false); // disattiva l'oggetto nella scena
            Debug.Log("Oggetto " + item.name + " aggiunto all'inventario. Totale oggetti: " + items.Count);
        }
    }

    public void RemoveItem(PickableItem item)
    {
        if (item != null && items.Contains(item))
        {
            items.Remove(item);
            item.transform.position = this.transform.position + this.transform.forward; // posiziona l'oggetto davanti al player
            item.gameObject.SetActive(true); // riattiva l'oggetto nella scena
            Debug.Log("Oggetto " + item.name + " rimosso dall'inventario. Totale oggetti: " + items.Count);

            if (items.Count == 0)
            {
                Debug.Log("L'inventario è vuoto.");
                UI_InventoryPanel.instance.CloseInventory();
            }
        }

        else
        {
            Debug.LogWarning("Impossibile rimuovere l'oggetto " + item.name + " dall'inventario perché non è presente.");
        }
    }

    public List<PickableItem> GetItems()
    {
        return items;
    }
}
