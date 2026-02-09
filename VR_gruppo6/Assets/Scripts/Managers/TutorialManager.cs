using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

// script per gestire la fase di tutorial
public class TutorialManager : MonoBehaviour
{
    // singleton
    private static TutorialManager _instance;
    public static TutorialManager instance => _instance;

    [Header("Scene door reference")]
    [SerializeField] private InteractableDoor tutorialDoor; // porta della scena tutorial

    [Header("Tutorial objects")]
    [SerializeField] private bool requireComputer = true;
    [SerializeField] private bool requireCamera = true;
    [SerializeField] private bool requireArm = true;
    [SerializeField] private bool requireCart = true;
    [SerializeField] private bool requireLight = true;

    private HashSet<string> completedTutorialTasks = new HashSet<string>();
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
    }

    void Start()
    {
        if (tutorialDoor != null)
        {
            tutorialDoor.Lock();
        }
    }

    void Update()
    {
        if (Keyboard.current.tKey.isPressed && SceneManager.GetActiveScene().name=="Tutorial")
        {
            Debug.Log("Porta Silente sbloccata");
            tutorialDoor.Unlock();
        }
    }

    public void CompleteTask(string taskId)
    {
        if (isFinished || completedTutorialTasks.Contains(taskId))
        {
            return;
        }

        completedTutorialTasks.Add(taskId);
        
        Debug.Log($"✅ Task '{taskId}' completato! ({completedTutorialTasks.Count}/{GetRequiredTasksCount()})");

        CheckTutorialCompletion();
    }

    private void CheckTutorialCompletion()
    {
        bool allCompleted = true;

        if (requireComputer && !completedTutorialTasks.Contains(TASK_COMPUTER))
            allCompleted = false;

        if (requireCamera && !completedTutorialTasks.Contains(TASK_CAMERA))
            allCompleted = false;

        if (requireArm && !completedTutorialTasks.Contains(TASK_ARM))
            allCompleted = false;

        if (requireCart && !completedTutorialTasks.Contains(TASK_CART))
            allCompleted = false;

        if (requireLight && !completedTutorialTasks.Contains(TASK_LIGHT))
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
        return $"{completedTutorialTasks.Count}/{GetRequiredTasksCount()}";
    }

    public bool IsTaskCompleted(string taskId)
    {
        return completedTutorialTasks.Contains(taskId);
    }

    public bool IsFinished => isFinished;
}
