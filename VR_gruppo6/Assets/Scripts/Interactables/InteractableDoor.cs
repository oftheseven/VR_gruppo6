using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.EventSystems;

public class InteractableDoor : MonoBehaviour
{
    [Header("Door settings")]
    [SerializeField] private Animator doorAnimator;

    [Header("Connected Scenes")]
    [SerializeField] private string sceneA = "TortaInTesta"; // prima scena
    [SerializeField] private string sceneB = "DivinationClass"; // seconda scena
    [SerializeField] private Vector3 offsetFromAToB = new Vector3(0, 0, 0);
    [SerializeField] private float loadDelay = 1f;

    private bool isOpen = false;
    public bool IsOpen => isOpen;

    private string currentLoadedScene = "";

    public void Interact()
    {
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
        isOpen = true;
        doorAnimator.SetTrigger("Open");
        
        string playerScene = SceneZone.GetCurrentPlayerScene();
        // Debug.Log($"Apro porta da: {playerScene}");

        string sceneToLoad = GetSceneToLoad(playerScene);

        if (!string.IsNullOrEmpty(sceneToLoad) && !IsSceneLoaded(sceneToLoad))
        {
            StartCoroutine(LoadOtherScene(sceneToLoad, playerScene));
        }
        else
        {
            // Debug.Log($"Scena {sceneToLoad} già caricata");
        }
    }

    private void CloseDoor()
    {
        isOpen = false;
        doorAnimator.SetTrigger("Close");
        
        string currentPlayerScene = SceneZone.GetCurrentPlayerScene();
        Debug.Log($"🚪 Chiudo porta, player in: {currentPlayerScene}");

        // se il player è dall'altra parte, faccio l'unload dell'altra scena
        string sceneToUnload = "";

        if (currentPlayerScene == sceneA)
        {
            sceneToUnload = sceneB; // Player in A → scarica B
            Debug.Log($"🗑️ Player in {sceneA}, scarico {sceneB}");
        }
        else if (currentPlayerScene == sceneB)
        {
            sceneToUnload = sceneA; // Player in B → scarica A
            Debug.Log($"🗑️ Player in {sceneB}, scarico {sceneA}");
        }
        else
        {
            Debug.LogWarning($"⚠️ Player in scena sconosciuta: {currentPlayerScene}");
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
        yield return new WaitForSeconds(loadDelay);

        // Debug.Log($"Carico {sceneToLoad} (aperto da {fromScene})");

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        currentLoadedScene = sceneToLoad;
        // Debug.Log($"Scena {sceneToLoad} caricata!");

        RemoveDuplicateEventSystems(currentLoadedScene);
    
        PositionScene(sceneToLoad, fromScene);
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
                Debug.Log($"🗑️ Rimosso EventSystem duplicato da scena {loadedSceneName}");
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

    public string GetInteractionText()
    {
        return isOpen ? "Premi E per chiudere la porta" : "Premi E per aprire la porta";
    }
}