using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

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
    [SerializeField] private string sceneToLoad; // nome della scena da caricare MI RACCOMANDO DEVE COINCIDERE ESATTAMENTE
    [SerializeField] private Vector3 sceneOffset = new Vector3(0, 0, 10); // offset della scena
    [SerializeField] private bool unloadPreviousScene = false;
    [SerializeField] private float loadDelay = 1f; // tempo dopo apertura prima di caricare

    private bool isOpen = false;
    public bool IsOpen => isOpen;

    // private bool isLocked = false;
    // public bool IsLocked => isLocked;

    private bool sceneLoaded = false;
    private Scene loadedScene;

    public void Interact()
    {
        // Debug.Log("Interazione con " + this.name);

        if (!isOpen)
        {
            doorAnimator.SetTrigger("Open");
            isOpen = true;

            if (!sceneLoaded && !string.IsNullOrEmpty(sceneToLoad))
            {
                StartCoroutine(LoadAdditiveSceneAfterDelay());
            }
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

    private IEnumerator LoadAdditiveSceneAfterDelay()
    {
        yield return new WaitForSeconds(loadDelay);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);

        while (!asyncLoad.isDone)
        {
            // Debug.Log("Caricamento scena in corso...");
            yield return null;
        }

        loadedScene = SceneManager.GetSceneByName(sceneToLoad);
        sceneLoaded = true;

        DontDestroyOnLoad(PlayerController.instance.gameObject);

        // Debug.Log("Scena caricata: " + loadedScene.name);
        
        PositionLoadedScene();

        if (unloadPreviousScene)
        {
            UnloadPreviousScene();
        }
    }

    private void PositionLoadedScene()
    {
        if (!loadedScene.IsValid())
        {
            Debug.LogWarning("Loaded scene is not valid.");
            return;
        }

        GameObject[] rootObjects = loadedScene.GetRootGameObjects();

        Vector3 targetPosition = sceneOffset;

        foreach (GameObject obj in rootObjects)
        {
            obj.transform.position += targetPosition;
        }
    }

    private void UnloadPreviousScene()
    {
        Scene currentScene = gameObject.scene;
        
        if (currentScene.IsValid())
        {
            Debug.Log($"Scaricamento scena: {currentScene.name}");
            SceneManager.UnloadSceneAsync(currentScene);
        }
    }
}
