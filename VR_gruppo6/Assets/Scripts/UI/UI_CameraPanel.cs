using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class UI_CameraPanel :  MonoBehaviour
{   
    [Header("Camera timer settings")]
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

    public void OpenCamera()
    {
        this.gameObject.SetActive(true);
        isOpen = true;
        PlayerController.EnableMovement(false);
        //Debug.Log("Computer aperto");
    }

    public void CloseCamera()
    {
        isOpen = false;
        holdTimer = 0f;
        //Debug.Log("Camera chiuso - avvio cooldown");
        
        StartCoroutine(CooldownAndHide());
        this.gameObject.SetActive(false);
        canInteract = true;
        //Debug.Log("Cooldown terminato - canInteract = " + canInteract);
        PlayerController.EnableMovement(true);
    }

    public void CloseCameraImmediate()
    {
        isOpen = false;
        holdTimer = 0f;
        this.gameObject.SetActive(false);
        canInteract = true;
        PlayerController.EnableMovement(true);
    }

    public void HandleCameraClose()
    {
        if (Keyboard. current.eKey.isPressed)
        {
            holdTimer += Time.deltaTime;
            
            if (holdTimer >= holdTimeToClose)
            {
                CloseCamera();
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