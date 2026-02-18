using UnityEngine;
using UnityEngine.InputSystem;

public class RotatingCamera : MonoBehaviour
{
    // [Header("Rotation settings")]
    // [SerializeField] private float rotationSpeed = 10f;

    // void Update()
    // {
    //     Rotate();
    // }

    // private void Rotate()
    // {
    //     if (Keyboard.current.dKey.isPressed)
    //     {
    //         this.transform.localRotation = Quaternion.Euler(0f, transform.localRotation.eulerAngles.y + rotationSpeed * Time.deltaTime, 0f);
    //     } 
    //     else if (Keyboard.current.aKey.isPressed)
    //     {            
    //         this.transform.localRotation = Quaternion.Euler(0f, transform.localRotation.eulerAngles.y - rotationSpeed * Time.deltaTime, 0f);
    //     }
    // }
}