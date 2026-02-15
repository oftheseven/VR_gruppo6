using UnityEngine;
using System.Collections.Generic;

public class ArmVisualFeedback : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InteractableArm arm;
    [SerializeField] private Camera armDirectorCamera;
    
    [Header("Laser settings")]
    [SerializeField] private bool showLaser = true;
    private LineRenderer laserLine;
    [SerializeField] private Color laserColor = new Color(1f, 0f, 0f, 0.5f);
    [SerializeField] private float laserWidth = 0.05f;
    [SerializeField] private float maxLaserDistance = 50f;
    
    [Header("Ground circle settings")]
    [SerializeField] private bool showGroundCircle = true;
    private GameObject groundCirclePrefab;
    [SerializeField] private Color circleColor = new Color(1f, 1f, 0f, 0.5f);
    [SerializeField] private float circleSize = 2f;
    
    [Header("Waypoint marker settings")]
    private GameObject waypointMarkerPrefab;
    [SerializeField] private Color waypointColor = new Color(0f, 1f, 0f, 0.8f);
    [SerializeField] private float waypointHeight = 2f;
    
    [Header("Ground detection")]
    [SerializeField] private LayerMask groundLayer = ~0;
    
    private GameObject groundCircleInstance;
    private List<GameObject> waypointMarkers = new List<GameObject>();
    private bool isActive = false;

    void Start()
    {
        if (laserLine == null)
        {
            laserLine = gameObject.AddComponent<LineRenderer>();
        }
        
        SetupLaser();
        
        if (showGroundCircle && groundCirclePrefab == null)
        {
            CreateDefaultGroundCircle();
        }
        else if (showGroundCircle && groundCirclePrefab != null)
        {
            groundCircleInstance = Instantiate(groundCirclePrefab);
            groundCircleInstance.SetActive(false);
        }
        
        if (armDirectorCamera == null && arm != null)
        {
            armDirectorCamera = arm.DirectorModeCamera;
        }
        
        DisableVisuals();
    }

    void Update()
    {
        if (!isActive) return;
        
        UpdateLaser();
        UpdateGroundCircle();
    }

    public void ShowWaypointMarkers()
    {
        foreach (GameObject marker in waypointMarkers)
        {
            if (marker != null)
            {
                marker.SetActive(true);
            }
        }
    }
    
    public void HideWaypointMarkers()
    {
        foreach (GameObject marker in waypointMarkers)
        {
            if (marker != null)
            {
                marker.SetActive(false);
            }
        }
    }

    private void SetupLaser()
    {
        laserLine.startWidth = laserWidth;
        laserLine.endWidth = laserWidth;
        laserLine.material = new Material(Shader.Find("UI/Default"));
        laserLine.startColor = laserColor;
        laserLine.endColor = laserColor;
        laserLine.positionCount = 2;
        laserLine.enabled = false;
        
        laserLine.sortingOrder = 100;
    }

    private void CreateDefaultGroundCircle()
    {
        groundCircleInstance = new GameObject("GroundCircle");
        
        LineRenderer circleRenderer = groundCircleInstance.AddComponent<LineRenderer>();
        circleRenderer.material = new Material(Shader.Find("UI/Default"));
        circleRenderer.startColor = circleColor;
        circleRenderer.endColor = circleColor;
        circleRenderer.startWidth = 0.1f;
        circleRenderer.endWidth = 0.1f;
        circleRenderer.loop = true;
        circleRenderer.useWorldSpace = false;
        circleRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        circleRenderer.receiveShadows = false;
        
        int segments = 32;
        circleRenderer.positionCount = segments;
        
        float angle = 0f;
        for (int i = 0; i < segments; i++)
        {
            float x = Mathf.Cos(angle) * circleSize;
            float z = Mathf.Sin(angle) * circleSize;
            circleRenderer.SetPosition(i, new Vector3(x, 0.01f, z)); // 0.01f per evitare z-fighting
            angle += 2f * Mathf.PI / segments;
        }
        
        groundCircleInstance.SetActive(false);
    }

    private void UpdateLaser()
    {
        if (!showLaser || armDirectorCamera == null || laserLine == null)
        {
            if (laserLine != null) laserLine.enabled = false;
            return;
        }
        
        Vector3 startPos = armDirectorCamera.transform.position;
        Vector3 direction = Vector3.down;
        
        RaycastHit hit;
        Vector3 endPos;
        
        if (Physics.Raycast(startPos, direction, out hit, maxLaserDistance, groundLayer))
        {
            endPos = hit.point;
        }
        else
        {
            endPos = startPos + direction * maxLaserDistance;
        }
        
        laserLine.SetPosition(0, startPos);
        laserLine.SetPosition(1, endPos);
        laserLine.enabled = true;
    }

    private void UpdateGroundCircle()
    {
        if (!showGroundCircle || armDirectorCamera == null || groundCircleInstance == null)
        {
            if (groundCircleInstance != null) groundCircleInstance.SetActive(false);
            return;
        }
        
        Vector3 startPos = armDirectorCamera.transform.position;
        Vector3 direction = Vector3.down;
        
        RaycastHit hit;
        
        if (Physics.Raycast(startPos, direction, out hit, maxLaserDistance, groundLayer))
        {
            groundCircleInstance.transform.position = hit.point + Vector3.up * 0.01f;
            groundCircleInstance.SetActive(true);
        }
        else
        {
            groundCircleInstance.SetActive(false);
        }
    }

    public void CreateWaypointMarker(int waypointIndex)
    {
        if (armDirectorCamera == null) return;
        
        Vector3 startPos = armDirectorCamera.transform.position;
        Vector3 direction = Vector3.down;
        
        RaycastHit hit;
        
        if (Physics.Raycast(startPos, direction, out hit, maxLaserDistance, groundLayer))
        {
            GameObject marker;
            
            if (waypointMarkerPrefab != null)
            {
                marker = Instantiate(waypointMarkerPrefab, hit.point, Quaternion.identity);
            }
            else
            {
                marker = CreateDefaultWaypointMarker(hit.point);
            }
            
            marker.name = $"Waypoint_{waypointIndex}";
            waypointMarkers.Add(marker);
        }
    }

    private GameObject CreateDefaultWaypointMarker(Vector3 position)
    {
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        marker.transform.position = position;
        marker.transform.localScale = new Vector3(0.5f, waypointHeight / 2f, 0.5f);
        marker.transform.position += Vector3.up * waypointHeight / 2f;
        
        Renderer renderer = marker.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("UI/Default"));
        mat.color = waypointColor;
        renderer.material = mat;
        
        Destroy(marker.GetComponent<Collider>());
        
        GameObject textObj = new GameObject("WaypointNumber");
        textObj.transform.SetParent(marker.transform);
        textObj.transform.localPosition = Vector3.up * 0.5f;
        textObj.transform.localRotation = Quaternion.Euler(90, 0, 0);
        
        TextMesh textMesh = textObj.AddComponent<TextMesh>();
        textMesh.text = (waypointMarkers.Count + 1).ToString();
        textMesh.fontSize = 50;
        textMesh.characterSize = 0.1f;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = Color.white;
        
        return marker;
    }

    public void ClearAllWaypointMarkers()
    {
        foreach (GameObject marker in waypointMarkers)
        {
            if (marker != null)
            {
                Destroy(marker);
            }
        }
        
        waypointMarkers.Clear();
    }

    public void EnableVisuals()
    {
        isActive = true;
        
        if (laserLine != null)
            laserLine.enabled = showLaser;
        
        if (groundCircleInstance != null)
            groundCircleInstance.SetActive(showGroundCircle);
        
        ShowWaypointMarkers();
    }

    public void DisableVisuals()
    {
        isActive = false;
        
        if (laserLine != null)
            laserLine.enabled = false;
        
        if (groundCircleInstance != null)
            groundCircleInstance.SetActive(false);
        
        HideWaypointMarkers();
    }

    void OnDestroy()
    {
        ClearAllWaypointMarkers();
        
        if (groundCircleInstance != null)
        {
            Destroy(groundCircleInstance);
        }
    }
}