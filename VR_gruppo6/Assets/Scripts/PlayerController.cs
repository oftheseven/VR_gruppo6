using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Vector3 spawnPoint = Vector3.zero;
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _runSpeed = 10f;
    [SerializeField] private float _rotationSpeed = 3f;
    [SerializeField] private float _maxLookAngle = 80f; // Limite per guardare su/giù

    private float verticalRotation = 0f;

    void Start()
    {
        this.transform.position = spawnPoint;
    }

    // da modificare, non possiamo lasciare tutto nell'update, questo era giusto per provare al volo
    void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        Vector2 moveInput = Vector2.zero;
        Vector2 lookInput = Vector2.zero;

        // DA FINIRE QUESTO SWITCH PER LA GESTIONE DEI MOVIMENTI (INSERIRE FORSE UNO SWITCH INTERNO PER LA CORSA)
        // var key = Keyboard.current;

        // switch (key)
        // {
        //     case var k when k.wKey.isPressed:
        //         moveInput.y += 1;
        //         Debug.Log("W pressed");
        //         break;
        //     case var k when k.sKey.isPressed:
        //         moveInput.y -= 1;
        //         Debug.Log("S pressed");
        //         break;
        //     case var k when k.aKey.isPressed:
        //         moveInput.x -= 1;
        //         Debug.Log("A pressed");
        //         break;
        //     case var k when k.dKey.isPressed:
        //         moveInput.x += 1;
        //         Debug.Log("D pressed");
        //         break;
        // }

        if (Keyboard.current.wKey.isPressed)
        {
            if (Keyboard.current.leftShiftKey.isPressed)
                moveInput.y += 2;
            else
            moveInput.y += 1;
        }
        if (Keyboard.current.sKey.isPressed)
        {
            if (Keyboard.current.leftShiftKey.isPressed)
                moveInput.y -= 2;
            else
                moveInput.y -= 1;
        }
        if (Keyboard.current.aKey.isPressed)
        {
            if (Keyboard.current.leftShiftKey.isPressed)
                moveInput.x -= 2;
            else
                moveInput.x -= 1;
        }
        if (Keyboard.current.dKey.isPressed)
        {            
            if (Keyboard.current.leftShiftKey.isPressed)
                moveInput.x += 2;
            else
                moveInput.x += 1;
        }

        if (Mouse.current.delta.x.ReadValue() != 0 || Mouse.current.delta.y.ReadValue() != 0)
        {
            lookInput = Mouse.current.delta.ReadValue();
        }

        // Movimento basato solo sulla rotazione Y (orizzontale)
        Vector3 forward = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;
        Vector3 right = new Vector3(transform.right.x, 0, transform.right.z).normalized;
        
        Vector3 move = (forward * moveInput.y + right * moveInput.x) * _moveSpeed * Time.deltaTime;
        this.transform.position += move;

        // Rotazione orizzontale (Y axis)
        Vector3 horizontalRotation = new Vector3(0, lookInput.x, 0) * _rotationSpeed * Time.deltaTime;
        this.transform.Rotate(horizontalRotation, Space.World);

        // Rotazione verticale (X axis) con limiti
        verticalRotation -= lookInput.y * _rotationSpeed * Time.deltaTime;
        verticalRotation = Mathf.Clamp(verticalRotation, -_maxLookAngle, _maxLookAngle);
        this.transform.localEulerAngles = new Vector3(verticalRotation, this.transform.localEulerAngles.y, 0);
    }
}