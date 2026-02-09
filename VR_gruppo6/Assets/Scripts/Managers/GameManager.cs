using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    // singleton
    private static GameManager _instance;
    public static GameManager instance => _instance;

    [Header("Dont destroy on load objects")]
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject allCanvas;
    [SerializeField] private GameObject[] doors;

    [Header("TortaInTesta door settings")]
    [SerializeField] private string tortaInTestaSceneName = "TortaInTesta";
    [SerializeField] private string exitDoorName = "PortaDivination";

    [Header("TortaInTesta objects")]
    [SerializeField] private bool requireComputer = true;
    [SerializeField] private bool requireCamera = true;
    [SerializeField] private bool requireArm = true;
    [SerializeField] private bool requireCart = true;
    [SerializeField] private bool requireLight = true;
    [SerializeField] private bool requireOperator = true;
    [SerializeField] private bool requireSlider = true;

    private HashSet<string> completedGameTasks = new HashSet<string>();
    private bool isFinished = false;
    private InteractableDoor exitDoor = null;

    private const string TASK_COMPUTER = "computer";
    private const string TASK_CAMERA = "camera";
    private const string TASK_ARM = "arm";
    private const string TASK_CART = "cart";
    private const string TASK_LIGHT = "light";
    private const string TASK_OPERATOR = "operator";
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
            DontDestroyOnLoad(transform.root.gameObject);
            DontDestroyOnLoad(allCanvas);
            DontDestroyOnLoad(EventSystem.current.gameObject);
            foreach (GameObject door in doors)
            {
                DontDestroyOnLoad(door);
            }
        }
    }

    void Update()
    {
        if (Keyboard.current.tKey.isPressed && SceneManager.GetActiveScene().name=="TortaInTesta")
        {
            Debug.Log("Porta Silente sbloccata");
            exitDoor.Unlock();
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == tortaInTestaSceneName)
        {
            FindAndLockExitDoor();
        }
    }

    private void FindAndLockExitDoor()
    {
        GameObject doorObj = GameObject.Find(exitDoorName);
        
        if (doorObj != null)
        {
            exitDoor = doorObj.GetComponentInChildren<InteractableDoor>();
        }

        if (exitDoor != null)
        {
            exitDoor.Lock();
            Debug.Log($"🔒 Porta di uscita '{exitDoor.name}' bloccata in TortaInTesta");
        }
        else
        {
            Debug.LogWarning("⚠️ Porta di uscita non trovata in TortaInTesta!");
        }
    }

    public void CompleteTask(string taskId)
    {
        if (isFinished || completedGameTasks.Contains(taskId))
        {
            return;
        }

        completedGameTasks.Add(taskId);
        
        Debug.Log($"Task '{taskId}' completato! ({completedGameTasks.Count}/{GetRequiredTasksCount()})");

        CheckSceneCompletion();
    }

    private void CheckSceneCompletion()
    {
        bool allCompleted = true;

        if (requireComputer && !completedGameTasks.Contains(TASK_COMPUTER))
            allCompleted = false;

        if (requireCamera && !completedGameTasks.Contains(TASK_CAMERA))
            allCompleted = false;

        if (requireArm && !completedGameTasks.Contains(TASK_ARM))
            allCompleted = false;

        if (requireSlider && !completedGameTasks.Contains(TASK_SLIDER))
            allCompleted = false;

        if (requireCart && !completedGameTasks.Contains(TASK_CART))
            allCompleted = false;

        if (requireLight && !completedGameTasks.Contains(TASK_LIGHT))
            allCompleted = false;

        if (requireOperator && !completedGameTasks.Contains(TASK_OPERATOR))
            allCompleted = false;

        if (allCompleted)
        {
            CompleteScene();
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
        if (requireOperator) count++;
        if (requireSlider) count++;
        return count;
    }

    private void CompleteScene()
    {
        if (isFinished) return;

        isFinished = true;

        if (exitDoor != null)
        {
            exitDoor.Unlock();
            Debug.Log("🎉 TortaInTesta completato! Porta di uscita sbloccata.");
        }
    }

    public void MarkAsComplete()
    {
        if (!isFinished)
        {
            CompleteScene();
        }
    }

    public void OnComputerCompleted() => CompleteTask(TASK_COMPUTER);
    public void OnCameraCompleted() => CompleteTask(TASK_CAMERA);
    public void OnArmCompleted() => CompleteTask(TASK_ARM);
    public void OnCartCompleted() => CompleteTask(TASK_CART);
    public void OnLightCompleted() => CompleteTask(TASK_LIGHT);
    public void OnOperatorCompleted() => CompleteTask(TASK_OPERATOR);

    public string GetProgress()
    {
        return $"{completedGameTasks.Count}/{GetRequiredTasksCount()}";
    }

    public bool IsTaskCompleted(string taskId)
    {
        return completedGameTasks.Contains(taskId);
    }

    public bool IsFinished => isFinished;
}
