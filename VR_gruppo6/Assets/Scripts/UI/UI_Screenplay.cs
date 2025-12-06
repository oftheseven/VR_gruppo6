using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_Screenplay : MonoBehaviour
{
    // singleton
    private static UI_Screenplay _screenplayUI;
    public static UI_Screenplay ScreenplayUIInstance => _screenplayUI;

    void Start()
    {
        _screenplayUI = this;
        this.gameObject.SetActive(false); // all'avvio disattivo l'oggetto UI
    }
    public void OpenScreenplay()
    {
        this.gameObject.SetActive(true); // attivo l'oggetto UI se clicco il bottone di apertura
        UI_PlayerCanvas.PlayerCanvasUIInstance.ClosePlayerCanvas(); // chiudo il player canvas se apro lo screenplay
        
        // DA AGGIUNGERE POI UNA PAUSA DELL'APPLICAZIONE MENTRE IL CANVAS E' ATTIVO
    }

    public void CloseScreenplay()
    {
        this.gameObject.SetActive(false); // disattivo l'oggetto UI se clicco il bottone di chiusura
        //UI_PlayerCanvas.PlayerCanvasUIInstance.OpenPlayerCanvas(); // riapro il player canvas se chiudo lo screenplay
    }
}
