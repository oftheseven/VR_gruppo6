using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// script per gestire la fase di tutorial (TODO: fare una funzione che notifica la fine del tutorial e quindi sblocca la porta del tutorial per passare alla scena successiva)
public class TutorialManager : MonoBehaviour
{
    // singleton
    private static TutorialManager _instance;
    public static TutorialManager instance => _instance;

    [Header("Scene door reference")]
    [SerializeField] private InteractableDoor tutorialDoor; // porta della scena tutorial

    [Header("Tutorial Objects")]
    [SerializeField] private bool requireComputer = true;
    [SerializeField] private bool requireCamera = true;
    [SerializeField] private bool requireArm = true;
    [SerializeField] private bool requireCart = true;
    [SerializeField] private bool requireLight = true;

    private HashSet<string> completedTasks = new HashSet<string>();
    private bool isFinished = false;

    private const string TASK_COMPUTER = "computer";
    private const string TASK_CAMERA = "camera";
    private const string TASK_ARM = "arm";
    private const string TASK_CART = "cart";
    private const string TASK_LIGHT = "light";

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
        // tutorialDoor.Lock();
    }

    void Start()
    {
        if (tutorialDoor != null)
        {
            tutorialDoor.Lock();
        }

        // StartCoroutine(Tutorial());
    }

    // coroutine per gestire il tutorial (ad esempio durata fissa di 10 secondi) 
    // TODO: fare la meccanica reale
    // private IEnumerator Tutorial()
    // {
    //     Debug.Log("Inizio tutorial...");
    //     yield return new WaitForSeconds(10f);
    //     CompleteTutorial();
    // }

    public void CompleteTask(string taskId)
    {
        if (isFinished || completedTasks.Contains(taskId))
        {
            return;
        }

        completedTasks.Add(taskId);
        
        Debug.Log($"✅ Task '{taskId}' completato! ({completedTasks.Count}/{GetRequiredTasksCount()})");

        CheckTutorialCompletion();
    }

    private void CheckTutorialCompletion()
    {
        bool allCompleted = true;

        if (requireComputer && !completedTasks.Contains(TASK_COMPUTER))
            allCompleted = false;

        if (requireCamera && !completedTasks.Contains(TASK_CAMERA))
            allCompleted = false;

        if (requireArm && !completedTasks.Contains(TASK_ARM))
            allCompleted = false;

        if (requireCart && !completedTasks.Contains(TASK_CART))
            allCompleted = false;

        if (requireLight && !completedTasks.Contains(TASK_LIGHT))
            allCompleted = false;

        if (allCompleted)
        {
            CompleteTutorial();
        }
    }

    private void CompleteTutorial()
    {
        if (isFinished) return;

        isFinished = true;
        
        if (tutorialDoor != null)
        {
            tutorialDoor.Unlock();
            Debug.Log("🔓 Tutorial completato! Porta sbloccata.");
        }
    }

    private int GetRequiredTasksCount()
    {
        int count = 0;
        if (requireComputer) count++;
        if (requireCamera) count++;
        if (requireArm) count++;
        if (requireCart) count++;
        if (requireLight) count++;
        return count;
    }

    public void MarkAsComplete()
    {
        if (!isFinished)
        {
            // StopAllCoroutines();
            CompleteTutorial();
        }
    }

    public void OnComputerCompleted() => CompleteTask(TASK_COMPUTER);
    public void OnCameraCompleted() => CompleteTask(TASK_CAMERA);
    public void OnArmCompleted() => CompleteTask(TASK_ARM);
    public void OnCartCompleted() => CompleteTask(TASK_CART);
    public void OnLightCompleted() => CompleteTask(TASK_LIGHT);

    public string GetProgress()
    {
        return $"{completedTasks.Count}/{GetRequiredTasksCount()}";
    }

    public bool IsTaskCompleted(string taskId)
    {
        return completedTasks.Contains(taskId);
    }

    public bool IsFinished => isFinished;
}
