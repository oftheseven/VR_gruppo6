using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class InteractableTutor : MonoBehaviour, IDialogueSource
{
    // singleton
    private static InteractableTutor _instance;
    public static InteractableTutor instance => _instance;

    [Header("Tutor Configuration")]
    [SerializeField] private string tutorName = "Tutor Remy";
    [SerializeField] private string interactionText = "[E] per parlare con il Tutor";

    [Header("Tutorial dialogue files")]
    [SerializeField] private TextAsset introDialogue;
    [SerializeField] private TextAsset tutorialLightsAssignedDialogue;
    [SerializeField] private TextAsset tutorialCameraAssignedDialogue;
    [SerializeField] private TextAsset tutorialSliderAssignedDialogue;
    [SerializeField] private TextAsset tutorialArmAssignedDialogue;
    [SerializeField] private TextAsset tutorialGreenscreenAssignedDialogue;
    [SerializeField] private TextAsset tutorialCompleteDialogue;
    [SerializeField] private TextAsset reminderDialogue; // reminder generico

    [Header("Salotto dialogue files")]
    [SerializeField] private TextAsset livingRoomLightsAssignedDialogue;
    [SerializeField] private TextAsset livingRoomCameraAssignedDialogue;
    [SerializeField] private TextAsset livingRoomSliderAssignedDialogue;
    [SerializeField] private TextAsset livingRoomGreenscreenAssignedDialogue;
    [SerializeField] private TextAsset livingRoomCompleteDialogue;

    [Header("Divination dialogue files")]
    [SerializeField] private TextAsset divinationLightsAssignedDialogue;
    [SerializeField] private TextAsset divinationCameraAssignedDialogue;
    [SerializeField] private TextAsset divinationArmAssignedDialogue;
    [SerializeField] private TextAsset divinationGreenscreenAssignedDialogue;
    [SerializeField] private TextAsset divinationCompleteDialogue;

    [Header("UI reference")]
    [SerializeField] private UI_DialoguePanel dialoguePanel;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string animIsTalkingParam = "IsTalking";

    private QuestManager.MainQuest lastSeenQuest = QuestManager.MainQuest.None;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogError("Animator non trovato su InteractableTutor!");
            }
        }

        if (dialoguePanel == null)
        {
            dialoguePanel = FindFirstObjectByType<UI_DialoguePanel>();
            if (dialoguePanel == null)
            {
                Debug.LogError("UI_DialoguePanel non trovato!");
            }
        }
    }

    void Update()
    {
        // mando avanti le quest per debug
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            QuestManager.instance.AdvanceQuest();
        }
    }

    public void Interact()
    {
        if (QuestManager.instance == null)
        {
            Debug.LogError("QuestManager non trovato!");
            return;
        }

        if (QuestManager.instance.AwaitingTutorConfirm)
        {
            Debug.Log("Tutor conferma avanzamento quest!");
            QuestManager.instance.TutorConfirmedQuestAdvance();
        }

        QuestManager.MainQuest currentQuest = QuestManager.instance.CurrentQuest;
        TextAsset dialogueToUse = GetAppropriateDialogue(currentQuest);
        
        if (dialogueToUse != null)
        {
            List<string> lines = ParseDialogue(dialogueToUse);
            StartDialogue(lines);
        }
        else
        {
            Debug.LogWarning("Nessun dialogo assegnato per quest: " + currentQuest);
        }

        // CONDIZIONE DA CAMBIARE, MAGARI METTERE UN FADE-IN FADE-OUT A SCHERMO PER FAR CAPIRE CHE SI E' PASSATI ALLA FASE SUCCESSIVA
        if (currentQuest == QuestManager.MainQuest.TutorialComplete)
        {
            QuestManager.instance.AdvanceQuest(); // passo alla fase del salotto
        }

        HandleQuestProgression(currentQuest);
    }

    private TextAsset GetAppropriateDialogue(QuestManager.MainQuest currentQuest)
    {
        switch (currentQuest)
        {
            // ------------------TUTORIAL-------------------

            case QuestManager.MainQuest.None:
                return introDialogue;
            case QuestManager.MainQuest.TutorialLights:
                return tutorialLightsAssignedDialogue;
            case QuestManager.MainQuest.TutorialCamera:
                return tutorialCameraAssignedDialogue;
            case QuestManager.MainQuest.TutorialSlider:
                return tutorialSliderAssignedDialogue;
            case QuestManager.MainQuest.TutorialArm:
                return tutorialArmAssignedDialogue;
            case QuestManager.MainQuest.TutorialGreenScreen:
                return tutorialGreenscreenAssignedDialogue;
            case QuestManager.MainQuest.TutorialComplete:
                return tutorialCompleteDialogue;

            // ------------------SALOTTO-------------------

            case QuestManager.MainQuest.LivingRoomLight:
                return livingRoomLightsAssignedDialogue;
            case QuestManager.MainQuest.LivingRoomCamera:
                return livingRoomCameraAssignedDialogue;
            case QuestManager.MainQuest.LivingRoomSlider:
                return livingRoomSliderAssignedDialogue;
            case QuestManager.MainQuest.LivingRoomGreenScreen:
                return livingRoomGreenscreenAssignedDialogue;
            case QuestManager.MainQuest.LivingRoomComplete:
                return livingRoomCompleteDialogue;

            // ------------------DIVINATION-------------------

            case QuestManager.MainQuest.DivinationLight:
                return divinationLightsAssignedDialogue;
            case QuestManager.MainQuest.DivinationCamera:
                return divinationCameraAssignedDialogue;
            case QuestManager.MainQuest.DivinationArm:
                return divinationArmAssignedDialogue;
            case QuestManager.MainQuest.DivinationGreenScreen:
                return divinationGreenscreenAssignedDialogue;
            case QuestManager.MainQuest.DivinationComplete:
                return divinationCompleteDialogue;

            // ------------------REMINDER-------------------

            default:
                return reminderDialogue;
        }
    }

    private void HandleQuestProgression(QuestManager.MainQuest currentQuest)
    {
        if (currentQuest == QuestManager.MainQuest.None)
        {
            QuestManager.instance.StartTutorial();
        }

        lastSeenQuest = currentQuest;
    }

    private List<string> ParseDialogue(TextAsset dialogueFile)
    {
        List<string> lines = new List<string>();
        
        if (dialogueFile == null)
        {
            lines.Add("...");
            return lines;
        }

        string[] rawLines = dialogueFile.text.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
        
        foreach (string line in rawLines)
        {
            string trimmed = line.Trim();
            if (!string.IsNullOrEmpty(trimmed))
            {
                lines.Add(trimmed);
            }
        }

        return lines;
    }
    private void StartDialogue(List<string> lines)
    {
        if (dialoguePanel == null)
        {
            Debug.LogError("DialoguePanel è null!");
            return;
        }

        PlayerController.EnableMovement(false);

        SetTalking(true);

        dialoguePanel.ShowDialogue(tutorName, lines.ToArray(), this);
    }

    public void OnDialogueEnd()
    {
        PlayerController.EnableMovement(true);
        
        if (PlayerController.instance != null)
        {
            PlayerController.instance.OnDialogueEnded();
        }

        SetTalking(false);
    }

    private void SetTalking(bool talking)
    {
        if (animator != null && animator.isActiveAndEnabled)
        {
            animator.SetBool(animIsTalkingParam, talking);
        }
    }

    public string GetInteractionText()
    {
        return interactionText;
    }
}