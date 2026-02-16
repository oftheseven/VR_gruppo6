using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class InteractableTutor : MonoBehaviour, IDialogueSource
{
    [Header("Tutor Configuration")]
    [SerializeField] private string tutorName = "Tutor";
    [SerializeField] private string interactionText = "Premi E per parlare con il Tutor";

    [Header("Dialogue Files")]
    [SerializeField] private TextAsset introDialogue;              // Prima volta (avvia tutorial)
    [SerializeField] private TextAsset lightsAssignedDialogue;     // Dopo intro, assegna lights
    // [SerializeField] private TextAsset lightsCompleteDialogue;     // Lights completata
    [SerializeField] private TextAsset sliderAssignedDialogue;     // Assegna slider
    // [SerializeField] private TextAsset sliderCompleteDialogue;     // Slider completata
    [SerializeField] private TextAsset armAssignedDialogue;        // Assegna arm
    // [SerializeField] private TextAsset armCompleteDialogue;        // Arm completata
    [SerializeField] private TextAsset greenscreenAssignedDialogue;// Assegna greenscreen
    [SerializeField] private TextAsset tutorialCompleteDialogue;   // Tutorial finito
    [SerializeField] private TextAsset reminderDialogue;           // Reminder generico

    [Header("UI Reference")]
    [SerializeField] private UI_DialoguePanel dialoguePanel;

    private Quaternion originalRotation;
    private Coroutine rotationCoroutine;
    private QuestManager.TutorialQuest lastSeenQuest = QuestManager.TutorialQuest.None;

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
        
        // Determina quale dialogo mostrare
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

        // Aggiorna stato quest se necessario
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
                return reminderDialogue;
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
                return reminderDialogue;
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
                return reminderDialogue;
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
                return reminderDialogue;
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

        if (rotationCoroutine != null)
        {
            StopCoroutine(rotationCoroutine);
        }
        rotationCoroutine = StartCoroutine(SmoothLookAtPlayer());

        PlayerController.EnableMovement(false);
    }

    public void OnDialogueEnd()
    {
        if (rotationCoroutine != null)
        {
            StopCoroutine(rotationCoroutine);
        }
        rotationCoroutine = StartCoroutine(ReturnToOriginalRotation());

        PlayerController.EnableMovement(true);
        
        if (PlayerController.instance != null)
        {
            PlayerController.instance.OnDialogueEnded();
        }
    }

    private IEnumerator SmoothLookAtPlayer()
    {
        if (PlayerController.instance == null) yield break;

        Vector3 direction = PlayerController.instance.transform.position - transform.position;
        direction.y = 0;

        if (direction.sqrMagnitude < 0.01f) yield break;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.5f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 2f);
            yield return null;
        }

        transform.rotation = targetRotation;
    }

    private IEnumerator ReturnToOriginalRotation()
    {
        while (Quaternion.Angle(transform.rotation, originalRotation) > 0.5f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, originalRotation, Time.deltaTime * 2f);
            yield return null;
        }

        transform.rotation = originalRotation;
        rotationCoroutine = null;
    }

    public string GetInteractionText()
    {
        return interactionText;
    }
}