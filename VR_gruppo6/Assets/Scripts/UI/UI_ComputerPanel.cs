using UnityEngine;
using UnityEngine.InputSystem;

public class UI_ComputerPanel : MonoBehaviour
{
    // singleton
    private static UI_ComputerPanel _computerPanelUI;
    public static UI_ComputerPanel instance => _computerPanelUI;

    // variabili per la gestione di apertura/chiusura del pannello
    public bool isOpen = false;
    private float holdTimer = 0f;

    void Start()
    {
        _computerPanelUI = this;
        this.gameObject.SetActive(false); // all'avvio disattivo l'oggetto UI
    }

    public void OpenComputer()
    {
        this.gameObject.SetActive(true);
        isOpen = true;
    }

    public void CloseComputer()
    {
        this.gameObject.SetActive(false);
        isOpen = false;
    }

    // DA AGGIUSTARE LA GESTIONE DI CHIUSURA DEL PANNELLO
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
}
