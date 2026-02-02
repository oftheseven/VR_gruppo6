using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.EventSystems;

public class InteractableDoor : MonoBehaviour
{
    [Header("Interaction text")]
    [SerializeField] private string interactionText = "Premi E per aprire la porta";

    [Header("Door settings")]
    [SerializeField] private Animator doorAnimator;

    [Header("Connected Scenes")]
    [SerializeField] private string sceneA = "TortaInTesta"; // Prima scena
    [SerializeField] private string sceneB = "DivinationClass"; // Seconda scena
    [SerializeField] private Vector3 offsetFromAToB = new Vector3(0, 0, 10); // Offset quando carichi B da A
    [SerializeField] private float loadDelay = 1f;

    private bool isOpen = false;
    public bool IsOpen => isOpen;

    private string currentLoadedScene = ""; // Quale scena "extra" è caricata

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
        
        // Rileva da quale scena sto aprendo
        string playerScene = SceneZone.GetCurrentPlayerScene();
        Debug.Log($"🚪 Apro porta da: {playerScene}");

        // Determina quale scena caricare
        string sceneToLoad = GetSceneToLoad(playerScene);

        if (!string.IsNullOrEmpty(sceneToLoad) && !IsSceneLoaded(sceneToLoad))
        {
            StartCoroutine(LoadOtherScene(sceneToLoad, playerScene));
        }
        else
        {
            Debug.Log($"✓ Scena {sceneToLoad} già caricata");
        }
    }

    private void CloseDoor()
    {
        isOpen = false;
        doorAnimator.SetTrigger("Close");
        
        string playerScene = SceneZone.GetCurrentPlayerScene();
        Debug.Log($"🚪 Chiudo porta, player in: {playerScene}");

        // Se player è dall'altra parte, scarica la scena vecchia
        string sceneToUnload = GetSceneToUnload(playerScene);
        
        // if (!string.IsNullOrEmpty(sceneToUnload) && IsSceneLoaded(sceneToUnload))
        // {
        //     StartCoroutine(UnloadOldScene(sceneToUnload));
        // }
    }

    // ⭐ Determina quale scena caricare in base a dove è il player
    private string GetSceneToLoad(string playerScene)
    {
        if (playerScene == sceneA)
        {
            return sceneB; // Player in A, carica B
        }
        else if (playerScene == sceneB)
        {
            return sceneA; // Player in B, carica A
        }
        
        // Default: carica B se non sappiamo dove è il player
        Debug.LogWarning($"⚠️ Player in scena sconosciuta: {playerScene}, carico {sceneB}");
        return sceneB;
    }

    // ⭐ Determina quale scena scaricare
    private string GetSceneToUnload(string playerScene)
    {
        if (playerScene == sceneA)
        {
            return sceneB; // Player in A, scarica B
        }
        else if (playerScene == sceneB)
        {
            return sceneA; // Player in B, scarica A
        }
        
        return "";
    }

    // ⭐ Controlla se una scena è già caricata
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

        Debug.Log($"🌍 Carico {sceneToLoad} (aperto da {fromScene})");

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        currentLoadedScene = sceneToLoad;
        Debug.Log($"✅ Scena {sceneToLoad} caricata!");

        // Persist player e UI
        if (PlayerController.instance != null)
        {
            DontDestroyOnLoad(PlayerController.instance.gameObject);
        }
        
        if (EventSystem.current != null)
        {
            DontDestroyOnLoad(EventSystem.current.gameObject);
        }

        // Posiziona la scena caricata
        PositionScene(sceneToLoad, fromScene);
    }

    // ⭐ Posiziona la scena caricata in base a da dove è stata aperta
    private void PositionScene(string loadedScene, string fromScene)
    {
        Scene scene = SceneManager.GetSceneByName(loadedScene);
        
        if (!scene.IsValid())
        {
            Debug.LogWarning($"⚠️ Scena {loadedScene} non valida");
            return;
        }

        // Calcola offset
        Vector3 offset = Vector3.zero;
        
        if (fromScene == sceneA && loadedScene == sceneB)
        {
            offset = offsetFromAToB; // A → B: usa offset configurato
        }
        else if (fromScene == sceneB && loadedScene == sceneA)
        {
            offset = -offsetFromAToB; // B → A: offset opposto
        }

        // Applica offset
        GameObject[] rootObjects = scene.GetRootGameObjects();
        foreach (GameObject obj in rootObjects)
        {
            obj.transform.position += offset;
        }

        Debug.Log($"📦 Scena {loadedScene} posizionata a offset {offset}");
    }

    // private IEnumerator UnloadOldScene(string sceneToUnload)
    // {
    //     yield return new WaitForSeconds(0.5f);

    //     // Verifica che player NON sia nella scena da scaricare
    //     string playerScene = SceneZone.GetCurrentPlayerScene();
        
    //     if (playerScene == sceneToUnload)
    //     {
    //         Debug.Log($"⚠️ Player ancora in {sceneToUnload}, annullo scaricamento");
    //         yield break;
    //     }

    //     Scene scene = SceneManager.GetSceneByName(sceneToUnload);
        
    //     if (scene.IsValid())
    //     {
    //         Debug.Log($"🗑️ Scarico scena: {sceneToUnload}");
    //         SceneManager.UnloadSceneAsync(sceneToUnload);
    //         currentLoadedScene = "";
    //     }
    // }

    public string GetInteractionText()
    {
        return isOpen ? "Premi E per chiudere la porta" : "Premi E per aprire la porta";
    }
}