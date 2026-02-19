using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    void Start()
    {
        Debug.Log("Main Menu Loaded");
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 1;
    }

    public void PlayApp()
    {
        SceneManager.LoadSceneAsync(1); // carico la scena
    }

    public void QuitApp()
    {
        Application.Quit(); // esco dall'applicazione
    }
}
