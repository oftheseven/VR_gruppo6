using UnityEngine;
using UnityEngine.InputSystem;

public class ArmCameraOrbit : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform orbitTarget; // base del braccio
    
    [Header("Orbit settings")]
    [SerializeField] private float orbitSpeed = 100f;
    [SerializeField] private float distance = 5f;
    [SerializeField] private float height = 3f;
    [SerializeField] private float inclinationAngle = 30f;
    
    private float currentYaw = 0f;
    private bool isActive = false;
    private Camera cam;
    
    void Start()
    {
        cam = GetComponent<Camera>();
        cam.enabled = false; // disabilito la camera all'inizio

        if (orbitTarget == null)
        {
            ArmVisualFeedback feedback = FindFirstObjectByType<ArmVisualFeedback>();
            if (feedback != null)
            {
                InteractableArm arm = feedback.GetComponent<InteractableArm>();
                if (arm != null && arm.PivotBase != null)
                {
                    orbitTarget = arm.PivotBase;
                }
            }
        }
        
        if (orbitTarget == null)
        {
            Debug.LogWarning("ArmCameraOrbit: nessun orbit target assegnato!");
        }
    }
    
    void LateUpdate()
    {
        if (!isActive || orbitTarget == null) return;
        
        HandleInput();
        UpdateCameraPosition();
    }
    
    private void HandleInput()
    {
        if (Keyboard.current == null) return;
        
        if (Keyboard.current.aKey.isPressed)
        {
            currentYaw -= orbitSpeed * Time.deltaTime;
        }
        
        if (Keyboard.current.dKey.isPressed)
        {
            currentYaw += orbitSpeed * Time.deltaTime;
        }
    }
    
    private void UpdateCameraPosition()
    {
        if (orbitTarget == null) return;
        
        float radians = currentYaw * Mathf.Deg2Rad;
        float x = Mathf.Sin(radians) * distance;
        float z = Mathf.Cos(radians) * distance;
        
        Vector3 targetPos = orbitTarget.position;
        transform.position = new Vector3(targetPos.x + x, targetPos.y + height, targetPos.z + z);
        
        // guardo sempre verso il target
        transform.LookAt(orbitTarget.position + Vector3.up * height);
        this.transform.rotation *= Quaternion.Euler(inclinationAngle, 0, 0);
    }
    
    public void EnableOrbit()
    {
        isActive = true;
        
        if (orbitTarget != null)
        {
            Vector3 directionToCamera = transform.position - orbitTarget.position;
            directionToCamera.y = 0;
            
            if (directionToCamera.magnitude > 0.1f)
            {
                currentYaw = Mathf.Atan2(directionToCamera.x, directionToCamera.z) * Mathf.Rad2Deg;
            }
        }
        
        // Debug.Log("Camera orbit abilitata");
    }
    
    public void DisableOrbit()
    {
        isActive = false;
        // Debug.Log("Camera orbit disabilitata");
    }
    
    public void SetOrbitTarget(Transform target)
    {
        orbitTarget = target;
    }
    
    public void ResetToDefaultPosition()
    {
        currentYaw = 0f;
        
        if (isActive)
        {
            UpdateCameraPosition();
        }
    }
}