using UnityEngine;

public class Waypoint : MonoBehaviour
{
    [Header("Waypoint settings")]
    [SerializeField] private float reachDistance = 0.15f;
    [Header("Visual feedback")]
    [SerializeField] private Color activeColor = Color.green;
    [SerializeField] private Color reachedColor = Color.blue;
    [SerializeField] private Color hiddenColor = new Color(0, 0, 0, 0);

    private int waypointIndex;
    private bool isReached = false;
    private Renderer waypointRenderer;
    private bool isActiveWaypoint = false;

    public float ReachDistance => reachDistance;
    public int WaypointIndex => waypointIndex;
    public bool IsReached => isReached;

    void Awake()
    {
        waypointRenderer = GetComponent<Renderer>();
        Hide();
    }

    public void Initialize(int index)
    {
        waypointIndex = index;
        isReached = false;
        isActiveWaypoint = false;

        Hide();
    }

    public void SetActive()
    {
        isActiveWaypoint = true;

        if (waypointRenderer != null)
        {
            waypointRenderer.enabled = true;
            waypointRenderer.material.color = activeColor;
        }

        // Debug.Log($"🎯 Waypoint {waypointIndex} attivato");
    }

    public void MarkAsReached()
    {
        isReached = true;
        isActiveWaypoint = false;

        if (waypointRenderer != null)
        {
            waypointRenderer.material.color = reachedColor;
        }

        // Debug.Log($"✅ Waypoint {waypointIndex} completato");
    }

    public void Hide()
    {
        isActiveWaypoint = false;

        if (waypointRenderer != null)
        {
            waypointRenderer.enabled = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ArmTip") && isActiveWaypoint && !isReached)
        {
            // Debug.Log($"🎯 ArmTip ha toccato Waypoint {waypointIndex}");
            
            if (WaypointManager.instance != null)
            {
                WaypointManager.instance.OnWayPointReached(this);
            }
        }
    }
}