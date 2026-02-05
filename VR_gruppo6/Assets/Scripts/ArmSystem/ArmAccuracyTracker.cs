using UnityEngine;

public class ArmAccuracyTracker : MonoBehaviour
{
    // singleton
    private static ArmAccuracyTracker _instance;
    public static ArmAccuracyTracker instance => _instance;

    [Header("Tracking settings")]
    [SerializeField] private Transform armTip; // reference alla punta del braccio meccanico
    [SerializeField] private float trackingInterval = 0.1f; // intervallo di tempo per il tracking della posizione

    private bool isTracking = false;
    private float trackingTimer = 0f;
    private float sessionStartTime = 0f;

    public Transform ArmTip => armTip;
    public float SessionDuration => isTracking ? (Time.time - sessionStartTime) : 0f;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(transform.root.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    void Update()
    {
        if (isTracking)
        {
            trackingTimer += Time.deltaTime;
            if (trackingTimer >= trackingInterval)
            {
                trackingTimer = 0f;
            }
        }
    }

    public void StartTracking()
    {
        if (armTip == null)
        {
            // Debug.LogError("❌ Punta del braccio non assegnata!");
            return;
        }
        
        isTracking = true;
        sessionStartTime = Time.time;
        trackingTimer = 0f;
        
        // Debug.Log("🎬 Tracking iniziato");
    }

    public void StopTracking()
    {
        isTracking = false;
        // Debug.Log($"⏹️ Tracking fermato (durata: {SessionDuration:F1}s)");

        if (ArmMovementRecorder.instance != null)
        {
            ArmMovementRecorder.instance.StopRecording();
        }
    }

    public float GetDistanceToWaypoint(Vector3 waypointPosition)
    {
        if (armTip == null) return float.MaxValue;
        
        return Vector3.Distance(armTip.position, waypointPosition);
    }
}