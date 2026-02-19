using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    // void Start()
    // {
    //     Debug.Log("Main Menu Loaded");
    //     Application.targetFrameRate = 60;
    //     QualitySettings.vSyncCount = 1;
    // }

    // public void PlayApp()
    // {
    //     SceneManager.LoadSceneAsync(1); // carico la scena
    // }

    // public void QuitApp()
    // {
    //     Application.Quit(); // esco dall'applicazione
    // }

    [Header("UI References")]
    [SerializeField] private GameObject loadingPanel;

    void Start()
    {
        Debug.Log("Main Menu Loaded");
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 1;

        if (loadingPanel != null)
            loadingPanel.SetActive(false);
    }

    public void PlayApp()
    {
        StartCoroutine(LoadGameSceneAsync());
    }

    private System.Collections.IEnumerator LoadGameSceneAsync()
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        AsyncOperation async = SceneManager.LoadSceneAsync(1);

        while (!async.isDone)
        {
            yield return null;
        }
    }

    public void QuitApp()
    {
        Application.Quit(); // esco dall'applicazione
    }
}
