using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using System.Runtime.InteropServices;

public class UI_ArmPanel :  MonoBehaviour
{
    [Header("Camera timer settings")]
    [SerializeField] private float holdTimeToClose = 2f;
    [SerializeField] private float cooldownTime = 1f;

    [Header("Info panel reference")]
    [SerializeField] private UI_InfoPanel infoPanel; // reference al pannello info specifico per questa camera

    [Header("Hold to close UI")]
    [SerializeField] private GameObject holdIndicator; // container del cerchio
    [SerializeField] private Image holdFillImage; // image con fill radial

    [Header("Arm reference")]
    [SerializeField] private InteractableArm interactableArm; // reference all'InteractableArm da far muovere

    private bool isOpen = false;
    public bool IsOpen => isOpen;
    private bool canInteract = true;
    public bool CanInteract => canInteract;
    private float holdTimer = 0f;

    private float targetPivot1X = 0f;

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
            HandleArmMovement();
        }
    }

    public void OpenArm()
    {
        this.gameObject.SetActive(true);
        isOpen = true;
        PlayerController.EnableMovement(false);

        if (infoPanel != null)
        {
            infoPanel.OnDeviceOpened();
        }
    }

    public void CloseArm()
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

    public void HandleArmClose()
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
                CloseArm();
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

    private IEnumerator CooldownAndHide()
    {
        canInteract = false;
        //Debug.Log("Cooldown iniziato - canInteract = " + canInteract);
        yield return new WaitForSeconds(cooldownTime);
    }

    // funzione che gestisce le varie rotazioni del braccio meccanico
    private void HandleArmMovement()
    {
        if (InteractableArm.instance.MechanicalArmPivot != null)
        {
            if (Keyboard.current.rightArrowKey.isPressed)
            {
                InteractableArm.instance.MechanicalArmPivot.transform.Rotate(Vector3.up * 20f * Time.deltaTime);
            }
            else if (Keyboard.current.leftArrowKey.isPressed)
            {
                InteractableArm.instance.MechanicalArmPivot.transform.Rotate(Vector3.up * -20f * Time.deltaTime);
            }
        }
 
        if (InteractableArm.instance.Pivot1 != null)
        {
            if (Keyboard.current.upArrowKey.isPressed)
            {
                // InteractableArm.instance.Pivot1.transform.Rotate(Vector3.right * 20f * Time.deltaTime);
                targetPivot1X += InteractableArm.instance.RotationSpeed * Time.deltaTime;
            }
            else if (Keyboard.current.downArrowKey.isPressed)
            {
                // InteractableArm.instance.Pivot1.transform.Rotate(Vector3.right * -20f * Time.deltaTime);
                targetPivot1X -= InteractableArm.instance.RotationSpeed * Time.deltaTime;
            }

            targetPivot1X = Mathf.Clamp(targetPivot1X, InteractableArm.instance.MinPivot1X, InteractableArm.instance.MaxPivot1X);
            Quaternion targetRotation = Quaternion.Euler(targetPivot1X, 0, 0);
            InteractableArm.instance.Pivot1.transform.localRotation = Quaternion.Lerp(
                                                                                      InteractableArm.instance.Pivot1.transform.localRotation,
                                                                                      targetRotation,
                                                                                      Time.deltaTime * 5f // Velocità smoothing
                                                                                     );
        }
    }
}