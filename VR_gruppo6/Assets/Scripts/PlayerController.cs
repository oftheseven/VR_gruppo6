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
    //private Vector3 currentVelocity = Vector3.zero;
    private float gravity = 9.81f;
    private Interactable currentInteractable = null;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.freezeRotation = true; // evita rotazioni indesiderate da collisioni
    
        // nascondo e blocco il cursore
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;
    }

    void Update()
    {
        HandleRotation();
        CheckForInteractable();

        if (Keyboard.current.spaceKey.isPressed && IsGrounded())
        {
            Jump();
        }

        if (Keyboard.current.eKey.isPressed && currentInteractable != null && UI_ComputerPanel.instance.isOpen == false)
        {
            currentInteractable.Interact();
            interactiontext.gameObject.SetActive(false);
            UI_ComputerPanel.instance.OpenComputer();
        }
        if (UI_ComputerPanel.instance.isOpen)
        {
            UI_ComputerPanel.instance.HandleComputerClose();
        }

        // tasto ESC per sbloccare il cursore (debug)
        // if (Keyboard. current.escapeKey.isPressed)
        // {
        //     Cursor.lockState = CursorLockMode. None;
        //     Cursor.visible = true;
        // }
    }

    void FixedUpdate()
    {
        Move();
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

    private void OnDrawGizmos()
    {
        Gizmos.color = IsGrounded() ? Color.green : Color.red;
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        Gizmos.DrawLine(origin, origin + Vector3.down * _groundCheckDistance);
    }

    private void CheckForInteractable()
    {
        Vector3 rayOrigin = _cameraTransform.position;
        Vector3 rayDirection = _cameraTransform.forward;

        RaycastHit hit;

        if (Physics.Raycast(rayOrigin, rayDirection, out hit, interactionDistance))
        {
            Interactable interactable = hit.collider.GetComponent<Interactable>();

            if (interactable != null)
            {
                if (currentInteractable != interactable)
                {
                    currentInteractable = interactable;
                    ShowInteractionText(interactable.getInteractionText());
                    Debug.Log(interactable.getInteractionText());
                }
            }
            else
            {
                ClearInteractable();
            }
        }
        else
        {
            ClearInteractable();
        }
    }

    private void ClearInteractable()
    {
        currentInteractable = null;
        interactiontext.gameObject.SetActive(false);
    }

    private void ShowInteractionText(string text)
    {
        interactiontext.text = text;
        interactiontext.gameObject.SetActive(true);
    }

    // DA AGGIUSTARE LA GESTIONE DI CHIUSURA DEL PANNELLO
    // private void HandleComputerClose()
    // {
    //     if (Keyboard.current.eKey.isPressed)
    //     {
    //         holdTimer += Time.deltaTime;
    //         Debug.Log(holdTimer);
    //         if (holdTimer >= 2f)
    //         {
    //             UI_ComputerPanel.instance.CloseComputer();
    //             holdTimer = 0f;
    //         }
    //     }
    //     else
    //     {
    //         holdTimer = 0f;
    //     }
    // }
}