using System.Collections;
using UnityEngine;

public class UI_Screenplay : MonoBehaviour
{
    // singleton
    private static UI_Screenplay _screenplayUI;
    public static UI_Screenplay instance => _screenplayUI;

    // CAPIRE SE HA PIU' SENSO USARE DEGLI ARRAY PER I TICK
    [Header("Empty Tick References")]
    // [SerializeField] private GameObject emptyticks;
    [SerializeField] private GameObject emptyTick1;
    [SerializeField] private GameObject emptyTick2;
    [SerializeField] private GameObject emptyTick3;
    [SerializeField] private GameObject emptyTick4;

    [Header("Full Tick References")]
    // [SerializeField] private GameObject fullticks;
    [SerializeField] private GameObject fullTick1;
    [SerializeField] private GameObject fullTick2;
    [SerializeField] private GameObject fullTick3;
    [SerializeField] private GameObject fullTick4;

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