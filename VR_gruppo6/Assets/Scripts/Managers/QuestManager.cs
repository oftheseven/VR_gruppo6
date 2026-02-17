using UnityEngine;
using UnityEngine.Events;

public class QuestManager : MonoBehaviour
{
    private static QuestManager _instance;
    public static QuestManager instance => _instance;

    public enum MainQuest
    {
        // TUTORIAL
        None,
        TutorialLights,         // quest 1: modifica luci
        TutorialCamera,         // quest 2: modifica camera
        TutorialSlider,         // quest 3: crea keyframe slider
        TutorialArm,            // quest 4: crea waypoint braccio
        TutorialGreenScreen,    // quest 5: seleziona immagine computer
        TutorialComplete,       // tutorial completato

        // SALOTTO
        LivingRoomLight,        // quest 1: modifica luci salotto
        LivingRoomCamera,       // quest 2: modifica camera salotto
        LivingRoomSlider,       // quest 3: crea keyframe slider salotto
        LivingRoomGreenScreen,  // quest 4: seleziona immagine computer salotto
        LivingRoomComplete,     // salotto completato

        // DIVINATION
        DivinationLight,        // quest 1: modifica luci divination
        DivinationCamera,       // quest 2: modifica camera divination
        DivinationArm,          // quest 3: crea waypoint braccio divination
        DivinationGreenScreen,  // quest 4: seleziona immagine computer divination
        DivinationComplete,     // divination completata
    }

    // SETTAGGI PER LE QUEST DEL SALOTTO
    [Header("Living room lights")]
    [SerializeField] private InteractableLight[] livingRoomLights;
    public InteractableLight[] LivingRoomLights => livingRoomLights;

    [Header("Living room lens index")]
    [SerializeField] private int livingLensIndex = 0;
    public int LivingLensIndex => livingLensIndex;

    [Header("Living room slider time requirement")]
    [SerializeField] private float lookDurationRequired = 3.0f;
    public float LookDurationRequired => lookDurationRequired;

    // SETTAGGI PER LE QUEST DELLA DIVINATION

    private MainQuest currentQuest = MainQuest.None; // iniziamo dalla prima quest introduttiva
    public MainQuest CurrentQuest => currentQuest;

    private UnityEvent<MainQuest> OnQuestChanged;
    private UnityEvent OnTutorialComplete;

    private bool awaitingTutorConfirm = false;
    public bool AwaitingTutorConfirm => awaitingTutorConfirm;

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
        if (currentQuest != MainQuest.None)
        {
            Debug.LogWarning("Tutorial già iniziato!");
            return;
        }

        SetQuest(MainQuest.TutorialLights);
        Debug.Log("Tutorial avviato - Quest: Lights");
    }

    public void CompleteCurrentQuest()
    {
        Debug.Log($"Quest completata: {currentQuest}");

        // mostro una frase intermedia per far tornare l'utente dal tutor
        awaitingTutorConfirm = true;
    
        if (UI_BasePanel.instance != null)
        {
            UI_BasePanel.instance.ShowTalkToRemy();
        }
    }

    public void TutorConfirmedQuestAdvance()
    {
        awaitingTutorConfirm = false;
        AdvanceQuest();
    }

    public void AdvanceQuest()
    {
        Debug.Log($"Quest completata: {currentQuest}");

        switch (currentQuest)
        {
            // TUTORIAL
            case MainQuest.TutorialLights:
                SetQuest(MainQuest.TutorialCamera);
                break;
            case MainQuest.TutorialCamera:
                SetQuest(MainQuest.TutorialSlider);
                break;
            case MainQuest.TutorialSlider:
                SetQuest(MainQuest.TutorialArm);
                break;
            case MainQuest.TutorialArm:
                SetQuest(MainQuest.TutorialGreenScreen);
                break;
            case MainQuest.TutorialGreenScreen:
                SetQuest(MainQuest.TutorialComplete);
                break;
            case MainQuest.TutorialComplete:
                SetQuest(MainQuest.LivingRoomLight); // inizio quest salotto
                break;
            
            // SALOTTO
            case MainQuest.LivingRoomLight:
                SetQuest(MainQuest.LivingRoomCamera);
                break;
            case MainQuest.LivingRoomCamera:
                SetQuest(MainQuest.LivingRoomSlider);
                break;
            case MainQuest.LivingRoomSlider:
                SetQuest(MainQuest.LivingRoomGreenScreen);
                break;
            case MainQuest.LivingRoomGreenScreen:
                SetQuest(MainQuest.LivingRoomComplete); 
                break;

            // DIVINATION
            case MainQuest.LivingRoomComplete:
                SetQuest(MainQuest.DivinationLight); 
                break;
            case MainQuest.DivinationLight:
                SetQuest(MainQuest.DivinationArm); 
                break;
            case MainQuest.DivinationArm:
                SetQuest(MainQuest.DivinationGreenScreen);
                break;
            case MainQuest.DivinationGreenScreen:
                SetQuest(MainQuest.DivinationComplete);
                break;

            default:
                Debug.LogWarning($"Quest {currentQuest} non gestita!");
                break;
        }
    }

    // private void CompleteTutorial()
    // {
    //     SetQuest(MainQuest.TutorialComplete);

    //     OnTutorialComplete?.Invoke();
    //     Debug.Log("Tutorial completato!");
    // }

    private void SetQuest(MainQuest newQuest)
    {
        currentQuest = newQuest;
        OnQuestChanged?.Invoke(currentQuest);
    }

    public string GetCurrentQuestDescription()
    {
        return currentQuest switch
        {
            MainQuest.None => "Parla con il Tutor per iniziare",
            MainQuest.TutorialLights => "Fai pratica con una luce",
            MainQuest.TutorialCamera => "Fai pratica con la camera sul treppiede",
            MainQuest.TutorialSlider => "Fai pratica con lo slider facendo una breve registrazione del movimento",
            MainQuest.TutorialArm => "Fai pratica con il braccio meccanico facendo una breve registrazione del movimento",
            MainQuest.TutorialGreenScreen => "Fai pratica con il computer selezionando un'immagine per il green screen",
            MainQuest.TutorialComplete => "Tutorial completato!",

            MainQuest.LivingRoomLight => "Modifica le luci del salotto",
            MainQuest.LivingRoomCamera => "Modifica la camera del salotto",
            MainQuest.LivingRoomSlider => "Crea un keyframe per lo slider del salotto",
            MainQuest.LivingRoomGreenScreen => "Seleziona un'immagine per il computer del salotto",
            MainQuest.LivingRoomComplete => "Salotto completato!",

            MainQuest.DivinationLight => "Modifica le luci della divination",
            MainQuest.DivinationCamera => "Modifica la camera della divination",
            MainQuest.DivinationArm => "Crea un waypoint per il braccio della divination",
            MainQuest.DivinationGreenScreen => "Seleziona un'immagine per il computer della divination",
            MainQuest.DivinationComplete => "Divination completata!",

            _ => "Quest sconosciuta"
        };
    }

    public bool IsQuestActive(MainQuest quest)
    {
        return currentQuest == quest;
    }

    public bool IsTutorialComplete()
    {
        return currentQuest == MainQuest.TutorialComplete;
    }
}