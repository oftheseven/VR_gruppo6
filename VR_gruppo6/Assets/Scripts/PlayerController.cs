using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float smoothTime = 0.1f;

    [Header("References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Rigidbody rb;

    private Vector3 currentVelocity;
    private Vector3 moveDirection;
    private Vector2 moveInput;

    private void Start()
    {
        // Ottieni il Rigidbody se non è assegnato nell'inspector
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        // Configura il Rigidbody per evitare rotazioni indesiderate
        rb.freezeRotation = true;

        // Ottieni la main camera se non è assegnata
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void Update()
    {
        // Leggi l'input dal New Input System
        moveInput = GetMoveInput();

        if (IsKeyPressed())
        {
            // Crea il vettore di input
            Vector3 inputDirection = new Vector3(moveInput.x, 0f, moveInput.y);

            // Trasforma la direzione in base alla camera
            moveDirection = mainCamera.transform.TransformDirection(inputDirection);
            moveDirection.y = 0f; // Mantieni il movimento sul piano orizzontale
            moveDirection. Normalize();

            // Ruota il player verso la direzione di movimento
            if (moveDirection. magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time. deltaTime);
            }
        }
        else
        {
            moveDirection = Vector3.zero;
        }
    }

    private void FixedUpdate()
    {
        // Usa FixedUpdate per i movimenti con Rigidbody
        if (moveDirection.magnitude > 0.1f)
        {
            // Calcola la velocità target
            Vector3 targetVelocity = moveDirection * movementSpeed;

            // Smooth del movimento per renderlo più fluido
            Vector3 velocity = Vector3.SmoothDamp(rb.linearVelocity, targetVelocity, ref currentVelocity, smoothTime);
            
            // Mantieni la componente Y della velocità (gravità)
            velocity.y = rb.linearVelocity.y;

            // Applica la velocità
            rb.linearVelocity = velocity;
        }
        else
        {
            // Ferma gradualmente il player
            Vector3 velocity = Vector3.SmoothDamp(rb.linearVelocity, Vector3.zero, ref currentVelocity, smoothTime);
            velocity.y = rb.linearVelocity.y;
            rb.linearVelocity = velocity;
        }
    }

    private Vector2 GetMoveInput()
    {
        // Usa il New Input System
        Vector2 input = Vector2.zero;
        
        // Tastiera WASD / Frecce
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
                input.y += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
                input.y -= 1f;
            if (keyboard. dKey.isPressed || keyboard.rightArrowKey.isPressed)
                input.x += 1f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                input.x -= 1f;
        }

        // Gamepad (opzionale)
        Gamepad gamepad = Gamepad.current;
        if (gamepad != null)
        {
            input += gamepad.leftStick.ReadValue();
        }

        return input;
    }

    private bool IsKeyPressed()
    {
        return moveInput.magnitude > 0.01f;
    }
}