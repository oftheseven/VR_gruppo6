using UnityEngine;
using UnityEngine.InputSystem;

public class RotatingCamera : MonoBehaviour
{
    [Header("Rotation settings")]
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private Transform pivot;

    void Update()
    {
        Rotate();
    }

    private void Rotate()
    {
        if (Keyboard.current.dKey.isPressed)
        {
            transform.localRotation = Quaternion.Euler(0f, transform.localRotation.eulerAngles.y + rotationSpeed * Time.deltaTime, 0f);
        } 
        else if (Keyboard.current.aKey.isPressed)
        {            
            transform.localRotation = Quaternion.Euler(0f, transform.localRotation.eulerAngles.y - rotationSpeed * Time.deltaTime, 0f);
        }
    }
}