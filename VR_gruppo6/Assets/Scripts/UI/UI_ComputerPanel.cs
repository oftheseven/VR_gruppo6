using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class UI_ComputerPanel :  MonoBehaviour
{   
    [Header("Computer timer settings")]
    [SerializeField] private float holdTimeToClose = 2f;
    [SerializeField] private float cooldownTime = 1f;

    private bool isOpen = false;
    public bool IsOpen => isOpen;
    private bool canInteract = true;
    public bool CanInteract => canInteract;
    private float holdTimer = 0f;

    void Start()
    {
        this.gameObject.SetActive(false);
        canInteract = true;
    }

    public void OpenComputer()
    {
        this.gameObject.SetActive(true);
        isOpen = true;
        //Debug.Log("Computer aperto");
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
    }

    public void CloseComputerImmediate()
    {
        isOpen = false;
        holdTimer = 0f;
        this.gameObject.SetActive(false);
        canInteract = true;
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
}