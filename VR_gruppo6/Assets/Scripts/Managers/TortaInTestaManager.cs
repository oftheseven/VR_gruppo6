using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class TortaInTestaManager : MonoBehaviour
{
    private static TortaInTestaManager _instance;
    public static TortaInTestaManager instance => _instance;

    [Header("Scene Setup")]
    [SerializeField] private string exitDoorName = "PortaDivination";
    [SerializeField] private int totalQuests = 8;

    [Header("Computer Quest Configuration")]
    [SerializeField] private int computersRequired = 2;

    [Header("Camera Lens Quest Configuration")]
    [SerializeField] private string requiredOperatorID = "direttore_fotografia";
    [SerializeField] private int correctLensIndex = 2;

    [Header("Light Quest Configuration")]
    [SerializeField] private float requiredTemperature = 5000f;
    [SerializeField] private float requiredIntensity = 40f;
    [SerializeField] private float temperatureTolerance = 200f;
    [SerializeField] private float intensityTolerance = 0.5f;

    private InteractableDoor exitDoor;
    private HashSet<string> completedTasks = new HashSet<string>();

    // COMPUTER QUEST TRACKING
    private HashSet<string> completedComputers = new HashSet<string>();
    private bool computerQuestCompleted = false;

    // CAMERA LENS + LIGHT QUEST TRACKING
    private bool cameraLensQuestUnlocked = false;
    private bool cameraLensQuestCompleted = false;
    private bool lightConfigQuestUnlocked = false;
    private bool lightConfigQuestCompleted = false;

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

    // PRIMA VISIITA AL DOP
    public void OnOperatorFirstInteraction(string operatorID)
    {
        if (operatorID != requiredOperatorID)
        {
            Debug.Log($"Operatore '{operatorID}' non è il DOP");
            return;
        }

        if (cameraLensQuestUnlocked)
        {
            Debug.Log("Quest Camera già sbloccata");
            return;
        }

        cameraLensQuestUnlocked = true;
        Debug.Log($"Quest Camera sbloccata! Vai a impostare la lente corretta");
    }

    // SECONDA VISITA AL DOP
    public void OnOperatorSecondInteraction(string operatorID)
    {
        if (operatorID != requiredOperatorID)
        {
            return;
        }

        if (!cameraLensQuestCompleted)
        {
            Debug.Log("Completa prima la quest Camera!");
            return;
        }

        if (lightConfigQuestUnlocked)
        {
            Debug.Log("Quest Luce già sbloccata");
            return;
        }

        lightConfigQuestUnlocked = true;
        Debug.Log($"Quest Luce sbloccata! Imposta temperatura {requiredTemperature}K e intensità {requiredIntensity}");
        
        CompleteTask(TASK_OPERATOR);
    }

    public void OnCameraLensSelected(int lensIndex)
    {
        if (cameraLensQuestCompleted)
        {
            Debug.Log("Quest Camera lente già completata");
            return;
        }

        if (!cameraLensQuestUnlocked)
        {
            Debug.Log("Quest Camera non ancora sbloccata! Parla prima con il DOP");
            return;
        }

        Debug.Log($"Lente selezionata: {lensIndex}");

        if (lensIndex == correctLensIndex)
        {
            cameraLensQuestCompleted = true;
            Debug.Log($"LENTE CORRETTA! Torna dal DOP per la prossima quest");
            CompleteTask(TASK_CAMERA);
        }
        else
        {
            Debug.Log($"Lente sbagliata! Richiesta: indice {correctLensIndex}");
        }
    }

    public void OnLightValuesChanged(float temperature, float intensity, bool isTemperatureMode)
    {
        if (lightConfigQuestCompleted)
        {
            return;
        }

        if (!lightConfigQuestUnlocked)
        {
            return;
        }

        if (!isTemperatureMode)
        {
            return;
        }

        bool tempOK = Mathf.Abs(temperature - requiredTemperature) <= temperatureTolerance;
        bool intensityOK = Mathf.Abs(intensity - requiredIntensity) <= intensityTolerance;

        if (tempOK && intensityOK)
        {
            lightConfigQuestCompleted = true;
            Debug.Log($"CONFIGURAZIONE LUCE CORRETTA! Quest Luce completata!");
            CompleteTask(TASK_LIGHT);
        }
    }

    public void OnComputerImageCorrect(string computerID)
    {
        if (computerQuestCompleted)
        {
            Debug.Log("Quest Computer già completata");
            return;
        }

        if (completedComputers.Contains(computerID))
        {
            Debug.Log($"Computer '{computerID}' già completato");
            return;
        }

        completedComputers.Add(computerID);
        Debug.Log($"Computer '{computerID}' completato! ({completedComputers.Count}/{computersRequired})");

        if (completedComputers.Count >= computersRequired)
        {
            computerQuestCompleted = true;
            CompleteTask(TASK_COMPUTER);
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

    // public void OnComputerCompleted() => CompleteTask(TASK_COMPUTER);
    public void OnArmCompleted() => CompleteTask(TASK_ARM);
    // public void OnCameraCompleted() => CompleteTask(TASK_CAMERA);
    // public void OnLightCompleted() => CompleteTask(TASK_LIGHT);
    public void OnSliderCompleted() => CompleteTask(TASK_SLIDER);
    public void OnCartCompleted() => CompleteTask(TASK_CART);
    // public void OnOperatorCompleted() => CompleteTask(TASK_OPERATOR);

    public bool IsCameraQuestUnlocked() => cameraLensQuestUnlocked;
    public bool IsCameraQuestCompleted() => cameraLensQuestCompleted;
    public bool IsLightQuestUnlocked() => lightConfigQuestUnlocked;
    public bool IsLightQuestCompleted() => lightConfigQuestCompleted;
    public int GetCorrectLensIndex() => correctLensIndex;
    public string GetRequiredOperatorID() => requiredOperatorID;

    public string GetProgress() => $"{completedTasks.Count}/{totalQuests}";
    public string GetComputerProgress() => $"{completedComputers.Count}/{computersRequired}";
    public bool IsTaskCompleted(string taskId) => completedTasks.Contains(taskId);
    public bool IsFinished => isFinished;
}