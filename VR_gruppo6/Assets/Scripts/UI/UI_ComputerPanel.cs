using UnityEngine;

public class UI_ComputerPanel : MonoBehaviour
{
    // singleton
    private static UI_ComputerPanel _computerPanelUI;
    public static UI_ComputerPanel instance => _computerPanelUI;

    public bool isOpen = false;

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
}
