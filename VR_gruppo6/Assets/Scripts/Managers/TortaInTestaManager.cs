using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class TortaInTestaManager : MonoBehaviour
{
    private static TortaInTestaManager _instance;
    public static TortaInTestaManager instance => _instance;

    private InteractableDoor exitDoor;
    private HashSet<string> completedTasks = new HashSet<string>();

    [Header("Scene Setup")]
    [SerializeField] private string exitDoorName = "PortaDivination";
    [SerializeField] private int totalQuests = 6;

    [Header("OPERATOR 1: Camera lens quest + Light quest configuration")]
    [SerializeField] private string dopOperatorID = "direttore_fotografia";
    [SerializeField] private int correctLensIndex = 2;
    [SerializeField] private float requiredTemperature = 5000f;
    [SerializeField] private float requiredIntensity = 40f;
    [SerializeField] private float temperatureTolerance = 200f;
    [SerializeField] private float intensityTolerance = 0.5f;

    // CAMERA LENS + LIGHT QUEST TRACKING
    private bool cameraLensQuestUnlocked = false;
    private bool cameraLensQuestCompleted = false;
    private bool lightConfigQuestUnlocked = false;
    private bool lightConfigQuestCompleted = false;

    [Header("OPERATOR 2: Greenscreen quest + Arm quest configuration")]
    [SerializeField] private string operator2ID = "assistente_regia";
    [SerializeField] private int computersRequired = 2;
    [SerializeField] private float requiredArmAccuracy = 80f; 

    // COMPUTER QUEST TRACKING
    private HashSet<string> completedComputers = new HashSet<string>();
    private bool computerQuestUnlocked = false;
    private bool computerQuestCompleted = false;
    private bool armQuestUnlocked = false;
    private bool armQuestCompleted = false;
    

    private bool isFinished = false;

    private const string TASK_COMPUTER = "computer";
    private const string TASK_ARM = "arm";
    private const string TASK_CAMERA = "camera";
    private const string TASK_LIGHT = "light";
    private const string TASK_OPERATOR_DOP = "operator_dop";
    private const string TASK_OPERATOR2 = "operator2";

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

    // PRIMA VISITA ALL'OPERATORE (SIA DOP CHE ASSISTENTE REGIA)
    public void OnOperatorFirstInteraction(string operatorID)
    {
        if (operatorID == dopOperatorID)
        {
            // DOP logic
            if (cameraLensQuestUnlocked) return;
            cameraLensQuestUnlocked = true;
            // Debug.Log($"[DOP] Quest Camera sbloccata!");
        }
        else if (operatorID == operator2ID)
        {
            // OPERATORE 2 logic: sblocca greenscreen (computer)
            if (computerQuestUnlocked) return;
            computerQuestUnlocked = true;
            // Debug.Log($"[OP2] Quest Computer (Greenscreen) sbloccata! Imposta {computersRequired} greenscreen");
        }
    }

    // SECONDA VISITA ALL'OPERATORE (DOP: dopo aver completato la quest Camera, assistente regia: dopo aver completato la quest Computer)
    public void OnOperatorSecondInteraction(string operatorID)
    {
        if (operatorID == dopOperatorID)
        {
            // DOP logic
            if (!cameraLensQuestCompleted) return;
            if (lightConfigQuestUnlocked) return;
            lightConfigQuestUnlocked = true;
            // Debug.Log($"[DOP] Quest Luce sbloccata!");
            CompleteTask(TASK_OPERATOR_DOP);
        }
        else if (operatorID == operator2ID)
        {
            // OPERATORE 2 logic: sblocca quest Arm solo se quest Computer già completata
            if (!computerQuestCompleted)
            {
                // Debug.Log("Completa prima i greenscreen!");
                return;
            }
            if (armQuestUnlocked) return;
            armQuestUnlocked = true;
            // Debug.Log($"[OP2] Quest Arm sbloccata! Ottieni almeno {requiredArmAccuracy}% accuracy");
            CompleteTask(TASK_OPERATOR2);
        }
    }

    // DOP LENS QUEST
    public void OnCameraLensSelected(int lensIndex)
    {
        if (cameraLensQuestCompleted || !cameraLensQuestUnlocked) return;

        if (lensIndex == correctLensIndex)
        {
            cameraLensQuestCompleted = true;
            // Debug.Log($"[DOP] LENTE CORRETTA! Torna dal DOP");
            CompleteTask(TASK_CAMERA);
        }
    }

    // DOP LIGHT QUEST
    public void OnLightValuesChanged(float temperature, float intensity, bool isTemperatureMode)
    {
        if (lightConfigQuestCompleted || !lightConfigQuestUnlocked || !isTemperatureMode) return;

        bool tempOK = Mathf.Abs(temperature - requiredTemperature) <= temperatureTolerance;
        bool intensityOK = Mathf.Abs(intensity - requiredIntensity) <= intensityTolerance;

        if (tempOK && intensityOK)
        {
            lightConfigQuestCompleted = true;
            // Debug.Log($"[DOP] CONFIGURAZIONE LUCE CORRETTA!");
            CompleteTask(TASK_LIGHT);
        }
    }

    // OPERATORE 2 GREENSCREEN QUEST
    public void OnComputerImageCorrect(string computerID)
    {
        if (computerQuestCompleted)
        {
            // Debug.Log("Quest Computer già completata");
            return;
        }

        if (!computerQuestUnlocked)
        {
            // Debug.Log("Quest Computer non ancora sbloccata! Parla con l'Assistente Regia");
            return;
        }

        if (completedComputers.Contains(computerID))
        {
            // Debug.Log($"Computer '{computerID}' già completato");
            return;
        }

        completedComputers.Add(computerID);
        // Debug.Log($"[OP2] Computer '{computerID}' completato! ({completedComputers.Count}/{computersRequired})");

        if (completedComputers.Count >= computersRequired)
        {
            computerQuestCompleted = true;
            // Debug.Log($"[OP2] Tutti i greenscreen completati! Torna dall'Assistente Regia");
            CompleteTask(TASK_COMPUTER);
        }
    }

    // OPERATORE 2 ARM ACCURACY
    public void OnArmAccuracyAchieved(float accuracy)
    {
        if (armQuestCompleted)
        {
            // Debug.Log("Quest Arm già completata");
            return;
        }

        if (!armQuestUnlocked)
        {
            // Debug.Log("Quest Arm non ancora sbloccata! Parla con l'Assistente Regia");
            return;
        }

        // Debug.Log($"[OP2] Accuracy braccio: {accuracy:F1}% (richiesta: {requiredArmAccuracy}%)");

        if (accuracy >= requiredArmAccuracy)
        {
            armQuestCompleted = true;
            // Debug.Log($"[OP2] ACCURACY RAGGIUNTA! Quest Arm completata!");
            CompleteTask(TASK_ARM);
        }
        else
        {
            // Debug.Log($"[OP2] Accuracy troppo bassa! Serve almeno {requiredArmAccuracy}%");
        }
    }

    public void CompleteTask(string taskId)
    {
        if (isFinished || completedTasks.Contains(taskId))
        {
            // Debug.Log($"Task '{taskId}' già completata o scena finita");
            return;
        }

        completedTasks.Add(taskId);
        // Debug.Log($"Task '{taskId}' completata! ({completedTasks.Count}/{totalQuests})");

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

    // GETTERS DOP
    public bool IsCameraQuestUnlocked() => cameraLensQuestUnlocked;
    public bool IsCameraQuestCompleted() => cameraLensQuestCompleted;
    public bool IsLightQuestUnlocked() => lightConfigQuestUnlocked;
    public bool IsLightQuestCompleted() => lightConfigQuestCompleted;
    public int GetCorrectLensIndex() => correctLensIndex;

    // GETTERS OPERATORE 2
    public bool IsComputerQuestUnlocked() => computerQuestUnlocked;
    public bool IsComputerQuestCompleted() => computerQuestCompleted;
    public bool IsArmQuestUnlocked() => armQuestUnlocked;
    public bool IsArmQuestCompleted() => armQuestCompleted;
    public float GetRequiredArmAccuracy() => requiredArmAccuracy;

    public string GetOperatorID(int operatorIndex)
    {
        switch (operatorIndex)
        {
            case 1: return dopOperatorID;
            case 2: return operator2ID;
            default: return "";
        }
    }

    public string GetProgress() => $"{completedTasks.Count}/{totalQuests}";
    public string GetComputerProgress() => $"{completedComputers.Count}/{computersRequired}";
    public bool IsTaskCompleted(string taskId) => completedTasks.Contains(taskId);
    public bool IsFinished => isFinished;
}