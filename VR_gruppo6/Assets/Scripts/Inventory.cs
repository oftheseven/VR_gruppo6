using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// script dell'inventario attaccato al player
public class Inventory : MonoBehaviour
{
    // singleton
    private static Inventory _instance;
    public static Inventory instance => _instance;
    private Dictionary<string, PickableItem> itemTemplates = new Dictionary<string, PickableItem>();

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
        if (item == null)
        {
            return false;
        }

        string itemID = item.GetItemID();

        if (item.IsAccumulable() && itemTemplates.ContainsKey(itemID))
        {
            PickableItem template = itemTemplates[itemID];

            template.AddQuantity();
            Destroy(item.gameObject); // rimuovo l'oggetto dalla scena
            NotifyInventoryChanged();
            return true;
        }
        else if (!item.IsAccumulable() && itemTemplates.ContainsKey(itemID))
        {
            return false;
        }
        else
        {
            item.AddQuantity();
            item.gameObject.SetActive(false); // disattivo l'oggetto nella scena
            itemTemplates.Add(itemID, item);
            NotifyInventoryChanged();
            return true;
        }
    }

    public void RemoveItem(string itemID)
    {
        if (!itemTemplates.ContainsKey(itemID))
        {
            return;
        }

        PickableItem item = itemTemplates[itemID];

        SpawnItemInWorld(item);
        item.RemoveQuantity();

        if (item.GetQuantity() <= 0)
        {
            itemTemplates.Remove(itemID);
            Destroy(item.gameObject);
        }
        NotifyInventoryChanged();
    }

    public void RemoveItem(PickableItem item)
    {
        if (item != null)
        {
            RemoveItem(item.GetItemID());
        }
    }

    private void SpawnItemInWorld(PickableItem item)
    {
        Vector3 spawnPosition = transform.position + transform.forward * 1.0f;
        spawnPosition += new Vector3(
                                    Random.Range(-0.3f, 0.3f),
                                    0.5f,
                                    Random.Range(-0.3f, 0.3f)
                                    );

        GameObject spawnedItem = item.SpawnInWorld(spawnPosition, Quaternion.identity);

        if (spawnedItem != null)
        {
            Rigidbody rb = spawnedItem.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 randomForce = new Vector3(
                                                Random.Range(-1f, 1f),
                                                Random.Range(0.5f, 1.5f),
                                                Random.Range(-1f, 1f)
                                                );
                rb.AddForce(randomForce, ForceMode.Impulse);
            }
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
        return itemTemplates.Values.ToList();
    }

    public int GetUniqueItemCount()
    {
        return itemTemplates.Count;
    }

    public int GetTotalItemCount()
    {
        int total = 0;
        foreach (var item in itemTemplates.Values)
        {
            total += item. GetQuantity();
        }
        return total;
    }

    public PickableItem GetItem(string itemID)
    {
        return itemTemplates.ContainsKey(itemID) ? itemTemplates[itemID] :  null;
    }
}
