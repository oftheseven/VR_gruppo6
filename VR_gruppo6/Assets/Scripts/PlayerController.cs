using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _runMultiplier = 2f; // moltiplicatore per la corsa
    [SerializeField] private float _rotationSpeed = 3f;
    [SerializeField] private float _maxLookAngle = 89f;

    [Header("Jump")]
    [SerializeField] private float _jumpForce = 5f;
    [SerializeField] private LayerMask _groundLayer; // layer del terreno
    [SerializeField] private float _groundCheckDistance = 0.3f;

    [Header("References")]
    [SerializeField] private Transform _cameraTransform;

    [Header("Interaction")]
    [SerializeField] private float interactionDistance = 1f;
    [SerializeField] private TextMeshProUGUI interactiontext;

    private Rigidbody rb;
    private float verticalRotation = 0f;
    private float gravity = 9.81f;
    private static bool moveEnabled = true; // bool per abilitare/disabilitare il movimento
    public static void EnableMovement(bool enable) { moveEnabled = enable; } // funzione che modifica il valore di moveEnabled in modo che la variabile non sia pubblica

    [Header("Computer interaction")]
    private InteractableComputer currentComputer = null;
    private UI_ComputerPanel currentComputerPanel = null;

    [Header("Camera interaction")]
    private InteractableCamera currentCamera = null;
    private UI_CameraPanel currentCameraPanel = null;

    [Header("Operator interaction")]
    private InteractableOperator currentOperator = null;
    private bool isDialogueActive = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.freezeRotation = true; // evita rotazioni indesiderate da collisioni

        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;
    }

    void Update()
    {
        if (moveEnabled)
        {
            HandleRotation();
        }
        
        CheckForInteractable();

        if (Keyboard.current.spaceKey.isPressed && IsGrounded() && moveEnabled)
        {
            Jump();
        }

        CheckPanelsInteraction(); // controllo aperture e chiusure dei pannelli
        
        HandleComputerInteraction();
        HandleCameraInteraction();
        HandleOperatorInteraction();
    }

    void FixedUpdate()
    {
        if (moveEnabled)
        {
            Move();
        }
        ApplyExtraGravity();
    }

    private void Move()
    {
        Vector2 moveInput = GetMoveInput();

        Vector3 forward = new Vector3(transform.forward.x, 0, transform.forward. z).normalized;
        Vector3 right = new Vector3(transform.right.x, 0, transform.right.z).normalized;

        float currentSpeed = _moveSpeed;
        if (Keyboard.current.leftShiftKey.isPressed)
        {
            currentSpeed *= _runMultiplier;
        }

        Vector3 velocity = (forward * moveInput.y + right * moveInput.x) * currentSpeed;

        rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);
    }

    private void HandleRotation()
    {
        Vector2 lookInput = Mouse.current.delta.ReadValue();

        if (lookInput.sqrMagnitude < 0.01f) return;

        // rotazione orizzontale del corpo del player
        float horizontalRotation = lookInput.x * _rotationSpeed * Time.deltaTime;
        transform.Rotate(Vector3.up * horizontalRotation, Space.World);

        // rotazione verticale
        verticalRotation -= lookInput.y * _rotationSpeed * Time. deltaTime;
        verticalRotation = Mathf.Clamp(verticalRotation, -_maxLookAngle, _maxLookAngle);

        if (_cameraTransform != null)
        {
            _cameraTransform.localEulerAngles = new Vector3(verticalRotation, 0, 0);
        }
        else
        {
            transform.localEulerAngles = new Vector3(verticalRotation, transform.localEulerAngles. y, 0);
        }
    }

    private Vector2 GetMoveInput()
    {
        Vector2 input = Vector2.zero;
        var kb = Keyboard.current;

        if (kb == null) return input;

        if (kb.wKey.isPressed) input.y += 1;
        if (kb.sKey.isPressed) input.y -= 1;
        if (kb.aKey.isPressed) input.x -= 1;
        if (kb.dKey.isPressed) input.x += 1;

        return input. normalized;
    }

    private void Jump()
    {
        // applico una forza verso l'alto
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        rb.linearVelocity += Vector3.up * _jumpForce * Time.fixedDeltaTime;
    }

    private void ApplyExtraGravity()
    {
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * gravity * Time.fixedDeltaTime;
        }
    }

    private bool IsGrounded()
    {
        // raycast verso il basso per controllare se è a terra
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        return Physics.Raycast(origin, Vector3.down, _groundCheckDistance, _groundLayer);
    }

    private void CheckForInteractable()
    {
        if (isDialogueActive)
        {
            return;
        }

        Vector3 rayOrigin = _cameraTransform.position;
        Vector3 rayDirection = _cameraTransform.forward;

        RaycastHit hit;

        if (Physics.Raycast(rayOrigin, rayDirection, out hit, interactionDistance))
        {   
            // switch sul tag che viene rilevato dal raycast
            switch(hit.collider.tag)
            {
                case "Computer":
                    InteractableComputer computer = hit.collider.GetComponent<InteractableComputer>();
                    if (computer != null)
                    {
                        if (currentComputer != computer)
                        {
                            currentComputer = computer;
                            ShowInteractionText(currentComputer.getInteractionText());
                            Debug.Log(currentComputer.getInteractionText());
                        }
                    }
                    else
                    {
                        ClearInteractable();
                    }
                    break;

                case "Camera":
                    InteractableCamera camera = hit.collider.GetComponent<InteractableCamera>();
                    if (camera != null)
                    {
                        if (currentCamera != camera)
                        {
                            currentCamera = camera;
                            ShowInteractionText(currentCamera.getInteractionText());
                            Debug.Log(currentCamera.getInteractionText());
                        }
                    }
                    else
                    {
                        ClearInteractable();
                    }
                    break;
                
                case "Operatore" or "Operatrice":
                    InteractableOperator operatore = hit.collider.GetComponent<InteractableOperator>();
                    if (operatore != null)
                    {
                        if (currentOperator != operatore)
                        {
                            currentOperator = operatore;
                            ShowInteractionText(currentOperator.getInteractionText());
                        }
                    }
                    else
                    {
                        ClearInteractable();
                    }
                    break;
            }
        }
        else
        {
            ClearInteractable();
        }
    }

    private void ClearInteractable()
    {
        currentComputer = null;
        currentCamera = null;
        currentOperator = null;
        interactiontext.gameObject.SetActive(false);
    }

    private void ShowInteractionText(string text)
    {
        interactiontext.text = text;
        interactiontext.gameObject.SetActive(true);
    }

    private void HandleComputerInteraction()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame && currentComputer != null)
        {
            UI_ComputerPanel panel = currentComputer.GetComputerPanel();
            if (panel != null && !panel.IsOpen && panel.CanInteract)
            {
                currentComputer.Interact();
                interactiontext.gameObject.SetActive(false);
                currentComputerPanel = panel;
            }
        }

        if (currentComputerPanel != null && currentComputerPanel.IsOpen)
        {
            currentComputerPanel.HandleComputerClose();
        }
        else
        {
            currentComputerPanel = null;
        }
    }

    private void HandleCameraInteraction()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame && currentCamera != null)
        {
            UI_CameraPanel panel = currentCamera.GetCameraPanel();
            if (panel != null && !panel.IsOpen && panel.CanInteract)
            {
                currentCamera.Interact();
                interactiontext.gameObject.SetActive(false);
                currentCameraPanel = panel;
            }
        }

        if (currentCameraPanel != null && currentCameraPanel.IsOpen)
        {
            currentCameraPanel.HandleCameraClose();
        }
        else
        {
            currentCameraPanel = null;
        }
    }

    private void HandleOperatorInteraction()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame && currentOperator != null && !isDialogueActive)
        {
            currentOperator.Interact();
            interactiontext.gameObject.SetActive(false);
            isDialogueActive = true;
        }
    }

    public void OnDialogueEnded()
    {
        isDialogueActive = false;
        currentOperator = null;
    }

    private void CheckPanelsInteraction()
    {
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            UI_Screenplay.instance.OpenScreenplay();
        }
        if (Keyboard.current.escapeKey.wasPressedThisFrame && UI_Screenplay.instance.IsOpen)
        {
            UI_Screenplay.instance.CloseScreenplay();
        }

        if (Keyboard.current.mKey.wasPressedThisFrame)
        {
            UI_MenuPanel.instance.OpenMenu();
        }
        if (Keyboard.current.escapeKey.wasPressedThisFrame && UI_MenuPanel.instance.IsOpen)
        {
            UI_MenuPanel.instance.CloseMenu();
        }
    }
}