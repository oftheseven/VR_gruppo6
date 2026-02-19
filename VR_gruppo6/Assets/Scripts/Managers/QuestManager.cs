using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using TMPro;

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

    [Header("Quest completed image")]
    [SerializeField] private RawImage questCompletedImage; // immagine mostrata quando si completa una quest (es. "Quest Completed!")
    private TextMeshProUGUI questCompletedImageText = null;

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
    [Header("Divination lights")]
    [SerializeField] private InteractableLight[] divinationLights;
    public InteractableLight[] DivinationLights => divinationLights;

    [Header("Divination lens index")]
    [SerializeField] private int divinationLensIndex = 0;
    public int DivinationLensIndex => divinationLensIndex;

    [Header("Divination arm accuracy requirement")]
    [SerializeField] private float armAccuracy = 0.8f;
    public float ArmAccuracy => armAccuracy;
    
    // VARIABILI GENERALI
    private MainQuest currentQuest = MainQuest.None; // iniziamo dalla prima quest introduttiva
    public MainQuest CurrentQuest => currentQuest;

    private UnityEvent<MainQuest> OnQuestChanged;

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
        questCompletedImageText = questCompletedImage.GetComponentInChildren<TextMeshProUGUI>();
        questCompletedImage.gameObject.SetActive(false);
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

        if (questCompletedImage != null)
        {
            StartCoroutine(FadeInAndOut(questCompletedImage, 3f));
        }
    }

    public void TutorConfirmedQuestAdvance()
    {
        awaitingTutorConfirm = false;
        if (currentQuest == MainQuest.DivinationComplete)
        {
            UI_EndPanel.instance?.ShowEndPanel();
            return;
        }
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
                ShowSalottoActors();
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
                DirectorModeManager.instance.SetDirectorModeAvailable(true);
                break;

            // DIVINATION
            case MainQuest.LivingRoomComplete:
                SetQuest(MainQuest.DivinationLight);
                ShowDivinationActors();
                break;
            case MainQuest.DivinationLight:
                SetQuest(MainQuest.DivinationCamera); 
                break;
            case MainQuest.DivinationCamera:
                SetQuest(MainQuest.DivinationArm);
                break;
            case MainQuest.DivinationArm:
                SetQuest(MainQuest.DivinationGreenScreen);
                break;
            case MainQuest.DivinationGreenScreen:
                SetQuest(MainQuest.DivinationComplete);
                DirectorModeManager.instance.SetDirectorModeAvailable(true);
                break;

            default:
                Debug.LogWarning($"Quest {currentQuest} non gestita!");
                break;
        }
    }

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

            MainQuest.LivingRoomLight => "Imposta le luci del salotto all'80% e a 5000K",
            MainQuest.LivingRoomCamera => "Monta il 24mm sulla camera del salotto",
            MainQuest.LivingRoomSlider => "Fai una breve carrellata con lo slider del salotto",
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

    public void ShowMessage(string text)
    {
        questCompletedImageText.text = text;
        StartCoroutine(FadeInAndOut(questCompletedImage, 3f));
    }

    private IEnumerator FadeInAndOut(RawImage image, float duration)
    {
        if (image == null) yield break;

        float timer = 0f;
        Color color = image.color;
        color.a = 0f;
        image.color = color;
        image.gameObject.SetActive(true);
        while (timer < duration / 2f)
        {
            color.a = Mathf.Lerp(0f, 1f, timer / (duration / 2f));
            image.color = color;
            timer += Time.deltaTime;
            yield return null;
        }
        color.a = 1f;
        image.color = color;

        yield return new WaitForSeconds(1.5f);

        timer = 0f;
        while (timer < duration / 2f)
        {
            color.a = Mathf.Lerp(1f, 0f, timer / (duration / 2f));
            image.color = color;
            timer += Time.deltaTime;
            yield return null;
        }
        color.a = 0f;
        image.color = color;
        image.gameObject.SetActive(false);
        questCompletedImageText.text = "INCARICO COMPLETATO!";
    }

    private void ShowSalottoActors() 
    {
        DirectorModeManager.instance?.SetActorsActive(DirectorModeManager.instance.SalottoActors, true);
    }
    private void ShowDivinationActors() 
    {
        DirectorModeManager.instance?.SetActorsActive(DirectorModeManager.instance.DivinationActors, true);
    }
}