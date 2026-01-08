using System.Collections;
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
        PlayerController.EnableMovement(false);
        StartCoroutine(CooldownCoroutine());
    }

    public void CloseScreenplay()
    {
        this.gameObject.SetActive(false); // disattivo l'oggetto UI se clicco il bottone di chiusura
        PlayerController.EnableMovement(true);
        isOpen = false;
    }

    private IEnumerator CooldownCoroutine()
    {   
        yield return new WaitForSeconds(0.1f);
        //Debug.Log("Coroutine finita");
        isOpen = true;
    }
}