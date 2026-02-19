using UnityEngine;

public class DirectorModeAnimStarter : MonoBehaviour
{
    [Header("Animation setup")]
    [SerializeField] private Animator animator;
    [SerializeField] private string triggerName = "MoveToPosition";
    private Vector3 startingPosition;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        if (animator == null)
            Debug.LogWarning($"{gameObject.name}: Animator non assegnato!");

        this.startingPosition = this.transform.position;
    }

    public void PlayDirectorAnim()
    {
        if (animator != null)
        {
            animator.SetTrigger(triggerName);
            Debug.Log($"{gameObject.name}: Trigger animazione '{triggerName}' lanciato");
        }
    }

    public void ResetAnimation()
    {
        if (animator != null)
        {
            animator.ResetTrigger(triggerName);
            Debug.Log($"{gameObject.name}: Trigger animazione '{triggerName}' resettato");
        }
    }

    public void Reset()
    {
        this.transform.position = startingPosition;
        ResetAnimation();
    }
}