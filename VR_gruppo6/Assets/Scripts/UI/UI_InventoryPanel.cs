using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class UI_InventoryPanel : MonoBehaviour
{
    // singleton
    private static UI_InventoryPanel _instance;
    public static UI_InventoryPanel instance => _instance;

    [Header("Inventory references")]
    [SerializeField] private Inventory inventory;

    [Header("Item description UI references")]
    [SerializeField] private GameObject detailsPanel; // pannello con nome, descrizione, icona, bottone di drop e quantità dell'oggetto
    [SerializeField] private TextMeshProUGUI itemnNameText;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;
    // [SerializeField] private Image itemIconImage;
    [SerializeField] private Button dropItemButton;
    // [SerializeField] private TextMeshProUGUI itemCountText; // testo che mostra il numero di oggetti nell'inventario

    [Header("Navigation settings")]
    [SerializeField] private float inputCooldown = 0.2f;

    [Header("Item descriptions files")]
    [SerializeField] private TextAsset[] itemDescriptionFiles; // lista di file di testo che contengono le descrizioni degli oggetti

    private Dictionary<string, string> descriptions = new Dictionary<string, string>();
    private List<PickableItem> currentItems = new List<PickableItem>();
    private int selectedIndex = 0;
    private float lastInputTime = 0f;

    private bool isOpen = false;
    public bool IsOpen => isOpen;

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

        LoadAllDescriptions();
    }

    void Start()
    {
        this.gameObject.SetActive(false);

        if (dropItemButton != null)
        {
            dropItemButton.onClick.AddListener(DropSelectedItem);
        }

        if (detailsPanel != null)
        {
            detailsPanel.SetActive(false);
        }
    }

    void Update()
    {
        if (isOpen)
        {
            HandleItemNavigation();
        }
    }

    public void OpenInventory()
    {
        this.gameObject.SetActive(true); // attivo l'oggetto UI se clicco il bottone di apertura
        PlayerController.EnableMovement(false); // disabilito il movimento del player quando apro l'inventario
        RefreshInventory();
        StartCoroutine(CooldownCoroutine());
    }

    public void CloseInventory()
    {
        this.gameObject.SetActive(false); // disattivo l'oggetto UI se clicco il bottone di chiusura
        PlayerController.EnableMovement(true); // riabilito il movimento del player quando chiudo l'inventario
        isOpen = false;
    }

    private IEnumerator CooldownCoroutine()
    {   
        yield return new WaitForSeconds(0.1f);
        isOpen = true;
    }

    private void RefreshInventory()
    {
        if (inventory == null)
        {
            Debug.LogError("Inventario non assegnato!!");
            return;
        }

        currentItems = inventory.GetItems();

        // if (itemCountText != null)
        // {
        //     itemCountText.text = "Oggetti nell'inventario: " + currentItems.Count;
        // }

        if (currentItems.Count == 0)
        {
            selectedIndex = 0;
            HideItemDetails();
        }
        else
        {
            selectedIndex = Mathf.Clamp(selectedIndex, 0, currentItems.Count - 1);
            ShowSelectedItem();
        }
    }

    private void LoadAllDescriptions()
    {
        // foreach (TextAsset file in itemDescriptionFiles)
        // {
        //     string itemName = file.name;
        //     string itemDescription = file.text;

        //     Debug.Log($"Descrizione per l'oggetto {itemName}: {itemDescription}");
        // }

        descriptions.Clear();

        foreach (TextAsset file in itemDescriptionFiles)
        {
            string itemID = file.name;
            string itemDescription = file.text.Trim();

            if (!descriptions.ContainsKey(itemID))
            {
                descriptions.Add(itemID, itemDescription);
            }
            else
            {
                Debug.LogWarning($"Descrizione duplicata per l'oggetto: {itemID}");
            }
        }

        Debug.Log($"Caricate {descriptions.Count} descrizioni di oggetti.");
    }

    private void HandleItemNavigation()
    {
        if (currentItems.Count == 0)
        {
            return;
        }

        if (Time.time < lastInputTime + inputCooldown)
        {
            return;
        }

        bool inputDetected = false;

        if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            selectedIndex = (selectedIndex + 1) % currentItems.Count;
            inputDetected = true;
        }
        else if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            selectedIndex = (selectedIndex - 1 + currentItems.Count) % currentItems.Count;
            inputDetected = true;
        }

        if (inputDetected)
        {
            lastInputTime = Time.time;
            ShowSelectedItem();
        }
    }

    private void ShowSelectedItem()
    {
        if (currentItems.Count == 0 || selectedIndex < 0 || selectedIndex >= currentItems.Count)
        {
            HideItemDetails();
            return;
        }

        PickableItem selectedItem = currentItems[selectedIndex];

        if (selectedItem == null)
        {
            HideItemDetails();
            return;
        }

        if (detailsPanel != null)
        {
            detailsPanel.SetActive(true);
        }

        if (itemnNameText != null)
        {
            itemnNameText.text = selectedItem.GetDisplayName();
        }

        if (itemDescriptionText != null)
        {
            itemDescriptionText.text = GetDescription(selectedItem);
        }

        if (dropItemButton != null)
        {
            dropItemButton.gameObject.SetActive(true);
        }
    }

    private void HideItemDetails()
    {
        if (detailsPanel != null)
        {
            detailsPanel.SetActive(false);
        }

        if (dropItemButton != null)
        {
            dropItemButton.gameObject.SetActive(false);
        }
    }

    private void DropSelectedItem()
    {
        if (currentItems.Count == 0 || selectedIndex < 0 || selectedIndex >= currentItems.Count)
            return;

        PickableItem itemToDrop = currentItems[selectedIndex];

        if (itemToDrop != null && inventory != null)
        {
            inventory.RemoveItem(itemToDrop);
        }
    }

    private string GetDescription(PickableItem item)
    {
        string itemID = item.GetItemID();

        if (descriptions.TryGetValue(itemID, out string description))
        {
            return description;
        }

        if (descriptions.TryGetValue(item.GetDisplayName(), out description))
        {
            return description;
        }

        Debug.LogWarning($"Descrizione non trovata per: {itemID}");
        return "Nessuna descrizione disponibile. ";
    }

    public void OnInventoryChanged()
    {
        if (isOpen)
        {
            RefreshInventory();
        }
    }
}