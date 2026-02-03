using UnityEngine;
using System.Collections.Generic;

// struct per i risultati di accuratezza
public struct AccuracyResults
{
    public int waypointsReached;
    public int totalWaypoints;
    public int perfectHits;
    public float finalScore;
}

public class WaypointManager : MonoBehaviour
{
    // singleton
    private static WaypointManager _instance;
    public static WaypointManager instance => _instance;

    [Header("Waypoint setup")]
    [SerializeField] private Transform waypointsParent; // parent con 5 empty gameobject come waypoints (solo posizioni nello spazio)
    [SerializeField] private GameObject waypointPrefab; // prefab del waypoint da instanziare

    [Header("Scoring")]
    [SerializeField] private float perfectDistanceThreshold = 0.2f; // distanza perfetta per punteggio massimo
    [SerializeField] private float goodDistanceThreshold = 0.5f; // distanza buona per punteggio medio

    private List<Waypoint> waypoints = new List<Waypoint>();
    private int currentWaypointIndex = 0;
    private int waypointsReached = 0;
    private bool isActive = false;

    // statistiche per lo scoring
    private float totalDistanceFromWaypoints = 0f;
    private int perfectHits = 0;

    public bool IsActive => isActive;
    public int WaypointsReached => waypointsReached;
    public int TotalWaypoints => waypoints.Count;

    void Awake()
    {
        if (instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void StartWaypointChallenge()
    {
        if (isActive) 
        {
            Debug.LogWarning("Il Waypoint Challenge è già attivo.");
            return;
        }

        SpawnWaypoints();
        isActive = true;
        currentWaypointIndex = 0;
        waypointsReached = 0;
        totalDistanceFromWaypoints = 0f;
        perfectHits = 0;

        if (waypoints.Count > 0)
        {
            waypoints[0].SetActive();
        }

        Debug.Log("Waypoint Challenge iniziato.");
    }

    public void StopWaypointChallenge()
    {
        isActive = false;
        Debug.Log("Waypoint Challenge terminato.");
    }

    private void SpawnWaypoints()
    {
        ClearWaypoints();

        if (waypointsParent == null || waypointPrefab == null)
        {
            return;
        }

        for (int i = 0; i < waypointsParent.childCount; i++)
        {
            Transform spawnPoint = waypointsParent.GetChild(i);
            GameObject waypointObj = Instantiate(waypointPrefab, spawnPoint.position, Quaternion.identity);
            waypointObj.name = $"Waypoint_{i + 1}";
            Waypoint waypoint = waypointObj.GetComponent<Waypoint>();
            if (waypoint != null)
            {
                waypoint.gameObject.SetActive(true);
                waypoints.Add(waypoint);
            }
        }
    }

    public void OnWayPointReached(Waypoint waypoint)
    {
        if (!isActive) return;

        if (waypoint.WaypointIndex != currentWaypointIndex + 1)
        {
            Debug.LogWarning("Waypoint raggiunto fuori ordine.");
            return;
        }

        float distance = 0f;

        if (distance <= perfectDistanceThreshold)
        {
            perfectHits++;
        }

        totalDistanceFromWaypoints += distance;

        // marco il waypoint come raggiunto
        waypointsReached++;
        waypoint.MarkAsReached();
        currentWaypointIndex++;

        Debug.Log($"✅ Waypoint {waypoint.WaypointIndex} raggiunto! ({waypointsReached}/{waypoints.Count})");

        if (currentWaypointIndex < waypoints.Count)
        {
            waypoints[currentWaypointIndex].SetActive();
        }
        else
        {
            Debug.Log("🎉 Tutti i waypoint raggiunti!");
        }
    }

    public AccuracyResults CalculateFinalScore()
    {
        AccuracyResults results = new AccuracyResults();
        
        results.waypointsReached = waypointsReached;
        results.totalWaypoints = waypoints.Count;
        results.perfectHits = perfectHits;

        float waypointScore = ((float) waypointsReached / waypoints.Count) * 100f;

        float orderBonus = (waypointsReached == waypoints.Count) ? 20f : 0f;

        float precisionBonus = ((float) perfectHits / waypoints.Count) * 10f;

        results.finalScore = Mathf.Clamp(waypointScore + orderBonus + precisionBonus, 0f, 100f);

        Debug.Log($"Punteggio finale calcolato: {results.finalScore}");

        return results;
    }

    private void ClearWaypoints()
    {
        foreach (Waypoint wp in waypoints)
        {
            if (wp != null)
            {
                Destroy(wp.gameObject);
            }
        }
        waypoints.Clear();
    }

    void OnDestroy()
    {
        ClearWaypoints();
    }
}