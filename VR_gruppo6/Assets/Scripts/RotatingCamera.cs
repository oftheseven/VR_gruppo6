using UnityEngine;
using UnityEngine.InputSystem;

public class RotatingCamera : MonoBehaviour
{
    [Header("Rotation settings")]
    [SerializeField] private GameObject cameraReference;

    [SerializeField] private float rotationSpeed = 10f;

    void Awake()
    {
        if (cameraReference == null)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null)
                cameraReference = cam.gameObject;
            else
                Debug.LogWarning($"{name}: cameraReference non assegnato e nessuna Camera trovata nei figli!");
        }
    }

    void Update()
    {
        Rotate();
    }

    private void Rotate()
    {
        if (cameraReference == null) return;

        float y = cameraReference.transform.localRotation.eulerAngles.y;
        if (Keyboard.current.dKey.isPressed)
        {
            cameraReference.transform.localRotation = Quaternion.Euler(0f, y + rotationSpeed * Time.deltaTime, 0f);
        } 
        else if (Keyboard.current.aKey.isPressed)
        {            
            cameraReference.transform.localRotation = Quaternion.Euler(0f, y - rotationSpeed * Time.deltaTime, 0f);
        }
    }
}