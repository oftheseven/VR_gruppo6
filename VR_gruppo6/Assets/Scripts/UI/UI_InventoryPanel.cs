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
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;
    [SerializeField] private Image itemIconImage;
    [SerializeField] private Button dropItemButton;
    [SerializeField] private Button useItemButton;
    [SerializeField] private TextMeshProUGUI itemCountText; // testo che mostra il numero di item di quel tipo nell'inventario

    [Header("Navigation settings")]
    [SerializeField] private Image[] itemPreviewImages; // immagini degli slot dell'inventario
    [SerializeField] private Color selectedColor = Color.green;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color emptySlotColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
    [SerializeField] private float inputCooldown = 0.2f;

    [Header("Item descriptions files")]
    [SerializeField] private TextAsset[] itemDescriptionFiles; // lista di file di testo che contengono le descrizioni degli oggetti

    [Header("Drop info")]
    [SerializeField] private TextMeshProUGUI dropInfoText;

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

        if (useItemButton != null)
        {
            useItemButton.onClick.AddListener(UseSelectedItem);
        }

        if (dropItemButton != null)
        {
            dropItemButton.onClick.AddListener(DropSelectedItem);
        }

        if (detailsPanel != null)
        {
            detailsPanel.SetActive(false);
        }

        InitializeEmptySlots();
    }

    void Update()
    {
        if (isOpen)
        {
            HandleItemNavigation();
        }

        if (isOpen && dropInfoText != null && Inventory.instance != null)
        {
            dropInfoText.text = Inventory.instance.GetNearbyDropZoneInfo();
        }
    }

    public void OpenInventory()
    {
        this.gameObject.SetActive(true); // attivo l'oggetto UI se clicco il bottone di apertura
        PlayerController.EnableMovement(false); // disabilito il movimento del player quando apro l'inventario
        PlayerController.ShowCursor();
        
        selectedIndex = 0;
        RefreshInventory();
        StartCoroutine(CooldownCoroutine());
    }

    public void CloseInventory()
    {
        this.gameObject.SetActive(false); // disattivo l'oggetto UI se clicco il bottone di chiusura
        PlayerController.EnableMovement(true); // riabilito il movimento del player quando chiudo l'inventario
        PlayerController.HideCursor();
        
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

        UpdateInventorySlots();

        if (currentItems.Count == 0)
        {
            // selectedIndex = 0;
            HideItemDetails();
        }
        else
        {
            selectedIndex = Mathf.Clamp(selectedIndex, 0, currentItems. Count - 1);
            ShowSelectedItem();
        }
    }

    private void UpdateInventorySlots()
    {
        for (int i = 0; i < itemPreviewImages.Length; i++)
        {
            if (i < currentItems.Count)
            {
                PickableItem item = currentItems[i];
                Sprite icon = item.GetItemIcon();

                if (icon != null)
                {
                    itemPreviewImages[i].sprite = icon;
                    // itemPreviewImages[i].enabled = true;

                    if (i == selectedIndex)
                    {
                        itemPreviewImages[i].color = selectedColor;
                    }
                    else
                    {
                        itemPreviewImages[i].color = normalColor;
                    }
                }
                else
                {
                    itemPreviewImages[i].sprite = null;
                    itemPreviewImages[i].color = emptySlotColor;
                }
            }
            else
            {
                itemPreviewImages[i].sprite = null;
                itemPreviewImages[i].color = emptySlotColor;
            }
        }
    }

    private void InitializeEmptySlots()
    {
        if (itemPreviewImages == null)
            return;

        for (int i = 0; i < itemPreviewImages.Length; i++)
        {
            if (itemPreviewImages[i] != null)
            {
                itemPreviewImages[i].sprite = null;
                itemPreviewImages[i].color = emptySlotColor;
                itemPreviewImages[i].enabled = true;
            }
        }
    }

    private void LoadAllDescriptions()
    {   
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

        // Debug.Log($"Caricate {descriptions.Count} descrizioni di oggetti.");
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
        int previousIndex = selectedIndex;

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

        if (inputDetected && previousIndex != selectedIndex)
        {
            lastInputTime = Time.time;
            UpdateSlotColors();
            ShowSelectedItem();
        }
    }

    private void UpdateSlotColors()
    {
        for (int i = 0; i < itemPreviewImages.Length; i++)
        {
            if (i < currentItems.Count)
            {
                // slot con oggetto
                if (i == selectedIndex)
                {
                    itemPreviewImages[i].color = selectedColor;
                }
                else
                {
                    itemPreviewImages[i].color = normalColor;
                }
            }
            else
            {
                // slot vuoto
                itemPreviewImages[i].color = emptySlotColor;
            }
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

        if (itemNameText != null)
        {
            itemNameText.text = selectedItem.GetDisplayName();
        }

        if (itemDescriptionText != null)
        {
            itemDescriptionText.text = GetDescription(selectedItem);
        }

        if (itemCountText != null)
        {
            if (selectedItem.IsAccumulable())
            {
                itemCountText.text = $"Quantità: x{selectedItem.GetQuantity()}";
                itemCountText.gameObject.SetActive(true);
            }
            else
            {
                itemCountText.text = "";
                itemCountText.gameObject.SetActive(false);
            }
        }

        if (dropItemButton != null)
        {
            dropItemButton.gameObject.SetActive(true);
        }

        if (useItemButton != null)
        {
            if (selectedItem.IsUsable())
            {
                useItemButton.gameObject.SetActive(true);
            }
            else
            {
                useItemButton.gameObject.SetActive(false);
            }
        }

        if (itemIconImage != null)
        {
            itemIconImage.sprite = selectedItem.GetItemIcon();
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

        if (useItemButton != null)
        {
            useItemButton.gameObject.SetActive(false);
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
            selectedIndex = 0;
        }
    }

    private void UseSelectedItem()
    {
        if (currentItems.Count == 0 || selectedIndex < 0 || selectedIndex >= currentItems.Count)
            return;

        PickableItem itemToUse = currentItems[selectedIndex];

        if (itemToUse != null && itemToUse.IsUsable())
        {
            Debug.Log($"🎬 Usando {itemToUse.GetDisplayName()}...");
            
            itemToUse.Use();
            CloseInventory();
        }
        else
        {
            Debug.LogWarning("⚠️ Item non usabile o null!");
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