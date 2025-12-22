using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class UI_ComputerPanel :  MonoBehaviour
{
    private static UI_ComputerPanel _computerPanelUI;
    public static UI_ComputerPanel instance => _computerPanelUI;

    public bool isOpen = false;
    public bool canInteract = true;
    private float holdTimer = 0f;
    
    [SerializeField] private float holdTimeToClose = 2f;
    [SerializeField] private float cooldownTime = 1f;

    void Awake()
    {
        if (_computerPanelUI == null)
        {
            _computerPanelUI = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

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