using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro;

public class UI_ComputerPanel :  MonoBehaviour
{   
    [Header("Computer timer settings")]
    [SerializeField] private float holdTimeToClose = 2f;
    [SerializeField] private float cooldownTime = 1f;

    [Header("Info panel reference")]
    [SerializeField] private UI_InfoPanel infoPanel; // reference al pannello info specifico per questo computer

    [Header("Hold to close UI")]
    [SerializeField] private GameObject holdIndicator; // container del cerchio
    [SerializeField] private Image holdFillImage; // image con fill radial
    [SerializeField] private TextMeshProUGUI holdText; // tasto da cliccare

    private bool isOpen = false;
    public bool IsOpen => isOpen;
    private bool canInteract = true;
    public bool CanInteract => canInteract;
    private float holdTimer = 0f;

    void Start()
    {
        this.gameObject.SetActive(false);
        canInteract = true;

        if (holdIndicator != null)
        {
            holdIndicator.SetActive(false);
        }

        if (holdFillImage != null)
        {
            holdFillImage.fillAmount = 0;
        }
    }

    public void OpenComputer()
    {
        // se la missione è completa non si può più aprire il computer
        if (UI_Screenplay.instance.IsGreenScreenComplete())
        {
            return;
        }

        this.gameObject.SetActive(true);
        isOpen = true;
        PlayerController.EnableMovement(false);

        if (infoPanel != null)
        {
            infoPanel.OnDeviceOpened();
        }
        else
        {
            Debug.LogWarning("Info panel reference is null in UI_ComputerPanel.");
        }
    }

    public void CloseComputer()
    {
        isOpen = false;
        holdTimer = 0f;

        if (holdIndicator != null)
        {
            holdIndicator.SetActive(false);
        }

        if (infoPanel != null)
        {
            infoPanel.OnDeviceClosed();
        }
        
        StartCoroutine(CooldownAndHide());
        this.gameObject.SetActive(false);
        canInteract = true;
        PlayerController.EnableMovement(true);
    }

    public void CloseComputerImmediate()
    {
        isOpen = false;
        holdTimer = 0f;
        if (holdIndicator != null)
        {
            holdIndicator.SetActive(false);
        }

        if (infoPanel != null)
        {
            infoPanel.OnDeviceClosed();
        }

        this.gameObject.SetActive(false);
        canInteract = true;
        PlayerController.EnableMovement(true);
    }

    public void HandleComputerClose()
    {
        if (Keyboard.current.eKey.isPressed && !infoPanel.IsOpen)
        {
            holdTimer += Time.deltaTime;

            if (holdIndicator != null && !holdIndicator.activeSelf)
            {
                holdIndicator.SetActive(true);
            }

            if (holdFillImage != null)
            {
                holdFillImage.fillAmount = Mathf.Clamp01(holdTimer / holdTimeToClose);
            }
            
            if (holdTimer >= holdTimeToClose)
            {
                CloseComputer();
            }
        }
        else
        {
            holdTimer = 0f;

            if (holdIndicator != null)
            {
                holdIndicator.SetActive(false);
            }

            if (holdFillImage != null)
            {
                holdFillImage.fillAmount = 0;
            }
        }
    }

    private IEnumerator CooldownAndHide()
    {
        canInteract = false;
        yield return new WaitForSeconds(cooldownTime);
    }
}