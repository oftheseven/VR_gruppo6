using UnityEngine;
using UnityEngine.Events;

public class QuestManager : MonoBehaviour
{
    private static QuestManager _instance;
    public static QuestManager instance => _instance;

    // [Header("Quest Configuration")]
    // [SerializeField] private InteractableDoor exitDoor; // Porta da sbloccare alla fine

    public enum TutorialQuest
    {
        None,           // Inizio gioco, prima di parlare con Tutor
        Lights,         // Quest 1: Modifica luci
        Camera,         // Quest 2: Modifica camera
        Slider,         // Quest 3: Crea keyframe slider
        Arm,            // Quest 4: Crea waypoint braccio
        GreenScreen,    // Quest 5: Seleziona immagine computer
        Complete        // Tutorial completato
    }

    private TutorialQuest currentQuest = TutorialQuest.None;
    public TutorialQuest CurrentQuest => currentQuest;

    public UnityEvent<TutorialQuest> OnQuestChanged;
    public UnityEvent OnTutorialComplete;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    public void StartTutorial()
    {
        if (currentQuest != TutorialQuest.None)
        {
            Debug.LogWarning("Tutorial già iniziato!");
            return;
        }

        SetQuest(TutorialQuest.Lights);
        Debug.Log("🎓 Tutorial avviato - Quest: Lights");
    }

    public void CompleteCurrentQuest()
    {
        Debug.Log($"Quest completata: {currentQuest}");

        switch (currentQuest)
        {
            case TutorialQuest.Lights:
                SetQuest(TutorialQuest.Slider);
                break;
            
            case TutorialQuest.Camera:
                SetQuest(TutorialQuest.Slider);
                break;

            case TutorialQuest.Slider:
                SetQuest(TutorialQuest.Arm);
                break;

            case TutorialQuest.Arm:
                SetQuest(TutorialQuest.GreenScreen);
                break;

            case TutorialQuest.GreenScreen:
                CompleteTutorial();
                break;

            default:
                Debug.LogWarning($"Quest {currentQuest} non gestita!");
                break;
        }
    }

    private void CompleteTutorial()
    {
        SetQuest(TutorialQuest.Complete);

        if (DirectorModeManager.instance != null)
        {
            DirectorModeManager.instance.SetDirectorModeAvailable(true);
            Debug.Log("Director Mode sbloccato!");
        }

        OnTutorialComplete?.Invoke();
        Debug.Log("Tutorial completato!");
    }

    private void SetQuest(TutorialQuest newQuest)
    {
        currentQuest = newQuest;
        OnQuestChanged?.Invoke(currentQuest);
    }

    public string GetCurrentQuestDescription()
    {
        return currentQuest switch
        {
            TutorialQuest.None => "Parla con il Tutor per iniziare",
            TutorialQuest.Lights => "Vai alle luci e modifica l'intensità",
            TutorialQuest.Camera => "Vai alla camera e modifica l'inquadratura",
            TutorialQuest.Slider => "Vai allo slider e crea un keyframe",
            TutorialQuest.Arm => "Vai al braccio meccanico e crea un waypoint",
            TutorialQuest.GreenScreen => "Vai al computer e seleziona un'immagine",
            TutorialQuest.Complete => "Tutorial completato!",
            _ => "Quest sconosciuta"
        };
    }

    public bool IsQuestActive(TutorialQuest quest)
    {
        return currentQuest == quest;
    }

    public bool IsTutorialComplete()
    {
        return currentQuest == TutorialQuest.Complete;
    }

    void Update()
    {
        if (UnityEngine.InputSystem.Keyboard.current.uKey.wasPressedThisFrame)
        {
            Debug.Log($"Debug: Completo quest {currentQuest}");
            CompleteCurrentQuest();
        }
    }
}