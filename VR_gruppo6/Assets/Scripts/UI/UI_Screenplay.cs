using UnityEngine;

public class UI_Screenplay : MonoBehaviour
{
    // singleton
    private static UI_Screenplay _screenplayUI;
    public static UI_Screenplay instance => _screenplayUI;

    private bool isOpen = false;
    public bool IsOpen => isOpen;

    void Start()
    {
        _screenplayUI = this;
        this.gameObject.SetActive(false); // all'avvio disattivo l'oggetto UI
    }
    public void OpenScreenplay()
    {
        this.gameObject.SetActive(true); // attivo l'oggetto UI se clicco il bottone di apertura
        UI_PlayerCanvas.instance.ClosePlayerCanvas(); // chiudo il player canvas se apro lo screenplay
        isOpen = true;
        
        // DA AGGIUNGERE POI UNA PAUSA DELL'APPLICAZIONE MENTRE IL CANVAS E' ATTIVO
        PlayerController.EnableMovement(false);
    }

    public void CloseScreenplay()
    {
        this.gameObject.SetActive(false); // disattivo l'oggetto UI se clicco il bottone di chiusura
        isOpen = false;
        
        // DA AGGIUNGERE POI LA RIPRESA DELL'APPLICAZIONE MENTRE IL CANVAS E' ATTIVO
        PlayerController.EnableMovement(true);
    }
}