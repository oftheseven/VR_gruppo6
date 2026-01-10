using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using UnityEngine.WSA;
using System;

public class UI_InventoryPanel : MonoBehaviour
{
    // singleton
    private static UI_InventoryPanel _instance;
    public static UI_InventoryPanel instance => _instance;

    [Header("Inventory references")]
    [SerializeField] private string itemIconsFolderName = "ItemIcons"; // cartella che contiene le icone degli oggetti
    [SerializeField] private Inventory inventory;

    [Header("Inventory UI references")]
    [SerializeField] TextMeshProUGUI itemnNameText;
    [SerializeField] TextMeshProUGUI itemDescriptionText;
    [SerializeField] private Button dropItemButton;
    [SerializeField] private string itemDescriptionsFolderName = "ItemDescriptions"; // cartella che contiene le descrizioni degli oggetti

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
    }

    void Start()
    {
        this.gameObject.SetActive(false);
    }

    public void OpenInventory()
    {
        this.gameObject.SetActive(true); // attivo l'oggetto UI se clicco il bottone di apertura
        PlayerController.EnableMovement(false); // disabilito il movimento del player quando apro l'inventario
        LoadInventoryItems();
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

    private void LoadInventoryItems()
    {
        // IMPLEMENTARE IL CODICE PER CARICARE GLI OGGETTI DELL'INVENTARIO NELL'UI
    }
}
