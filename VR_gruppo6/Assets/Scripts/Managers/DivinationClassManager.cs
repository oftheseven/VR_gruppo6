using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class DivinationClassManager : MonoBehaviour
{
    private static DivinationClassManager _instance;
    public static DivinationClassManager instance => _instance;

    [Header("Scene Setup")]
    [SerializeField] private InteractableDoor exitDoor;
    [SerializeField] private int totalQuests = 6;

    private HashSet<string> completedTasks = new HashSet<string>();
    private bool isFinished = false;

    private const string TASK_COMPUTER = "computer";
    private const string TASK_CAMERA = "camera";
    private const string TASK_ARM = "arm";
    private const string TASK_LIGHT = "light";
    private const string TASK_PROP1 = "prop1";
    private const string TASK_PROP2 = "prop2";

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
        if (exitDoor != null)
        {
            exitDoor.Lock();
        }
    }

    void Update()
    {
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            Debug.Log("Debug: Porta sbloccata");
            exitDoor?.Unlock();
        }
    }

    public void CompleteTask(string taskId)
    {
        if (isFinished || completedTasks.Contains(taskId))
        {
            Debug.Log($"Task '{taskId}' già completata o scena finita");
            return;
        }

        completedTasks.Add(taskId);
        Debug.Log($"Task '{taskId}' completata! ({completedTasks.Count}/{totalQuests})");

        CheckCompletion();
    }

    private void CheckCompletion()
    {
        if (completedTasks.Count >= totalQuests)
        {
            CompleteScene();
        }
    }

    // AGGIUNGERE LA FINE DELL'APPLICAZIONE

    private void CompleteScene()
    {
        if (isFinished) return;

        isFinished = true;

        Debug.Log("DivinationClass completata!");

        if (exitDoor != null)
        {
            exitDoor.Unlock();
        }

        if (DirectorModeManager.instance != null)
        {
            DirectorModeManager.instance.SetDirectorModeAvailable(true);
        }
    }

    public void OnComputerCompleted() => CompleteTask(TASK_COMPUTER);
    public void OnCameraCompleted() => CompleteTask(TASK_CAMERA);
    public void OnArmCompleted() => CompleteTask(TASK_ARM);
    public void OnLightCompleted() => CompleteTask(TASK_LIGHT);
    public void OnProp1Completed() => CompleteTask(TASK_PROP1);
    public void OnProp2Completed() => CompleteTask(TASK_PROP2);

    public string GetProgress() => $"{completedTasks.Count}/{totalQuests}";
    public bool IsTaskCompleted(string taskId) => completedTasks.Contains(taskId);
    public bool IsFinished => isFinished;
}