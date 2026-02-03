using UnityEngine;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;

public class ArmMovementRecorder : MonoBehaviour
{
    // singleton
    private static ArmMovementRecorder _instance;
    public static ArmMovementRecorder instance => _instance;

    [Header("Recording settings")]
    [SerializeField] private float recordInterval = 0.1f;

    private List<ArmSnapshot> snapshots = new List<ArmSnapshot>();
    private bool isRecording = false;
    private float recordTimer = 0f;
    private float sessionStartTime = 0f;

    // reference ai transform dei pivot e della punta del braccio
    private Transform basePivot;
    private Transform pivot1;
    private Transform pivot2;
    private Transform armTip;

    public List<ArmSnapshot> RecordedSnapshots => snapshots;
    public bool IsRecording => isRecording;
    public int SnapshotCount => snapshots.Count;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (isRecording)
        {
            recordTimer += Time.deltaTime;
            if (recordTimer >= recordInterval)
            {
                RecordSnapshot();
                recordTimer = 0f;
            }
        }
    }

    public void StartRecording()
    {
        if (InteractableArm.instance != null)
        {
            basePivot = InteractableArm.instance.MechanicalArmPivot?.transform;
            pivot1 = InteractableArm.instance.Pivot1?.transform;
            pivot2 = InteractableArm.instance.Pivot2?.transform;
        }

        if (ArmAccuracyTracker.instance != null)
        {
            armTip = ArmAccuracyTracker.instance.ArmTip;
        }

        if (basePivot == null || pivot1 == null || pivot2 == null || armTip == null)
        {
            Debug.LogError("❌ Impossibile avviare recording: reference mancanti!");
            return;
        }

        snapshots.Clear();
        isRecording = true;
        sessionStartTime = Time.time;
        recordTimer = 0f;

        RecordSnapshot();
        
        Debug.Log("🎬 Recording movimento iniziato");
    }

    public void StopRecording()
    {
        if (!isRecording) return;
        
        isRecording = false;
        
        RecordSnapshot();
        
        Debug.Log($"⏹️ Recording fermato - {snapshots.Count} snapshot salvati");
    }

    private void RecordSnapshot()
    {
        if (basePivot == null || pivot1 == null || pivot2 == null || armTip == null)
        {
            return;
        }

        float currentTime = Time.time - sessionStartTime;

        ArmSnapshot snapshot = new ArmSnapshot
        (
            currentTime,
            basePivot.localRotation,
            pivot1.localRotation,
            pivot2.localRotation,
            armTip.position
        );

        snapshots.Add(snapshot);
    }

    public void ClearRecording()
    {
        snapshots.Clear();
    }
}