using UnityEngine;
using System.Collections;

public class Marker : MonoBehaviour
{
    [Header("Drop zone settings")]
    [SerializeField] private string dropZoneName = "Drop zone";
    [SerializeField] private float dropHeight = 0.1f;
    [SerializeField] private float detectionRadius = 1.5f;

    [Header("Visual feedback")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color activeColor = Color.green;
    [SerializeField] private float outlineWidth = 5f;

    [Header("Drop area")]
    [SerializeField] private bool useRandomPosition = true;
    [SerializeField] private float randomRadius = 0.5f;

    private Renderer markerRenderer;
    private Transform playerTransform;
    private bool playerNearby = false;
    private Material markerMaterial;

    void Start()
    {
        if (this.gameObject != null)
        {
            markerRenderer = this.gameObject.GetComponent<Renderer>();
            
            if (markerRenderer != null)
            {
                markerMaterial = markerRenderer.material;
            }
        }

        if (PlayerController.instance != null)
        {
            playerTransform = PlayerController.instance.transform;
        }

        // SetNormalState();
        SetupOutlines();
    }

    void Update()
    {
        CheckPlayerProximity();
    }

    private void CheckPlayerProximity()
    {
        if (playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        bool wasNearby = playerNearby;
        playerNearby = distance <= detectionRadius;

        if (playerNearby && !wasNearby)
        {
            // SetActiveState();
            SetOutlinesEnabled(this.gameObject, true);
        }
        else if (!playerNearby && wasNearby)
        {
            // SetNormalState();
            DisableAllOutlines();
        }
    }

    public Vector3 DropItem (PickableItem item)
    {
        Vector3 dropPosition = GetDropPosition();

        GameObject spawnedItem = item.SpawnInWorld(dropPosition, Quaternion.identity);

        if (spawnedItem != null)
        {
            Rigidbody rb = spawnedItem.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;

                StartCoroutine(StabilizeObject(rb));
            }

            Debug.Log($"📦 '{item.GetItemID()}' droppato in {dropZoneName}");
        }

        return dropPosition;
    }

    private IEnumerator StabilizeObject(Rigidbody rb)
    {
        bool wasKinematic = rb.isKinematic;
        rb.isKinematic = true;
        
        yield return new WaitForFixedUpdate();
        
        rb.isKinematic = wasKinematic;
    }

    private void SetupOutlines()
    {
        GetOrAddOutline(this.gameObject);

        DisableAllOutlines();
    }

    private void GetOrAddOutline(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
    
        foreach (Renderer renderer in renderers)
        {
            Outline outline = renderer.gameObject.GetComponent<Outline>();
            if (outline == null)
            {
                outline = renderer.gameObject.AddComponent<Outline>();
            }

            outline.OutlineColor = activeColor;
            outline.OutlineWidth = outlineWidth;
            outline.enabled = false;
        }
    }

    private void SetOutlinesEnabled(GameObject parent, bool enabled)
    {
        Outline[] outlines = parent.GetComponentsInChildren<Outline>();
        foreach (Outline outline in outlines)
        {
            outline.enabled = enabled;
        }
    }

    private void DisableAllOutlines()
    {
        SetOutlinesEnabled(this.gameObject, false);
    }

    private Vector3 GetDropPosition()
    {
        Vector3 basePosition = transform.position;
        basePosition.y += dropHeight;
        
        if (useRandomPosition)
        {
            Vector2 randomCircle = Random.insideUnitCircle * randomRadius;
            basePosition.x += randomCircle.x;
            basePosition.z += randomCircle.y;
        }
        
        return basePosition;
    }

    // private void SetNormalState()
    // {
    //     if (markerRenderer != null)
    //     {
    //         markerMaterial.color = normalColor;
    //     }
    // }

    // private void SetActiveState()
    // {
    //     if (markerRenderer != null)
    //     {
    //         markerMaterial.color = activeColor;
    //     }
    // }

    public bool IsPlayerNearby() => playerNearby;
    public string GetDropZoneName() => dropZoneName;
}