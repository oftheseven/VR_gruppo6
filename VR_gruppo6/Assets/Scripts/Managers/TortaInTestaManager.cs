using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class TortaInTestaManager : MonoBehaviour
{
    private static TortaInTestaManager _instance;
    public static TortaInTestaManager instance => _instance;

    [Header("Scene Setup")]
    [SerializeField] private string exitDoorName = "PortaDivination";
    [SerializeField] private int totalQuests = 7;

    private InteractableDoor exitDoor;
    private HashSet<string> completedTasks = new HashSet<string>();
    private bool isFinished = false;

    private const string TASK_COMPUTER = "computer";
    private const string TASK_ARM = "arm";
    private const string TASK_CAMERA = "camera";
    private const string TASK_LIGHT = "light";
    private const string TASK_SLIDER = "slider";
    private const string TASK_CART = "cart";
    private const string TASK_OPERATOR = "operator";

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
        FindAndLockDoor();
    }

    void Update()
    {
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            Debug.Log("Debug: Porta sbloccata");
            exitDoor?.Unlock();
        }
    }

    private void FindAndLockDoor()
    {
        GameObject doorObj = GameObject.Find(exitDoorName);
        
        if (doorObj != null)
        {
            exitDoor = doorObj.GetComponentInChildren<InteractableDoor>();
        }

        if (exitDoor != null)
        {
            exitDoor.Lock();
            Debug.Log($"Porta '{exitDoor.name}' bloccata");
        }
        else
        {
            Debug.LogWarning($"Porta '{exitDoorName}' non trovata!");
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

    private void CompleteScene()
    {
        if (isFinished) return;

        isFinished = true;

        Debug.Log("TortaInTesta completata!");

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
    public void OnArmCompleted() => CompleteTask(TASK_ARM);
    public void OnCameraCompleted() => CompleteTask(TASK_CAMERA);
    public void OnLightCompleted() => CompleteTask(TASK_LIGHT);
    public void OnSliderCompleted() => CompleteTask(TASK_SLIDER);
    public void OnCartCompleted() => CompleteTask(TASK_CART);
    public void OnOperatorCompleted() => CompleteTask(TASK_OPERATOR);

    public string GetProgress() => $"{completedTasks.Count}/{totalQuests}";
    public bool IsTaskCompleted(string taskId) => completedTasks.Contains(taskId);
    public bool IsFinished => isFinished;
}