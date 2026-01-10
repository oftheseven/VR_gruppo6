using System.Collections;
using UnityEngine;

public class UI_MenuPanel : MonoBehaviour
{
    // singleton
    private static UI_MenuPanel _menuPanelUI;
    public static UI_MenuPanel instance => _menuPanelUI;

    private bool isOpen = false;
    public bool IsOpen => isOpen;

    void Start()
    {
        _menuPanelUI = this;
        this.gameObject.SetActive(false); // all'avvio disattivo l'oggetto UI
    }

    public void OpenMenu()
    {
        this.gameObject.SetActive(true); // attivo l'oggetto UI se clicco il bottone di apertura
        PlayerController.EnableMovement(false); // disabilito il movimento del player quando apro il menu
        StartCoroutine(CooldownCoroutine());
    }

    public void CloseMenu()
    {
        this.gameObject.SetActive(false); // disattivo l'oggetto UI se clicco il bottone di chiusura
        PlayerController.EnableMovement(true); // riabilito il movimento del player quando chiudo il menu
        isOpen = false;
    }

    private IEnumerator CooldownCoroutine()
    {   
        yield return new WaitForSeconds(0.1f);
        isOpen = true;
    }
}