using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.EventSystems;

public class InteractableDoor : MonoBehaviour
{
    [Header("Door settings")]
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private bool isLocked = false;

    [Header("Lock after traversal")]
    [SerializeField] private bool lockAfterFirstTraversal = false;
    [SerializeField] private bool keepSceneALoaded = false;
    [SerializeField] private bool keepSceneBLoaded = false;

    [Header("Connected scenes")]
    [SerializeField] private string sceneA = ""; // prima scena
    [SerializeField] private string sceneB = ""; // seconda scena
    [SerializeField] private Vector3 offsetFromAToB = new Vector3(0, 0, 0);
    [SerializeField] private float loadDelay = 1f;

    private bool isOpen = false;
    public bool IsOpen => isOpen;

    public bool IsLocked => isLocked;

    private string currentLoadedScene = "";
    private string sceneWhereOpenedFrom = "";

    public void Interact()
    {
        if (isLocked)
        {
            Debug.Log("🔒 La porta è chiusa a chiave.");
            return;
        }

        if (!isOpen)
        {
            OpenDoor();
        }
        else
        {
            CloseDoor();
        }
    }

    private void OpenDoor()
    {
        sceneWhereOpenedFrom = SceneZone.GetCurrentPlayerScene();
        // Debug.Log($"Apro porta da: {playerScene}");

        string sceneToLoad = GetSceneToLoad(sceneWhereOpenedFrom);

        if (!string.IsNullOrEmpty(sceneToLoad) && !IsSceneLoaded(sceneToLoad))
        {
            StartCoroutine(LoadOtherScene(sceneToLoad, sceneWhereOpenedFrom));
        }
        else
        {
            // se invece la scena è già caricata, apro subito la porta senza ricaricare
            isOpen = true;
            doorAnimator.SetTrigger("Open");
        }
    }

    private void CloseDoor()
    {
        isOpen = false;
        doorAnimator.SetTrigger("Close");
        
        string currentPlayerScene = SceneZone.GetCurrentPlayerScene();
        // Debug.Log($"Chiudo porta, player in: {currentPlayerScene}");

        if (lockAfterFirstTraversal && 
            !string.IsNullOrEmpty(sceneWhereOpenedFrom) && 
            currentPlayerScene != sceneWhereOpenedFrom)
        {
            Lock();
            // Debug.Log("Porta bloccata permanentemente dopo attraversamento!");
        }

        sceneWhereOpenedFrom = "";

        string sceneToUnload = "";

        if (currentPlayerScene == sceneA)
        {
            sceneToUnload = sceneB;
            // Debug.Log($"Player in {sceneA}, scarico {sceneB}");
        }
        else if (currentPlayerScene == sceneB)
        {
            sceneToUnload = sceneA;
            // Debug.Log($"Player in {sceneB}, scarico {sceneA}");
        }
        else
        {
            // Debug.LogWarning($"Player in scena sconosciuta: {currentPlayerScene}");
            return;
        }

        if (sceneToUnload == currentPlayerScene)
        {
            Debug.LogWarning($"ATTENZIONE: non scarico la scena corrente del player ({currentPlayerScene})");
            return;
        }
        
        if (IsSceneLoaded(sceneToUnload))
        {
            StartCoroutine(UnloadOldScene(sceneToUnload));
        }
    }

    private string GetSceneToLoad(string playerScene)
    {
        if (playerScene == sceneA)
        {
            return sceneB;
        }
        else if (playerScene == sceneB)
        {
            return sceneA;
        }
        
        return sceneB;
    }

    private bool IsSceneLoaded(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.name == sceneName)
            {
                return true;
            }
        }
        return false;
    }

    private IEnumerator LoadOtherScene(string sceneToLoad, string fromScene)
    {
        if (UI_LoadingIcon.instance != null)
        {
            UI_LoadingIcon.instance.Show("Loading...");
        }

        yield return new WaitForSeconds(loadDelay);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        currentLoadedScene = sceneToLoad;

        Scene loadedScene = SceneManager.GetSceneByName(sceneToLoad);
        if (loadedScene.IsValid())
        {
            SceneManager.SetActiveScene(loadedScene);
        }

        RemoveDuplicateEventSystems(currentLoadedScene);
        PositionScene(sceneToLoad, fromScene);

        yield return new WaitForSeconds(0.2f);

        if (UI_LoadingIcon.instance != null)
        {
            UI_LoadingIcon.instance.Hide();
        }

        isOpen = true;
        doorAnimator.SetTrigger("Open");
    }

    private void RemoveDuplicateEventSystems(string loadedSceneName)
    {
        Scene loadedScene = SceneManager.GetSceneByName(loadedSceneName);
        
        if (!loadedScene.IsValid())
        {
            return;
        }

        GameObject[] rootObjects = loadedScene.GetRootGameObjects();
        
        foreach (GameObject rootObj in rootObjects)
        {
            EventSystem[] eventSystems = rootObj.GetComponentsInChildren<EventSystem>(true);
            
            foreach (EventSystem es in eventSystems)
            {
                es.enabled = false;
                // Debug.Log($"Rimosso EventSystem duplicato da scena {loadedSceneName}");
                DestroyImmediate(es.gameObject);
            }
        }
    }

    private void PositionScene(string loadedScene, string fromScene)
    {
        Scene scene = SceneManager.GetSceneByName(loadedScene);
        
        if (!scene.IsValid())
        {
            // Debug.LogWarning($"Scena {loadedScene} non valida");
            return;
        }

        Vector3 offset = Vector3.zero;
        
        if (fromScene == sceneA && loadedScene == sceneB)
        {
            offset = offsetFromAToB;
        }

        GameObject[] rootObjects = scene.GetRootGameObjects();
        foreach (GameObject obj in rootObjects)
        {
            obj.transform.position += offset;
        }

        // Debug.Log($"Scena {loadedScene} posizionata a offset {offset}");
    }

    private IEnumerator UnloadOldScene(string sceneToUnload)
    {
        if ((sceneToUnload == sceneA && keepSceneALoaded) || 
            (sceneToUnload == sceneB && keepSceneBLoaded))
        {
            // Debug.Log($"Scena {sceneToUnload} non viene scaricata (keep loaded)");
            yield break;
        }

        yield return new WaitForSeconds(2f);

        string playerScene = SceneZone.GetCurrentPlayerScene();
        
        if (playerScene == sceneToUnload)
        {
            // Debug.Log($"Player ancora in {sceneToUnload}, annullo scaricamento");
            yield break;
        }

        Scene scene = SceneManager.GetSceneByName(sceneToUnload);
        
        if (scene.IsValid())
        {
            // Debug.Log($"Scarico scena: {sceneToUnload}");
            SceneManager.UnloadSceneAsync(sceneToUnload);
            currentLoadedScene = "";
        }
    }

    public void Lock()
    {
        isLocked = true;
        Debug.Log($"Porta {gameObject.name} bloccata");
    }

    public void Unlock()
    {
        isLocked = false;
        Debug.Log($"Porta {gameObject.name} sbloccata");
    }

    public string GetInteractionText()
    {
        if (isLocked)
        {
            return "Porta chiusa a chiave";
        }
        return isOpen ? "Premi E per chiudere la porta" : "Premi E per aprire la porta";
    }
}