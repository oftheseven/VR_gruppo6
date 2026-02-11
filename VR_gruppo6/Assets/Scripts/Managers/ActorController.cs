using UnityEngine;
using UnityEngine.AI;

public class ActorController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator animator;

    [Header("Target")]
    [SerializeField] private Transform walkTarget;

    [Header("Animation Parameters")]
    private const string PARAM_IS_WALKING = "IsWalking";
    private const string PARAM_DO_ACTION = "DoAction";

    [Header("Settings")]
    [SerializeField] private float stoppingDistanceOffset = 0.1f;

    private bool isWalking = false;
    private bool destinationReached = false;

    void Start()
    {
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (agent == null || animator == null)
        {
            Debug.LogError($"{gameObject.name}: NavMeshAgent o Animator mancante!");
            enabled = false;
            return;
        }

        agent.updateRotation = true;
        agent.updatePosition = true;
        agent.isStopped = true;
    }

    void Update()
    {
        if (isWalking && agent != null && animator != null)
        {
            UpdateAnimationState();
        }
    }

    public void WalkToTarget()
    {
        if (walkTarget == null)
        {
            Debug.LogWarning($"{gameObject.name}: Nessun target assegnato!");
            return;
        }

        if (agent == null) return;

        agent.isStopped = false;
        agent.SetDestination(walkTarget.position);
        isWalking = true;
        destinationReached = false;

        Debug.Log($"{gameObject.name}: Cammina verso {walkTarget.name} @ {walkTarget.position}");
    }

    public void WalkToPosition(Vector3 position)
    {
        if (agent == null) return;

        agent.isStopped = false;
        agent.SetDestination(position);
        isWalking = true;
        destinationReached = false;

        Debug.Log($"{gameObject.name}: Cammina verso {position}");
    }

    public void StopWalking()
    {
        if (agent != null)
        {
            agent.isStopped = true;
        }

        isWalking = false;

        if (animator != null)
        {
            animator.SetBool(PARAM_IS_WALKING, false);
        }

        Debug.Log($"{gameObject.name}: Stop walking");
    }

    public void TriggerAction()
    {
        if (animator != null)
        {
            animator.SetTrigger(PARAM_DO_ACTION);
            Debug.Log($"{gameObject.name}: Action triggered!");
        }
    }

    public void ResetToIdle()
    {
        StopWalking();

        if (animator != null)
        {
            animator.Play("Idle");
        }

        destinationReached = false;

        Debug.Log($"{gameObject.name}: Reset to Idle");
    }

    private void UpdateAnimationState()
    {
        if (agent == null || animator == null) return;

        if (agent.pathPending)
        {
            animator.SetBool(PARAM_IS_WALKING, true);
            return;
        }

        float remainingDistance = agent.remainingDistance;
        float checkDistance = agent.stoppingDistance + stoppingDistanceOffset;

        bool shouldWalk = agent.hasPath && remainingDistance > checkDistance;
        
        animator.SetBool(PARAM_IS_WALKING, shouldWalk);

        if (!destinationReached && remainingDistance <= checkDistance)
        {
            if (agent.velocity.sqrMagnitude < 0.01f)
            {
                OnDestinationReached();
            }
        }
    }

    private void OnDestinationReached()
    {
        if (destinationReached) return;

        destinationReached = true;
        isWalking = false;
        animator.SetBool(PARAM_IS_WALKING, false);

        Debug.Log($"{gameObject.name}: Destinazione raggiunta!");
    }

    public void Teleport(Vector3 position)
    {
        if (agent != null)
        {
            agent.Warp(position);
        }
        else
        {
            transform.position = position;
        }

        Debug.Log($"{gameObject.name}: Teletrasportato a {position}");
    }
}