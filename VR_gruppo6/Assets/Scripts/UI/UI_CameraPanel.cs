using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

public class UI_CameraPanel :  MonoBehaviour
{   
    [Header("Camera reference")]
    [SerializeField] private InteractableCamera interactableCamera;

    [Header("Camera timer settings")]
    [SerializeField] private float holdTimeToClose = 2f;
    [SerializeField] private float cooldownTime = 1f;

    [Header("Info panel reference")]
    [SerializeField] private UI_InfoPanel infoPanel; // reference al pannello info specifico per questa camera

    [Header("Hold to close UI")]
    [SerializeField] private GameObject holdIndicator; // container del cerchio
    [SerializeField] private Image holdFillImage; // image con fill radial

    private bool isOpen = false;
    public bool IsOpen => isOpen;
    private bool canInteract = true;
    public bool CanInteract => canInteract;
    private float holdTimer = 0f;
    public InteractableCamera InteractableCamera => interactableCamera;

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

    void Update()
    {
        if (isOpen)
        {
            HandleCameraClose();
        }
    }

    public void OpenCamera()
    {
        if (interactableCamera == null)
        {
            Debug.LogError("InteractableCamera non assegnata in UI_CameraPanel!");
            return;
        }

        this.gameObject.SetActive(true);
        isOpen = true;
        PlayerController.EnableMovement(false);

        PlayerController.ShowCursor();

        if (PlayerController.instance != null)
        {
            PlayerController.instance.playerCamera.gameObject.SetActive(false);
        }
        
        if (interactableCamera.ViewCamera != null)
        {
            interactableCamera.ViewCamera.gameObject.SetActive(true);
        }

        if (infoPanel != null)
        {
            infoPanel.OnDeviceOpened();
        }
    }

    public void CloseCamera()
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

        PlayerController.HideCursor();

        if (interactableCamera != null)
        {
            interactableCamera.ResetInteractionCooldown();
        }
        
        StartCoroutine(CooldownAndHide());

        if (PlayerController.instance != null)
        {
            PlayerController.instance.playerCamera.gameObject.SetActive(true);
        }
        
        if (interactableCamera != null && interactableCamera.ViewCamera != null)
        {
            interactableCamera.ViewCamera.gameObject.SetActive(false);
        }

        this.gameObject.SetActive(false);
        canInteract = true;
        PlayerController.EnableMovement(true);
    }

    public void HandleCameraClose()
    {
        if (Keyboard.current.eKey.isPressed && !infoPanel.IsOpen)
        {
            holdTimer += Time.deltaTime;

            // mostro l'indicatore quando inizio a premere
            if (holdIndicator != null && !holdIndicator.activeSelf)
            {
                holdIndicator.SetActive(true);
            }

            // aggiorno il fill dell'immagine radiale
            if (holdFillImage != null)
            {
                holdFillImage.fillAmount = Mathf.Clamp01(holdTimer / holdTimeToClose);
            }
            
            if (holdTimer >= holdTimeToClose)
            {
                CloseCamera();
            }
        }
        else
        {
            // resetto quando rilascio il tasto
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

    // private IEnumerator CooldownAndHide()
    // {
    //     canInteract = false;
    //     //Debug.Log("Cooldown iniziato - canInteract = " + canInteract);
    //     yield return new WaitForSeconds(cooldownTime);
    // }

    private IEnumerator CooldownAndHide()
    {
        canInteract = false;
        Debug.Log($"Camera panel cooldown iniziato: {cooldownTime}s");

        yield return new WaitForSeconds(cooldownTime);

        canInteract = true;
        Debug.Log("Camera panel cooldown finito");
    }
}
