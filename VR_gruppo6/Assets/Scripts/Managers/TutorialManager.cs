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

    private void Awake()
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

    void Update()
    {
        StartCoroutine(Tutorial());
        if (isFinished)
        {
            // sblocca la porta del tutorial
            Debug.Log("🔓 Tutorial completato! Porta sbloccata.");
            tutorialDoor.Unlock();
        }
    }

    // coroutine per gestire il tutorial (ad esempio durata fissa di 10 secondi) 
    // TODO: fare la meccanica reale
    private IEnumerator Tutorial()
    {
        yield return new WaitForSeconds(10f);
        isFinished = true;
    }
}
