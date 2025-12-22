using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class UI_ComputerPanel : MonoBehaviour
{
    // singleton
    private static UI_ComputerPanel _computerPanelUI;
    public static UI_ComputerPanel instance => _computerPanelUI;

    // variabili per la gestione di apertura/chiusura del pannello
    public bool isOpen = false;
    public bool canInteract = true;
    private float holdTimer = 0f;
    [SerializeField] private float holdTimeToClose = 2f;

    void Start()
    {
        _computerPanelUI = this;
        this.gameObject.SetActive(false); // all'avvio disattivo l'oggetto UI
        canInteract = true;
    }

    public void OpenComputer()
    {
        this.gameObject.SetActive(true);
        isOpen = true;
        Debug.Log("Computer aperto");
    }

    public void CloseComputer()
    {
        StartCoroutine(Cooldown());
        this.gameObject.SetActive(false);
        isOpen = false;
        Debug.Log("Computer chiuso");
    }

    // DA AGGIUSTARE LA GESTIONE DI CHIUSURA DEL PANNELLO (aggiungere un timer tra la chiusura e la possibilità di riaprirlo)
    public void HandleComputerClose()
    {
        if (Keyboard.current.eKey.isPressed)
        {
            holdTimer += Time.deltaTime;
            Debug.Log(holdTimer);
            if (holdTimer >= 2f)
            {
                CloseComputer();
                holdTimer = 0f;
            }
        }
        else
        {
            holdTimer = 0f;
        }
    }

    private IEnumerator Cooldown()
    {
        canInteract = false;
        yield return new WaitForSeconds(1f);
        canInteract = true;
    }
}