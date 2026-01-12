using UnityEngine;

public class InteractableCart : MonoBehaviour
{
    [Header("Interaction text")]
    [SerializeField] private string interactionText = "Premi E per muovere il carrello";

    [Header("Player reference")]
    [SerializeField] private PlayerController playerController;
    
    [Header("Cart movement settings")]
    [SerializeField] private float followDistance = 2f;
    [SerializeField] private float followSpeed = 10f;
    [SerializeField] private float rotationSpeed = 2f;

    private Rigidbody rb;
    private bool isBeingHeld = false;
    private Transform playerTransform;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; // attivo kinematic per evitare che venga influenzato dalla fisica
    }

    void FixedUpdate()
    {
        if (isBeingHeld && playerTransform != null)
        {
            FollowPlayer();
        }
    }

    public void Interact()
    {
        Debug.Log("Interazione con " + this.gameObject.name);

        if (!isBeingHeld)
        {
            StartHolding();
        }
        else
        {
            StopHolding();
        }
    }

    public string getInteractionText()
    {
        return interactionText;
    }

    private void StartHolding()
    {
        if (playerController != null)
        {
            playerTransform = playerController.transform;
            isBeingHeld = true;

            rb.isKinematic = false;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            playerController.SetPushingCart(this);

            // Debug.Log("Carrello preso in mano");
        }
    }

    private void StopHolding()
    {
        isBeingHeld = false;
        
        if (playerController != null)
        {
            playerController.SetPushingCart(null);
        }

        playerTransform = null;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.isKinematic = true;

        // Debug.Log("Carrello lasciato");
    }

    private void FollowPlayer()
    {
        Vector3 targetPosition = playerTransform.position + playerTransform.forward * followDistance;
        targetPosition.y = transform.position.y;

        Vector3 newPosition = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPosition);

        Quaternion targetRotation = Quaternion.LookRotation(playerTransform.forward);
        Quaternion newRotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(newRotation);
    }

    public bool IsBeingHeld()
    {
        return isBeingHeld;
    }
}
