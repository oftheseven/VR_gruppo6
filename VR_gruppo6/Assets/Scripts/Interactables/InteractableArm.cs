using UnityEngine;
using System.Collections.Generic;

public class InteractableArm : MonoBehaviour
{
    [Header("Arm Components")]
    [SerializeField] private Transform armEnd;
    [SerializeField] private Camera directorModeCamera; // camera da usare in director mode
    
    [Header("Waypoint Settings")]
    [SerializeField] private float playbackSpeed = 1.0f;
    [SerializeField] private int maxWaypoints = 20;

    [Header("Pivot Controls")]
    [SerializeField] private Transform pivotBase;
    [SerializeField] private Transform pivotJoint;
    [SerializeField] private float jointMinAngle = -20f;
    [SerializeField] private float jointMaxAngle = 20f;
    
    [Header("UI References")]
    [SerializeField] private UI_ArmPanel armPanel;

    [Header("Visual Feedback")]
    [SerializeField] private ArmVisualFeedback visualFeedback;
    
    [Header("Interaction")]
    [SerializeField] private string interactionText = "Premi E per gestire il braccio";
    
    private List<ArmWaypoint> recordedWaypoints = new List<ArmWaypoint>();
    private bool isRecording = false;
    private float recordingStartTime = 0f;
    
    public List<ArmWaypoint> RecordedWaypoints => recordedWaypoints;
    public int WaypointCount => recordedWaypoints.Count;
    public bool IsRecording => isRecording;
    public float PlaybackSpeed => playbackSpeed;
    public Transform ArmEnd => armEnd;
    public Transform PivotBase => pivotBase;
    public Transform PivotJoint => pivotJoint;
    public float JointMinAngle => jointMinAngle;
    public float JointMaxAngle => jointMaxAngle;
    public Camera DirectorModeCamera => directorModeCamera;
    public ArmVisualFeedback VisualFeedback => visualFeedback;

    void Start()
    {
        if (armEnd == null)
        {
            armEnd = transform;
        }

        if (pivotBase == null)
        {
            pivotBase = transform;
        }
        
        if (pivotJoint == null)
        {
            Debug.LogWarning("PivotJoint non assegnato! Sistema a 1 pivot.");
        }

        if (directorModeCamera != null)
        {
            directorModeCamera.enabled = false;
        }

        if (visualFeedback == null)
        {
            visualFeedback = GetComponent<ArmVisualFeedback>();
        }

        ClampJointRotation();
    }

    void LateUpdate()
    {
        StabilizeArmTip();
    }

    private void StabilizeArmTip()
    {
        if (armEnd == null) return;
        
        float baseYRotation = 0f;
        
        if (pivotBase != null)
        {
            baseYRotation = pivotBase.eulerAngles.y;
        }
        
        armEnd.rotation = Quaternion.Euler(0f, baseYRotation, 0f);
    }

    public void RotateBase(float delta)
    {
        if (pivotBase == null) return;
        
        // rotazione orizzontale
        pivotBase.Rotate(Vector3.up, delta, Space.Self);
    }
    
    public void RotateJoint(float delta)
    {
        if (pivotJoint == null) return;
        
        // rotazione verticale
        pivotJoint.Rotate(Vector3.right, delta, Space.Self);
        ClampJointRotation();
    }

    private void ClampJointRotation()
    {
        if (pivotJoint == null) return;
        
        Vector3 currentRotation = pivotJoint.localEulerAngles;
        
        float angleX = currentRotation.x;
        if (angleX > 180f)
            angleX -= 360f;
        
        float clampedX = Mathf.Clamp(angleX, jointMinAngle, jointMaxAngle);
        
        if (Mathf.Abs(angleX - clampedX) > 0.01f)
        {
            currentRotation.x = clampedX;
            pivotJoint.localEulerAngles = currentRotation;
        }
    }

    public void ApplyJointLimits()
    {
        ClampJointRotation();
    }
    
    public void StartRecording()
    {
        recordedWaypoints.Clear();
        isRecording = true;
        recordingStartTime = Time.time;
        
        Debug.Log("Arm recording iniziato");
    }
    
    public void AddWaypoint()
    {
        if (!isRecording)
        {
            Debug.LogWarning("Recording non attivo!");
            return;
        }
        
        if (recordedWaypoints.Count >= maxWaypoints)
        {
            Debug.LogWarning($"Massimo waypoint raggiunto ({maxWaypoints})!");
            return;
        }
        
        float currentTime = Time.time - recordingStartTime;
        
        List<Quaternion> jointRotations = new List<Quaternion>();
        
        if (pivotBase != null)
        {
            jointRotations.Add(pivotBase.localRotation);
        }
        
        if (pivotJoint != null)
        {
            jointRotations.Add(pivotJoint.localRotation);
        }
        
        if (jointRotations.Count > 0)
        {
            ArmWaypoint waypoint = new ArmWaypoint(armEnd, currentTime, jointRotations.ToArray());
            recordedWaypoints.Add(waypoint);
            
            string debugInfo = $"Waypoint {recordedWaypoints.Count} aggiunto:";
            if (pivotBase != null)
                debugInfo += $"\n   Base Y: {pivotBase.localEulerAngles.y:F1}°";
            if (pivotJoint != null)
                debugInfo += $"\n   Joint X: {pivotJoint.localEulerAngles.x:F1}°";
            
            Debug.Log(debugInfo);
        }
        else
        {
            ArmWaypoint waypoint = new ArmWaypoint(armEnd, currentTime);
            recordedWaypoints.Add(waypoint);
            
            Debug.Log($"Waypoint {recordedWaypoints.Count} aggiunto @ {armEnd.position}");
        }

        if (visualFeedback != null)
        {
            visualFeedback.CreateWaypointMarker(recordedWaypoints.Count);
        }
    }
    
    public void StopRecording()
    {
        if (!isRecording)
        {
            Debug.LogWarning("Recording già fermato!");
            return;
        }
        
        isRecording = false;
        
        Debug.Log($"Recording completato! {recordedWaypoints.Count} waypoint salvati");
    }
    
    public void ClearWaypoints()
    {
        recordedWaypoints.Clear();
        isRecording = false;

        if (visualFeedback != null)
        {
            visualFeedback.ClearAllWaypointMarkers();
        }
        
        Debug.Log("Waypoint cancellati");
    }
    
    public void Interact()
    {
        if (armPanel != null)
        {
            armPanel.OpenArmPanel();
        }
    }
    
    public string GetInteractionText()
    {
        return interactionText;
    }
    
    public UI_ArmPanel GetArmPanel()
    {
        return armPanel;
    }
    
    public float GetPlaybackDuration()
    {
        if (recordedWaypoints.Count < 2)
            return 0f;
        
        float totalDistance = 0f;
        
        for (int i = 0; i < recordedWaypoints.Count - 1; i++)
        {
            totalDistance += recordedWaypoints[i].DistanceTo(recordedWaypoints[i + 1]);
        }
        
        return totalDistance / playbackSpeed;
    }
}