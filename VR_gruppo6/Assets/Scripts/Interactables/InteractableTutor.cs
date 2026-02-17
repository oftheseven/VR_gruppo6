using UnityEngine;
using System.Collections.Generic;

public class InteractableTutor : MonoBehaviour, IDialogueSource
{
    // singleton
    private static InteractableTutor _instance;
    public static InteractableTutor instance => _instance;

    [Header("Tutor Configuration")]
    [SerializeField] private string tutorName = "Tutor Remy";
    [SerializeField] private string interactionText = "[E] per parlare con il Tutor";

    [Header("Dialogue Files")]
    [SerializeField] private TextAsset introDialogue;
    [SerializeField] private TextAsset lightsAssignedDialogue;
    [SerializeField] private TextAsset lightReminderDialogue;
    [SerializeField] private TextAsset cameraAssigneDialogue;
    [SerializeField] private TextAsset cameraReminderDialogue;
    [SerializeField] private TextAsset sliderAssignedDialogue;
    [SerializeField] private TextAsset sliderReminderDialogue;
    [SerializeField] private TextAsset armAssignedDialogue;
    [SerializeField] private TextAsset armReminderDialogue;
    [SerializeField] private TextAsset greenscreenAssignedDialogue;
    [SerializeField] private TextAsset greenscreenReminderDialogue;
    [SerializeField] private TextAsset tutorialCompleteDialogue;
    [SerializeField] private TextAsset reminderDialogue; // reminder generico

    [Header("UI Reference")]
    [SerializeField] private UI_DialoguePanel dialoguePanel;

    private Quaternion originalRotation;
    private Coroutine rotationCoroutine;
    private QuestManager.TutorialQuest lastSeenQuest = QuestManager.TutorialQuest.None;

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
        originalRotation = transform.rotation;

        if (dialoguePanel == null)
        {
            dialoguePanel = FindFirstObjectByType<UI_DialoguePanel>();
            if (dialoguePanel == null)
            {
                Debug.LogError("UI_DialoguePanel non trovato!");
            }
        }
    }

    public void Interact()
    {
        if (QuestManager.instance == null)
        {
            Debug.LogError("QuestManager non trovato!");
            return;
        }

        QuestManager.TutorialQuest currentQuest = QuestManager.instance.CurrentQuest;
        
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

        HandleQuestProgression(currentQuest);
    }

    private TextAsset GetAppropriateDialogue(QuestManager.TutorialQuest currentQuest)
    {
        // prima volta (tutorial non ancora iniziato)
        if (currentQuest == QuestManager.TutorialQuest.None)
        {
            return introDialogue;
        }

        // quest Lights
        if (currentQuest == QuestManager.TutorialQuest.Lights)
        {
            // se è la prima volta che vedi questa quest, dai l'assegnazione
            if (lastSeenQuest != QuestManager.TutorialQuest.Lights)
            {
                return lightsAssignedDialogue;
            }
            else
            {
                return lightReminderDialogue;
            }
        }

        // quest camera
        if (currentQuest == QuestManager.TutorialQuest.Camera)
        {
            if (lastSeenQuest != QuestManager.TutorialQuest.Camera)
            {
                return cameraAssigneDialogue;
            }
            else
            {
                return cameraReminderDialogue;
            }
        }

        // quest slider (lights appena completata)
        if (currentQuest == QuestManager.TutorialQuest.Slider)
        {
            if (lastSeenQuest != QuestManager.TutorialQuest.Slider)
            {
                return sliderAssignedDialogue;
            }
            else
            {
                return sliderReminderDialogue;
            }
        }

        // quest arm (slider appena completata)
        if (currentQuest == QuestManager.TutorialQuest.Arm)
        {
            if (lastSeenQuest != QuestManager.TutorialQuest.Arm)
            {
                return armAssignedDialogue;
            }
            else
            {
                return armReminderDialogue;
            }
        }

        // quest greenscreen (arm appena completata)
        if (currentQuest == QuestManager.TutorialQuest.GreenScreen)
        {
            if (lastSeenQuest != QuestManager.TutorialQuest.GreenScreen)
            {
                return greenscreenAssignedDialogue;
            }
            else
            {
                return greenscreenReminderDialogue;
            }
        }

        // tutorial completato
        if (currentQuest == QuestManager.TutorialQuest.Complete)
        {
            return tutorialCompleteDialogue;
        }

        return reminderDialogue;
    }

    private void HandleQuestProgression(QuestManager.TutorialQuest currentQuest)
    {
        if (currentQuest == QuestManager.TutorialQuest.None)
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

        dialoguePanel.ShowDialogue(tutorName, lines.ToArray(), this);

        PlayerController.EnableMovement(false);
    }

    public void OnDialogueEnd()
    {
        PlayerController.EnableMovement(true);
        
        if (PlayerController.instance != null)
        {
            PlayerController.instance.OnDialogueEnded();
        }
    }

    public string GetInteractionText()
    {
        return interactionText;
    }
}