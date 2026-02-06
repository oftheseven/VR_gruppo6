using UnityEngine;

public class InteractableCart : MonoBehaviour
{
    [Header("Interaction text")]
    [SerializeField] private string interactionText = "Premi E per muovere il carrello";
    
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

        if (TutorialManager.instance != null)
        {
            TutorialManager.instance.OnCartCompleted();
        }

        if (GameManager.instance != null)
        {
            GameManager.instance.OnCartCompleted();
        }
    }

    public string GetInteractionText()
    {
        return interactionText;
    }

    private void StartHolding()
    {
        if (PlayerController.instance != null)
        {
            playerTransform = PlayerController.instance.transform;
            isBeingHeld = true;

            rb.isKinematic = false;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            PlayerController.instance.SetPushingCart(this);

            // Debug.Log("Carrello preso in mano");
        }
    }

    private void StopHolding()
    {
        isBeingHeld = false;
        
        if (PlayerController.instance != null)
        {
            PlayerController.instance.SetPushingCart(null);
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
