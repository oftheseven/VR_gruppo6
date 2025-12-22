using UnityEngine;

public class UI_PlayerCanvas : MonoBehaviour
{
    // singleton
    private static UI_PlayerCanvas _playerCanvasUI;
    public static UI_PlayerCanvas instance => _playerCanvasUI;

    void Start()
    {
        _playerCanvasUI = this;
        this.gameObject.SetActive(true); // all'avvio attivo l'oggetto UI
    }

    public void OpenPlayerCanvas()
    {
        this.gameObject.SetActive(true); // attivo l'oggetto UI se clicco il bottone di apertura
        UI_Screenplay.instance.CloseScreenplay(); // chiudo lo screenplay se apro il player canvas
    }

    public void ClosePlayerCanvas()
    {
        this.gameObject.SetActive(false); // disattivo l'oggetto UI se clicco il bottone di chiusura
    }
}