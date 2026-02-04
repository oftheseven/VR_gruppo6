using UnityEngine;
using System.Collections;

// script per gestire la fase di tutorial (TODO: fare una funzione che notifica la fine del tutorial e quindi sblocca la porta del tutorial per passare alla scena successiva)
public class TutorialManager : MonoBehaviour
{
    // singleton
    private static TutorialManager _instance;
    public static TutorialManager instance => _instance;

    [Header("Scene door reference")]
    [SerializeField] private InteractableDoor tutorialDoor; // porta della scena tutorial

    private bool isFinished = false;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
        tutorialDoor.Lock();
    }

    void Start()
    {
        if (tutorialDoor != null)
        {
            tutorialDoor.Lock();
        }

        StartCoroutine(Tutorial());
    }

    // coroutine per gestire il tutorial (ad esempio durata fissa di 10 secondi) 
    // TODO: fare la meccanica reale
    private IEnumerator Tutorial()
    {
        Debug.Log("Inizio tutorial...");
        yield return new WaitForSeconds(10f);
        CompleteTutorial();
    }

    private void CompleteTutorial()
    {
        isFinished = true;
        
        if (tutorialDoor != null)
        {
            tutorialDoor.Unlock();
            Debug.Log("🔓 Tutorial completato! Porta sbloccata.");
        }
    }

    public void MarkAsComplete()
    {
        if (!isFinished)
        {
            StopAllCoroutines();
            CompleteTutorial();
        }
    }
}
