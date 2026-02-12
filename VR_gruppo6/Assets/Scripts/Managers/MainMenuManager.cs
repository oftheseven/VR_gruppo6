using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    void Start()
    {
        Debug.Log("Main Menu Loaded");
    }

    public void PlayApp()
    {
        SceneManager.LoadSceneAsync("TutorialScene"); // carico la prima scena
    }

    public void QuitApp()
    {
        Application.Quit(); // esco dall'applicazione
    }
}
