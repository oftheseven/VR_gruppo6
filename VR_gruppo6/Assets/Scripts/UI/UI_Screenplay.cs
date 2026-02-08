using System.Collections;
using UnityEngine;

public class UI_Screenplay : MonoBehaviour
{
    // singleton
    private static UI_Screenplay _screenplayUI;
    public static UI_Screenplay instance => _screenplayUI;

    // CAPIRE SE HA PIU' SENSO USARE DEGLI ARRAY PER I TICK
    [Header("Empty tick references")]
    // [SerializeField] private GameObject emptyticks;
    [SerializeField] private GameObject emptyTick1;
    [SerializeField] private GameObject emptyTick2;
    [SerializeField] private GameObject emptyTick3;
    [SerializeField] private GameObject emptyTick4;

    [Header("Full tick references")]
    // [SerializeField] private GameObject fullticks;
    [SerializeField] private GameObject fullTick1;
    [SerializeField] private GameObject fullTick2;
    [SerializeField] private GameObject fullTick3;
    [SerializeField] private GameObject fullTick4;

    private bool isOpen = false;
    public bool IsOpen => isOpen;

    private int greenScreenProgress = 0;

    void Start()
    {
        _screenplayUI = this;
        this.gameObject.SetActive(false); // all'avvio disattivo l'oggetto UI
        EnableAllEmptyTicks();
        DisableAllFullTicks();
    }
    public void OpenScreenplay()
    {   
        if (IsGreenScreenComplete())
        {
            emptyTick1.SetActive(false);
            fullTick1.SetActive(true);

            // FARE IN MODO CHE NEL CASO IN CUI LA MISSIONE E' COMPLETATA, NON SI POSSANO RIAPRIRE I COMPUTER
        }
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

    private void EnableAllEmptyTicks()
    {
        emptyTick1.SetActive(true);
        emptyTick2.SetActive(true);
        emptyTick3.SetActive(true);
        emptyTick4.SetActive(true);
    }

    private void DisableAllFullTicks()
    {
        fullTick1.SetActive(false);
        fullTick2.SetActive(false);
        fullTick3.SetActive(false);
        fullTick4.SetActive(false);
    }

    public void AdvanceGreenScreenProgress()
    {
        if (greenScreenProgress < 2)
        {
            greenScreenProgress++;
        }
    }

    public bool IsGreenScreenComplete()
    {
        if (greenScreenProgress == 2)
        {
            if (GameManager.instance != null)
            {
                GameManager.instance.OnComputerCompleted();
            }
            return true;
        }
        return false;
    }
}