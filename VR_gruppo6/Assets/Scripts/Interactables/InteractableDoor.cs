using UnityEditor;
using UnityEngine;

public class InteractableDoor : MonoBehaviour
{
    [Header("Interaction text")]
    [SerializeField] private string interactionText = "Premi E per aprire la porta";

    [Header("Sub-door reference")]
    [SerializeField] private GameObject subDoor; // riferimento alla sotto-porta che si apre/chiude

    [Header("Door settings")]
    [SerializeField] private Animator doorAnimator;
    // [SerializeField] private AudioSource doorAudioSource;
    // [SerializeField] private AudioClip openClip;
    // [SerializeField] private AudioClip lockedClip;

    [Header("Scene to load reference")]
    [SerializeField] private SceneAsset sceneToLoad; // reference alla scena da caricare quando si apre la porta

    private bool isOpen = false;
    public bool IsOpen => isOpen;

    private bool isLocked = false;
    public bool IsLocked => isLocked;

    void Start()
    {
        
    }

    public void Interact()
    {
        Debug.Log("Interazione con " + this.name);
    }

    public string GetInteractionText()
    {
        return interactionText;
    }
}
