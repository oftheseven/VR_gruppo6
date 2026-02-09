using UnityEngine;
using System.Collections;

public class Marker : MonoBehaviour
{
    [Header("Drop zone settings")]
    [SerializeField] private string dropZoneName = "Drop zone";
    [SerializeField] private float dropHeight = 0.1f;
    [SerializeField] private float detectionRadius = 3f;

    [Header("Visual feedback")]
    [SerializeField] private GameObject markerVisual; // reference al marker
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color activeColor = Color.green;

    [Header("Drop area")]
    [SerializeField] private bool useRandomPosition = true;
    [SerializeField] private float randomRadius = 0.5f;

    private Renderer markerRenderer;
    private Transform playerTransform;
    private bool playerNearby = false;
    private Material markerMaterial;

    void Start()
    {
        if (markerVisual != null)
        {
            markerRenderer = markerVisual.GetComponent<Renderer>();
            
            if (markerRenderer != null)
            {
                markerMaterial = markerRenderer.material;
            }
        }

        if (PlayerController.instance != null)
        {
            playerTransform = PlayerController.instance.transform;
        }

        SetNormalState();
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
            SetActiveState();
        }
        else if (!playerNearby && wasNearby)
        {
            SetNormalState();
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

    private void SetNormalState()
    {
        if (markerRenderer != null)
        {
            markerRenderer.material.color = normalColor;
        }
    }

    private void SetActiveState()
    {
        if (markerRenderer != null)
        {
            markerRenderer.material.color = activeColor;
        }
    }

    public bool IsPlayerNearby() => playerNearby;
    public string GetDropZoneName() => dropZoneName;
}