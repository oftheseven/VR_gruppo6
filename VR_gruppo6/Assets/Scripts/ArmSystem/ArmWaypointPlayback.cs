using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ArmWaypointPlayback : MonoBehaviour
{
    private static ArmWaypointPlayback _instance;
    public static ArmWaypointPlayback instance => _instance;
    
    private InteractableArm currentArm;
    private bool isPlayingBack = false;
    private float playbackProgress = 0f;
    
    public bool IsPlayingBack => isPlayingBack;
    public float PlaybackProgress => playbackProgress;
    
    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this);
        }
        else
        {
            _instance = this;
        }
        // this.gameObject.SetActive(false);
    }
    
    public void StartPlayback(InteractableArm arm)
    {
        if (arm.WaypointCount < 2)
        {
            Debug.LogError("Servono almeno 2 waypoint per playback!");
            return;
        }
        
        currentArm = arm;
        StartCoroutine(PlaybackCoroutine());
    }
    
    public void StopPlayback()
    {
        isPlayingBack = false;
        StopAllCoroutines();
        
        // Debug.Log("Playback fermato");
    }
    
    private IEnumerator PlaybackCoroutine()
    {
        isPlayingBack = true;
        playbackProgress = 0f;
        
        List<ArmWaypoint> waypoints = currentArm.RecordedWaypoints;
        float speed = currentArm.PlaybackSpeed;
        
        // Debug.Log($"Playback iniziato: {waypoints.Count} waypoint, velocità {speed}");
        
        float[] cumulativeDistances = new float[waypoints.Count];
        cumulativeDistances[0] = 0f;
        
        for (int i = 1; i < waypoints.Count; i++)
        {
            float segmentDistance = waypoints[i - 1].DistanceTo(waypoints[i]);
            cumulativeDistances[i] = cumulativeDistances[i - 1] + segmentDistance;
        }
        
        float totalDistance = cumulativeDistances[waypoints.Count - 1];
        float duration = totalDistance / speed;
        
        // Debug.Log($"Distanza totale: {totalDistance:F2}, Durata: {duration:F2}s");
        
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float currentDistance = (elapsedTime / duration) * totalDistance;
            playbackProgress = elapsedTime / duration;
            
            int segmentIndex = 0;
            for (int i = 1; i < cumulativeDistances.Length; i++)
            {
                if (currentDistance <= cumulativeDistances[i])
                {
                    segmentIndex = i - 1;
                    break;
                }
            }
            
            if (segmentIndex >= waypoints.Count - 1)
            {
                segmentIndex = waypoints.Count - 2;
            }
            
            float segmentStartDist = cumulativeDistances[segmentIndex];
            float segmentEndDist = cumulativeDistances[segmentIndex + 1];
            float segmentLength = segmentEndDist - segmentStartDist;
            float t = segmentLength > 0 ? (currentDistance - segmentStartDist) / segmentLength : 0f;
            t = Mathf.Clamp01(t);
        
            Vector3 interpolatedPos = Vector3.Lerp(
                waypoints[segmentIndex].position,
                waypoints[segmentIndex + 1].position,
                t
            );
            
            Quaternion interpolatedRot = Quaternion.Slerp(
                waypoints[segmentIndex].rotation,
                waypoints[segmentIndex + 1].rotation,
                t
            );
            
            ApplyArmTransform(interpolatedPos, interpolatedRot, waypoints[segmentIndex], waypoints[segmentIndex + 1], t);
            
            yield return null;
        }
        
        ArmWaypoint lastWaypoint = waypoints[waypoints.Count - 1];
        ApplyArmTransform(lastWaypoint.position, lastWaypoint.rotation, lastWaypoint, lastWaypoint, 1f);
        
        isPlayingBack = false;
        playbackProgress = 1f;
        
        // Debug.Log("Playback completato!");
        
        // AGGIUNGERE CHIAMATA AL MANAGER PER NOTIFICARE CHE LA QUEST E' STATA COMPLETATA
    }
    
    private void ApplyArmTransform(Vector3 position, Quaternion rotation, ArmWaypoint from, ArmWaypoint to, float t)
    {
        Transform pivotBase = currentArm.PivotBase;
        Transform pivotJoint = currentArm.PivotJoint;
        
        if (from.jointRotations != null && to.jointRotations != null)
        {
            if (pivotBase != null && from.jointRotations.Length > 0 && to.jointRotations.Length > 0)
            {
                Quaternion interpolatedBase = Quaternion.Slerp(
                    from.jointRotations[0],
                    to.jointRotations[0],
                    t
                );
                
                pivotBase.localRotation = interpolatedBase;
            }
            
            if (pivotJoint != null && from.jointRotations.Length > 1 && to.jointRotations.Length > 1)
            {
                Quaternion interpolatedJoint = Quaternion.Slerp(
                    from.jointRotations[1],
                    to.jointRotations[1],
                    t
                );
                
                pivotJoint.localRotation = interpolatedJoint;
                currentArm.ApplyJointLimits();
            }
        }
        else
        {
            Transform armEnd = currentArm.ArmEnd;
            if (armEnd != null)
            {
                armEnd.position = position;
                armEnd.rotation = rotation;
                
                // Debug.LogWarning("Playback fallback: muovendo armEnd direttamente");
            }
        }
    }
}