using UnityEditor;
using UnityEngine;

public class InteractableDoor : MonoBehaviour
{
    [Header("Interaction text")]
    [SerializeField] private string interactionText = "Premi E per aprire la porta";

    [Header("Door settings")]
    [SerializeField] private Animator doorAnimator;
    // [SerializeField] private AudioSource doorAudioSource;
    // [SerializeField] private AudioClip openClip;
    // [SerializeField] private AudioClip lockedClip;

    [Header("Scene to load reference")]
    [SerializeField] private SceneAsset sceneToLoad; // reference alla scena da caricare quando si apre la porta

    private bool isOpen = false;
    public bool IsOpen => isOpen;

    // private bool isLocked = false;
    // public bool IsLocked => isLocked;

    public void Interact()
    {
        Debug.Log("Interazione con " + this.name);

        if (!isOpen)
        {
            doorAnimator.SetTrigger("Open");
            isOpen = true;

            // // carico la scena specificata
            // if (sceneToLoad != null)
            // {
            //     string sceneName = sceneToLoad.name;
            //     UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
            // }
        }
        else
        {
            doorAnimator.SetTrigger("Close");
            isOpen = false;
        }
    }

    public string GetInteractionText()
    {
        if (!isOpen)
        {
            interactionText = "Premi E per aprire la porta";
        }
        else
        {
            interactionText = "Premi E per chiudere la porta";
        }
        return interactionText;
    }
}
