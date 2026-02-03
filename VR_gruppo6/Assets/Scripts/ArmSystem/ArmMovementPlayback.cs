using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ArmMovementPlayback : MonoBehaviour
{
    // singleton
    private static ArmMovementPlayback _instance;
    public static ArmMovementPlayback instance => _instance;

    [Header("Playback settings")]
    [SerializeField] private float playbackSpeed = 1f;

    private List<ArmSnapshot> snapshots;
    private bool isPlayingBack = false;
    private int currentSnapshotIndex = 0;

    private Transform basePivot;
    private Transform pivot1;
    private Transform pivot2;

    public bool IsPlayingBack => isPlayingBack;
    public float PlaybackProgress => snapshots != null && snapshots.Count > 0 
        ? (float)currentSnapshotIndex / snapshots.Count 
        : 0f;

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

    public void StartPlayback(List<ArmSnapshot> recordedSnapshots)
    {
        if (recordedSnapshots == null || recordedSnapshots.Count == 0)
        {
            Debug.LogWarning("Nessun snapshot registrato per la riproduzione.");
            return;
        }

        if (InteractableArm.instance != null)
        {
            basePivot = InteractableArm.instance.MechanicalArmPivot?.transform;
            pivot1 = InteractableArm.instance.Pivot1?.transform;
            pivot2 = InteractableArm.instance.Pivot2?.transform;
        }

        if (basePivot == null || pivot1 == null || pivot2 == null)
        {
            Debug.LogError("❌ Reference mancanti per playback!");
            return;
        }

        snapshots = new List<ArmSnapshot>(recordedSnapshots);
        currentSnapshotIndex = 0;

        PlayerController.instance.playerCamera.gameObject.SetActive(false);
        UI_ArmPanel.instance.ArmCamera.gameObject.SetActive(true);
        UI_AccuracyFeedback.instance.gameObject.SetActive(false);
        StartCoroutine(PlaybackCoroutine());
    }

    private IEnumerator PlaybackCoroutine()
    {
        isPlayingBack = true;

        if (UI_AccuracyFeedback.instance != null)
        {
            UI_AccuracyFeedback.instance.OnPlaybackStarted();
        }

        float playbackStartTime = Time.time;

        for (int i = 0; i < snapshots.Count; i++)
        {
            currentSnapshotIndex = i;
            ArmSnapshot snapshot = snapshots[i];
            
            if (i < snapshots.Count - 1)
            {
                ArmSnapshot nextSnapshot = snapshots[i + 1];
                float duration = (nextSnapshot.timestamp - snapshot.timestamp) / playbackSpeed;
                
                yield return StartCoroutine(InterpolateToSnapshot(snapshot, nextSnapshot, duration));
            }
            else
            {
                ApplySnapshot(snapshot);
                
                if (i < snapshots.Count - 1)
                {
                    float waitTime = (snapshots[i + 1].timestamp - snapshot.timestamp) / playbackSpeed;
                    yield return new WaitForSeconds(waitTime);
                }
            }
        }

        isPlayingBack = false;

        if (UI_AccuracyFeedback.instance != null)
        {
            UI_AccuracyFeedback.instance.OnPlaybackFinished();
            PlayerController.instance.playerCamera.gameObject.SetActive(true);
            UI_ArmPanel.instance.ArmCamera.gameObject.SetActive(false);
            UI_AccuracyFeedback.instance.gameObject.SetActive(true);
        }
    }

    private IEnumerator InterpolateToSnapshot(ArmSnapshot from, ArmSnapshot to, float duration)
    {
        float elapsed = 0f;
        
        Quaternion startBase = basePivot.localRotation;
        Quaternion startP1 = pivot1.localRotation;
        Quaternion startP2 = pivot2.localRotation;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            
            basePivot.localRotation = Quaternion.Slerp(startBase, to.baseRotation, t);
            pivot1.localRotation = Quaternion.Slerp(startP1, to.pivot1Rotation, t);
            pivot2.localRotation = Quaternion.Slerp(startP2, to.pivot2Rotation, t);
            
            yield return null;
        }
        
        ApplySnapshot(to);
    }
    
    private void ApplySnapshot(ArmSnapshot snapshot)
    {
        basePivot.localRotation = snapshot.baseRotation;
        pivot1.localRotation = snapshot.pivot1Rotation;
        pivot2.localRotation = snapshot.pivot2Rotation;
    }

    public void StopPlayback()
    {
        if (isPlayingBack)
        {
            StopAllCoroutines();
            isPlayingBack = false;
            Debug.Log("⏸️ Playback fermato");
        }
        PlayerController.instance.playerCamera.gameObject.SetActive(true);
        UI_ArmPanel.instance.ArmCamera.gameObject.SetActive(false);
        UI_AccuracyFeedback.instance.gameObject.SetActive(true);
    }

    public void SetPlaybackSpeed(float speed)
    {
        playbackSpeed = Mathf.Clamp(speed, 0.1f, 3f);
    }
}