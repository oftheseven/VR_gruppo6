using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

public class UI_ComputerPanel :  MonoBehaviour
{   
    [Header("Computer timer settings")]
    [SerializeField] private float holdTimeToClose = 2f;
    [SerializeField] private float cooldownTime = 1f;

    [Header("Info Panel")]
    [SerializeField] private GameObject infoPanel; // pannello con le informazioni
    [SerializeField] private Button infoButton; // bottone per riaprire le info
    [SerializeField] private Button closeInfoButton; // bottone per chiudere le info

    private bool isOpen = false;
    public bool IsOpen => isOpen;
    private bool canInteract = true;
    public bool CanInteract => canInteract;
    private float holdTimer = 0f;
    private bool isFirstTime = true;

    void Start()
    {
        this.gameObject.SetActive(false);
        canInteract = true;
    }

    public void OpenComputer()
    {
        this.gameObject.SetActive(true);
        isOpen = true;
        PlayerController.EnableMovement(false);
        //Debug.Log("Computer aperto");

        if (isFirstTime)
        {
            ShowInfoPanel();
            isFirstTime = false;

            if (infoButton != null)
            {
                infoButton.gameObject.SetActive(false);
            }
        }

        else
        {
            if (infoButton != null)
            {
                infoButton.gameObject.SetActive(true);
            }

            if (infoPanel != null)
            {
                infoPanel.SetActive(false);
            }
        }
    }

    public void CloseComputer()
    {
        isOpen = false;
        holdTimer = 0f;
        //Debug.Log("Computer chiuso - avvio cooldown");
        
        StartCoroutine(CooldownAndHide());
        this.gameObject.SetActive(false);
        canInteract = true;
        //Debug.Log("Cooldown terminato - canInteract = " + canInteract);
        PlayerController.EnableMovement(true);
    }

    public void CloseComputerImmediate()
    {
        isOpen = false;
        holdTimer = 0f;
        this.gameObject.SetActive(false);
        canInteract = true;
        PlayerController.EnableMovement(true);
    }

    public void HandleComputerClose()
    {
        if (Keyboard. current.eKey.isPressed)
        {
            holdTimer += Time.deltaTime;
            
            if (holdTimer >= holdTimeToClose)
            {
                CloseComputer();
            }
        }
        else
        {
            holdTimer = 0f;
        }
    }

    private IEnumerator CooldownAndHide()
    {
        canInteract = false;
        //Debug.Log("Cooldown iniziato - canInteract = " + canInteract);
        yield return new WaitForSeconds(cooldownTime);
    }

    public void ShowInfoPanel()
    {
        if (infoPanel != null)
        {
            infoPanel.SetActive(true);
        }

        if (infoButton != null)
        {
            infoButton.gameObject.SetActive(false);
        }
    }

    public void HideInfoPanel()
    {
        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }

        if (infoButton != null && ! isFirstTime)
        {
            infoButton.gameObject. SetActive(true);
        }
    }
}