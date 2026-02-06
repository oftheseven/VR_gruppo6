using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

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
    [SerializeField] private string exitDoorName = "Porta2";

    [Header("TortaInTesta objects")]
    [SerializeField] private bool requireComputer = true;
    [SerializeField] private bool requireCamera = true;
    [SerializeField] private bool requireArm = true;
    [SerializeField] private bool requireCart = true;
    [SerializeField] private bool requireLight = true;

    private HashSet<string> completedTasks = new HashSet<string>();
    private bool isFinished = false;
    private InteractableDoor exitDoor = null;

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
            DontDestroyOnLoad(transform.root.gameObject);
            DontDestroyOnLoad(allCanvas);
            foreach (GameObject door in doors)
            {
                DontDestroyOnLoad(door);
            }
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
        if (isFinished || completedTasks.Contains(taskId))
        {
            return;
        }

        completedTasks.Add(taskId);
        
        Debug.Log($"Task '{taskId}' completato! ({completedTasks.Count}/{GetRequiredTasksCount()})");

        CheckSceneCompletion();
    }

    private void CheckSceneCompletion()
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
