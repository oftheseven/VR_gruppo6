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
        UI_PlayerCanvas.instance.ClosePlayerCanvas(); // chiudo il player canvas se apro il menu
        PlayerController.EnableMovement(false); // disabilito il movimento del player quando apro il menu
        isOpen = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseMenu()
    {
        this.gameObject.SetActive(false); // disattivo l'oggetto UI se clicco il bottone di chiusura
        UI_PlayerCanvas.instance.OpenPlayerCanvas(); // riapro il player canvas quando chiudo il menu
        PlayerController.EnableMovement(true); // riabilito il movimento del player quando chiudo il menu
        isOpen = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}