using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_MenuScript : MonoBehaviour
{
    // singleton
    private static UI_MenuScript _menuScriptUI;
    public static UI_MenuScript MenuScriptUIInstance => _menuScriptUI;

    void Start()
    {
        _menuScriptUI = this;
        this.gameObject.SetActive(false); // all'avvio disattivo l'oggetto UI
    }

    public void OpenMenu()
    {
        this.gameObject.SetActive(true); // attivo l'oggetto UI se clicco il bottone di apertura
        UI_PlayerCanvas.instance.ClosePlayerCanvas(); // chiudo il player canvas se apro il menu
    }

    public void CloseMenu()
    {
        this.gameObject.SetActive(false); // disattivo l'oggetto UI se clicco il bottone di chiusura
        UI_PlayerCanvas.instance.OpenPlayerCanvas(); // riapro il player canvas quando chiudo il menu
    }
}
