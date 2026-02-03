using UnityEngine;
using TMPro;

public class Waypoint : MonoBehaviour
{
    [Header("Waypoint settings")]
    [SerializeField] private int waypointIndex; // indice del waypoint (ordine)
    [SerializeField] private float triggerRadius = 0.5f;

    [Header("Visual feedback")]
    [SerializeField] private MeshRenderer sphereRenderer;
    [SerializeField] private TextMeshPro waypointLabel; // etichetta del waypoint

    [Header("Colors")]
    [SerializeField] private Color inactiveColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);
    [SerializeField] private Color activeColor = new Color(1f, 0.8f, 0f, 0.6f);
    [SerializeField] private Color reachedColor = new Color(0f, 1f, 0f, 0.8f);

    private bool isActive = false;
    private bool isReached = false;
    private SphereCollider sphereCollider;
    private Material sphereMaterial;

    public int WaypointIndex => waypointIndex;
    public bool IsActive => isActive;
    public bool IsReached => isReached;

    void Awake()
    {
        sphereCollider = GetComponent<SphereCollider>();
        if (sphereCollider == null)
        {
            sphereCollider = gameObject.AddComponent<SphereCollider>();
        }
        sphereCollider.isTrigger = true;
        sphereCollider.radius = triggerRadius;

        if (sphereRenderer != null)
        {
            sphereMaterial = sphereRenderer.material;
        }

        SetInactive();
    }

    public void SetActive()
    {
        isActive = true;
        UpdateVisuals();
        Debug.Log($"✅ Waypoint {waypointIndex} attivato");
    }

    public void SetInactive()
    {
        isActive = false;
        UpdateVisuals();
    }

    public void MarkAsReached()
    {
        if (isReached)
        {
            return;
        }

        isReached = true;
        UpdateVisuals();
        Debug.Log($"🏁 Waypoint {waypointIndex} raggiunto");
    }

    private void UpdateVisuals()
    {
        if (sphereMaterial != null)
        {
            if (isReached)
            {
                sphereMaterial.color = reachedColor;
            }
            else if (isActive)
            {
                sphereMaterial.color = activeColor;
            }
            else
            {
                sphereMaterial.color = inactiveColor;
            }
        }

        if (waypointLabel != null)
        {
            waypointLabel.text = waypointIndex.ToString();
            waypointLabel.color = isReached ? Color.white : (isActive ? Color.yellow : Color.gray);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isActive || isReached)
        {
            return;
        }

        if (other.CompareTag("ArmTip"))
        {
            WaypointManager.instance?.OnWayPointReached(this);
        }
    }
}