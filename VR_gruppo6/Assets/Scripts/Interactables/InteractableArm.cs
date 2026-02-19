using UnityEngine;
using System.Collections.Generic;

public class InteractableArm : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private string interactionText = "[E] per gestire il braccio";

    [Header("Arm Components")]
    [SerializeField] private Transform armTip;
    [SerializeField] private Transform armEnd;
    [SerializeField] private Camera directorModeCamera;
    
    [Header("Waypoint Settings")]
    [SerializeField] private float playbackSpeed = 1.0f;
    [SerializeField] private int maxWaypoints = 20;

    [Header("Divination Target Waypoints")]
    [SerializeField] private Transform[] divinationTargetWaypoints;
    private bool accuracyQuestCompleted = false;

    public Transform[] DivinationTargetWaypoints => divinationTargetWaypoints;

    [Header("Pivot Controls")]
    [SerializeField] private Transform pivotBase;
    [SerializeField] private Transform pivotJoint;
    [SerializeField] private float jointMinAngle = -20f;
    [SerializeField] private float jointMaxAngle = 20f;
    
    [Header("UI References")]
    [SerializeField] private UI_ArmPanel armPanel;

    [Header("Visual Feedback")]
    [SerializeField] private ArmVisualFeedback visualFeedback;

    [Header("Audio")]
    [SerializeField] private AudioClip armSFX;
    [SerializeField] [Range(0f, 2f)] private float motorVolume = 0.5f;
    [SerializeField] private float movementStopDelay = 0.3f;
    
    private AudioSource audioSource;
    private bool isMotorRunning = false;
    private float lastMovementTime = 0f;
    private bool isPlayingBack = false;

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

    private bool tutorialQuestCompleted = false;

    void Start()
    {
        if (armEnd == null)
        {
            armEnd = transform;
        }

        if (pivotBase == null)
        {
            Debug.LogWarning("PivotBase non assegnato! Usa il transform principale.");
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
        SetupAudio();
    }

    void Update()
    {
        UpdateAudio();
    }

    void LateUpdate()
    {
        StabilizeArmTip();
    }

    private void StabilizeArmTip()
    {
        if (armTip == null) return;
        
        float baseYRotation = 0f;
        
        if (pivotBase != null)
        {
            baseYRotation = pivotBase.eulerAngles.y;
        }
        
        armTip.rotation = Quaternion.Euler(0f, baseYRotation, 0f);
    }

    public void RotateBase(float delta)
    {
        if (pivotBase == null) return;
        
        // rotazione orizzontale
        pivotBase.Rotate(Vector3.up, delta, Space.Self);

        OnArmMoved();
    }
    
    public void RotateJoint(float delta)
    {
        if (pivotJoint == null) return;
        
        // rotazione verticale
        pivotJoint.Rotate(Vector3.right, delta, Space.Self);
        ClampJointRotation();

        OnArmMoved();
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
    }
    
    public void AddWaypoint()
    {
        if (!isRecording)
        {
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
            debugInfo += $" Pos={waypoint.position}s";
            
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

        CheckTutorialCompletion();
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
        CheckDivinationAccuracy();
    }

    private void CheckTutorialCompletion()
    {
        // tutorial: l'utente deve aver creato almeno 1 waypoint
        if (QuestManager.instance != null && 
            QuestManager.instance.IsQuestActive(QuestManager.MainQuest.TutorialArm) &&
            !tutorialQuestCompleted)
        {
            if (recordedWaypoints.Count >= 1)
            {
                tutorialQuestCompleted = true;
                QuestManager.instance.CompleteCurrentQuest();
            }
        }
    }

    private void CheckDivinationAccuracy()
    {
        if (QuestManager.instance == null
            || !QuestManager.instance.IsQuestActive(QuestManager.MainQuest.DivinationArm)
            || accuracyQuestCompleted)
            return;

        if (recordedWaypoints.Count != divinationTargetWaypoints.Length)
        {
            Debug.Log("Numero di waypoint utente diverso dai target!");
            return;
        }

        float maxDistance = 0f;
        float sumDistance = 0f;
        for (int i=0; i < divinationTargetWaypoints.Length; i++)
        {
            float dist = Vector3.Distance(
                recordedWaypoints[i].position,
                divinationTargetWaypoints[i].position
            );
            sumDistance += dist;
            if (dist > maxDistance) maxDistance = dist;
        }
        float averageDistance = sumDistance / divinationTargetWaypoints.Length;
    
        if (averageDistance <= QuestManager.instance.ArmAccuracy)
        {
            accuracyQuestCompleted = true;
            Debug.Log($"Quest divination COMPLETATA (accuratezza: {averageDistance:F2})");
            QuestManager.instance.CompleteCurrentQuest();
        }
        else
        {
            Debug.Log($"Divination: accuratezza insufficiente: avg={averageDistance:F2}, max={maxDistance:F2}, soglia={QuestManager.instance.ArmAccuracy:F2}");
            QuestManager.instance.ShowMessage("Accuratezza insufficiente, riprova!");
        }
    }
    
    public void ClearWaypoints()
    {
        recordedWaypoints.Clear();
        isRecording = false;
        tutorialQuestCompleted = false;

        if (visualFeedback != null)
        {
            visualFeedback.ClearAllWaypointMarkers();
        }
        
        Debug.Log("Waypoint cancellati");
    }
    
    public void Interact()
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayInteractionSFX();
        }
        
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

    private void SetupAudio()
    {
        if (armSFX != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = armSFX;
            audioSource.playOnAwake = false;
            audioSource.loop = true;
            audioSource.spatialBlend = 1f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.minDistance = 1f;
            audioSource.maxDistance = 10f;
            audioSource.dopplerLevel = 0f;
            audioSource.volume = motorVolume;

            if (AudioManager.instance != null && AudioManager.instance.AudioMixer != null)
            {
                audioSource.outputAudioMixerGroup = AudioManager.instance.AudioMixer.FindMatchingGroups("SFX")[0];
                Debug.Log($"{gameObject.name} audio assegnato a: {audioSource.outputAudioMixerGroup.name}");
            }
        }
    }

    private void OnArmMoved()
    {
        lastMovementTime = Time.time;
        
        if (!isMotorRunning)
        {
            StartMotor();
        }
    }

    private void UpdateAudio()
    {
        if (audioSource == null) return;
        
        bool hasRecentMovement = (Time.time - lastMovementTime) < movementStopDelay;
        
        bool isCurrentlyMoving = hasRecentMovement || isPlayingBack;
        
        if (isCurrentlyMoving && !isMotorRunning)
        {
            StartMotor();
        }
        else if (!isCurrentlyMoving && isMotorRunning)
        {
            StopMotor();
        }
    }

    private void StartMotor()
    {
        if (audioSource == null || isMotorRunning) return;
        
        audioSource.Play();
        isMotorRunning = true;
    }
    private void StopMotor()
    {
        if (audioSource == null || !isMotorRunning) return;
        
        audioSource.Stop();
        isMotorRunning = false;
    }

    public void OnPlaybackStart()
    {
        isPlayingBack = true;
        StartMotor();
    }
    public void OnPlaybackMoving()
    {
        OnArmMoved();
    }
    public void OnPlaybackStop()
    {
        isPlayingBack = false;
    }
}