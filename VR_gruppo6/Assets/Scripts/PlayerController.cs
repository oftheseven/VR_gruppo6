using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // singleton
    private static PlayerController _instance;
    public static PlayerController instance => _instance;

    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _runMultiplier = 2f; // moltiplicatore per la corsa
    [SerializeField] private float _rotationSpeed = 3f;
    [SerializeField] private float _maxLookAngle = 89f;

    [Header("Jump")]
    [SerializeField] private float _jumpForce = 5f;
    [SerializeField] private LayerMask _groundLayer; // layer del terreno

    private bool _isGrounded = false;
    private int _groundContactCount = 0;

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

    [Header("Cart interaction")]
    private InteractableCart currentCart = null;
    private InteractableCart pushingCart = null;

    [Header("Item interaction")]
    private PickableItem currentItem = null;

    [Header("Dolly interaction")]
    private InteractableDolly currentDolly = null;
    private UI_DollyPanel currentDollyPanel = null;

    [Header("Arm interaction")]
    private InteractableArm currentArm = null;
    private UI_ArmPanel currentArmPanel = null;

    private bool isInteracting = false;
    public Camera playerCamera => _cameraTransform.GetComponent<Camera>();

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.freezeRotation = true;

        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;
    }

    void Update()
    {
        CheckForInteractable(); // controllo se c'è un oggetto interagibile davanti al player
        CheckPanelsInteraction(); // controllo aperture e chiusure dei pannelli
        
        HandleComputerInteraction();
        HandleCameraInteraction();
        HandleOperatorInteraction();
        HandleCartInteraction();
        HandleItemInteraction();
        HandleDollyInteraction();
        HandleArmInteraction();
    }

    void FixedUpdate()
    {
        if (moveEnabled)
        {
            Move();
            HandleRotation();
        }

        ApplyExtraGravity();

        if (Keyboard.current.spaceKey.isPressed && _isGrounded && moveEnabled)
        {
            Jump();
        }
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

    public Vector2 GetMoveInput()
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

    private void OnCollisionEnter(Collision collision)
    {
        if (IsGroundLayer(collision.gameObject))
        {
            _groundContactCount++;
            _isGrounded = true;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (IsGroundLayer(collision.gameObject))
        {
            _isGrounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (IsGroundLayer(collision.gameObject))
        {
            _groundContactCount--;

            if (_groundContactCount <= 0)
            {
                _groundContactCount = 0;
                _isGrounded = false;
            }
        }
    }

    private bool IsGroundLayer(GameObject obj)
    {
        return ((1 << obj.layer) & _groundLayer) != 0;
    }

    private void ApplyExtraGravity()
    {
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * gravity * Time.fixedDeltaTime;
        }
    }

    private void CheckForInteractable()
    {
        if (isDialogueActive || isInteracting)
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
                        if (currentComputer != computer && !UI_Screenplay.instance.IsGreenScreenComplete())
                        {
                            currentComputer = computer;
                            ShowInteractionText(currentComputer.getInteractionText());
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
                            ShowInteractionText(currentCamera.GetInteractionText());
                        }
                    }
                    else
                    {
                        ClearInteractable();
                    }
                    break;
                
                case "Operatore":
                    InteractableOperator operatore = hit.collider.GetComponent<InteractableOperator>();
                    if (operatore != null)
                    {
                        if (currentOperator != operatore)
                        {
                            currentOperator = operatore;
                            ShowInteractionText(currentOperator.GetInteractionText());
                        }
                    }
                    else
                    {
                        ClearInteractable();
                    }
                    break;
                
                case "Carrello":
                    InteractableCart carrello = hit.collider.GetComponent<InteractableCart>();
                    if (carrello != null)
                    {
                        if (currentCart != carrello)
                        {
                            currentCart = carrello;
                            ShowInteractionText(currentCart.GetInteractionText());
                        }
                    }
                    else
                    {
                        ClearInteractable();
                    }
                    break;

                case "Pickable":
                    PickableItem item = hit.collider.GetComponent<PickableItem>();
                    if (item != null)
                    {
                        if (currentItem != item)
                        {
                            currentItem = item;
                            ShowInteractionText(item.GetInteractionText());
                        }
                    }
                    else
                    {
                        ClearInteractable();
                    }
                    break;
                
                case "Dolly":
                    InteractableDolly dolly = hit.collider.GetComponent<InteractableDolly>();
                    if (dolly != null)
                    {
                        if (currentDolly != dolly)
                        {
                            currentDolly = dolly;
                            ShowInteractionText(currentDolly.GetInteractionText());
                        }
                    }
                    else
                    {
                        ClearInteractable();
                    }
                    break;
                
                case "Arm":
                    InteractableArm arm = hit.collider.GetComponent<InteractableArm>();
                    if (arm != null)
                    {
                        Debug.Log("Rilevato braccio interagibile");
                        if (currentArm != arm)
                        {
                            currentArm = arm;
                            ShowInteractionText(currentArm.GetInteractionText());
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
        currentCart = null;
        currentItem = null;
        currentDolly = null;
        currentArm = null;
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
                isInteracting = true;
                interactiontext.gameObject.SetActive(false);
                currentComputerPanel = panel;
            }
        }

        if (currentComputerPanel != null && currentComputerPanel.IsOpen)
        {
            currentComputerPanel.HandleComputerClose();
        }
        else if (currentComputerPanel != null && !currentComputerPanel.IsOpen)
        {
            currentComputerPanel = null;
            isInteracting = false;
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
                isInteracting = true;
                interactiontext.gameObject.SetActive(false);
                currentCameraPanel = panel;
            }
        }

        if (currentCameraPanel != null && currentCameraPanel.IsOpen)
        {
            currentCameraPanel.HandleCameraClose();
        }
        else if (currentCameraPanel != null && !currentCameraPanel.IsOpen)
        {
            currentCameraPanel = null;
            isInteracting = false;
        }
    }

    private void HandleOperatorInteraction()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame && currentOperator != null && !isDialogueActive)
        {
            currentOperator.Interact();
            isInteracting = true;
            interactiontext.gameObject.SetActive(false);
            isDialogueActive = true;
        }
    }

    private void HandleCartInteraction()
    {
        if (pushingCart != null)
        {
            // l'utente sta già tenendo il carrello
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                pushingCart.Interact();
                interactiontext.gameObject.SetActive(false);
                isInteracting = false;
            }
        }
        else
        {   
            // l'utente non sta tenendo niente
            if (Keyboard.current.eKey.wasPressedThisFrame && currentCart != null && !isDialogueActive)
            {
                currentCart.Interact();
                interactiontext.gameObject.SetActive(false);
                isInteracting = true;
            }
        }
    }

    private void HandleItemInteraction()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame && currentItem != null && !isDialogueActive && !isInteracting)
        {
            bool success = Inventory.instance.AddItem(currentItem);
            if (success)
            {
                interactiontext.gameObject.SetActive(false);
                currentItem = null;
            }
        }
    }

    private void HandleArmInteraction()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame && currentArm != null)
        {
            UI_ArmPanel panel = currentArm.GetArmPanel();
            if (panel != null && !panel.IsOpen && panel.CanInteract)
            {
                currentArm.Interact();
                isInteracting = true;
                interactiontext.gameObject.SetActive(false);
                currentArmPanel = panel;
            }
        }

        if (currentArmPanel != null && currentArmPanel.IsOpen)
        {
            currentArmPanel.HandleArmClose();
        }
        else if (currentArmPanel != null && !currentArmPanel.IsOpen)
        {
            currentArmPanel = null;
            isInteracting = false;
        }
    }

    private void HandleDollyInteraction()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame && currentDolly != null)
        {
            UI_DollyPanel panel = currentDolly.GetDollyPanel();
            if (panel != null && !panel.IsOpen && panel.CanInteract)
            {
                currentDolly.Interact();
                isInteracting = true;
                interactiontext.gameObject.SetActive(false);
                currentDollyPanel = panel;
            }
        }

        if (currentDollyPanel != null && currentDollyPanel.IsOpen)
        {
            currentDollyPanel.HandleDollyClose();
        }
        else if (currentDollyPanel != null && !currentDollyPanel.IsOpen)
        {
            currentDollyPanel = null;
            isInteracting = false;
        }
    }

    public void SetPushingCart(InteractableCart cart)
    {
        pushingCart = cart;

        if (cart != null)
        {
            _moveSpeed *= 0.7f;
        }
        else
        {
            _moveSpeed /= 0.7f;
        }
    }

    public void OnDialogueEnded()
    {
        isDialogueActive = false;
        currentOperator = null;
        isInteracting = false;
    }

    private void CheckPanelsInteraction()
    {
        // APERTURA/CHIUSURA MENU
        if (Keyboard.current.escapeKey.wasPressedThisFrame && !isInteracting)
        {
            UI_MenuPanel.instance.OpenMenu();
        }
        if (Keyboard.current.escapeKey.wasPressedThisFrame && UI_MenuPanel.instance.IsOpen)
        {
            UI_MenuPanel.instance.CloseMenu();
        }

        // APERTURA/CHIUSURA SCREENPLAY
        if (Keyboard.current.tabKey.wasPressedThisFrame && !isInteracting)
        {
            UI_Screenplay.instance.OpenScreenplay();
        }
        if (Keyboard.current.tabKey.wasPressedThisFrame && UI_Screenplay.instance.IsOpen)
        {
            UI_Screenplay.instance.CloseScreenplay();
        }

        // APERTURA/CHIUSURA INVENTARIO
        if (Keyboard.current.iKey.wasPressedThisFrame && !isInteracting)
        {
            UI_InventoryPanel.instance.OpenInventory();
        }
        if (Keyboard.current.iKey.wasPressedThisFrame && UI_InventoryPanel.instance.IsOpen)
        {
            UI_InventoryPanel.instance.CloseInventory();
        }
    }
}