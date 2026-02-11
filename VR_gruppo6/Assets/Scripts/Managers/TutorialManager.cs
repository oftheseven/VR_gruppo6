using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class TutorialManager : MonoBehaviour
{
    private static TutorialManager _instance;
    public static TutorialManager instance => _instance;

    [Header("Scene Setup")]
    [SerializeField] private InteractableDoor exitDoor;
    [SerializeField] private int totalQuests = 6;

    private HashSet<string> completedTasks = new HashSet<string>();
    private bool isFinished = false;

    private const string TASK_COMPUTER = "computer";
    private const string TASK_CAMERA = "camera";
    private const string TASK_ARM = "arm";
    private const string TASK_CART = "cart";
    private const string TASK_LIGHT = "light";
    private const string TASK_SLIDER = "slider";

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

        if (exitDoor != null)
        {
            exitDoor.Lock();
            Debug.Log("Porta bloccata all'inizio del tutorial");
        }
        else
        {
            Debug.LogWarning("Exit door not assigned in TutorialManager.");
        }
    }

    void Update()
    {
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            Debug.Log("Debug: Porta sbloccata");
            exitDoor.Unlock();
        }
    }

    public void CompleteTask(string taskId)
    {
        Debug.Log($"CompleteTask chiamato con: '{taskId}'");

        if (isFinished)
        {
            // Debug.Log($"Task '{taskId}' già completata o tutorial finito");
            return;
        }

        if (completedTasks.Contains(taskId))
        {
            // Debug.Log($"Task '{taskId}' già completata, ignoro");
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

    private void CompleteScene()
    {
        if (isFinished) return;

        isFinished = true;

        Debug.Log("Tutorial completato!");

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
    public void OnCartCompleted() => CompleteTask(TASK_CART);
    public void OnLightCompleted() => CompleteTask(TASK_LIGHT);
    public void OnSliderCompleted() => CompleteTask(TASK_SLIDER);

    public string GetProgress() => $"{completedTasks.Count}/{totalQuests}";
    public bool IsTaskCompleted(string taskId) => completedTasks.Contains(taskId);
    public bool IsFinished => isFinished;
}