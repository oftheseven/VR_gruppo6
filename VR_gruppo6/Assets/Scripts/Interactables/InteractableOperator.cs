using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class InteractableOperator : MonoBehaviour
{
    [Header("Operator configuration")]
    [SerializeField] private string operatorID = "dop"; // identificatore del ruolo dell'operatore, usato per sbloccare la quest corretta

    [Header("Interaction text")]
    [SerializeField] private string interactionText = "Premi E per parlare";
    
    [Header("Dialogue settings")]
    [SerializeField] private string operatorName = "Operatore";
    [SerializeField] private TextAsset firstDialogueFile; // prima visita
    [SerializeField] private TextAsset secondDialogueFile; // seconda visita (dopo camera completata)
    [SerializeField] private TextAsset thirdDialogueFile; // terza visita (dopo camera completata ma prima di luce)
    [SerializeField] private TextAsset completedDialogueFile; // dopo tutto completato

    [Header("UI reference")]
    [SerializeField] private UI_DialoguePanel dialoguePanel; // riferimento al pannello di dialogo UI

    private List<string> dialogueLines = new List<string>(); // lista per memorizzare le linee di dialogo
    private Quaternion originalRotation; // rotazione originale dell'NPC
    private float rotationSpeed = 2f; // velocità di rotazione dell'NPC
    private Coroutine rotationCoroutine;

    public string GetOperatorID() => operatorID;

    void Start()
    {
        originalRotation = transform.rotation; // salvo la rotazione originale dell'NPC
    }

    public void Interact()
    {
        LoadAppropriateDialogue();

        if (dialoguePanel != null)
        {
            StartDialogue();
        }

        HandleQuestProgress();
    }

    private void LoadAppropriateDialogue()
    {
        dialogueLines.Clear();

        TextAsset dialogueToUse = null;

        // DOP
        if (TortaInTestaManager.instance != null && operatorID == TortaInTestaManager.instance.GetOperatorID(1))
        {
            if (TortaInTestaManager.instance.IsLightQuestCompleted())
            {
                // Caso 5: Tutto completato
                dialogueToUse = completedDialogueFile;
                // Debug.Log("Dialogo: Completato");
            }
            else if (TortaInTestaManager.instance.IsLightQuestUnlocked())
            {
                // Caso 4: Luce sbloccata ma non completata (dopo seconda visita)
                dialogueToUse = thirdDialogueFile != null ? thirdDialogueFile : secondDialogueFile;
                // Debug.Log("Dialogo: Luce in corso (reminder)");
            }
            else if (TortaInTestaManager.instance.IsCameraQuestCompleted())
            {
                // Caso 3: Camera completata, luce non ancora sbloccata (seconda visita)
                dialogueToUse = secondDialogueFile;
                // Debug.Log("Dialogo: Secondo (sblocca luce)");
            }
            else if (TortaInTestaManager.instance.IsCameraQuestUnlocked())
            {
                // Caso 2: Camera sbloccata ma non completata
                dialogueToUse = firstDialogueFile;
                // Debug.Log("Dialogo: Primo (reminder camera)");
            }
            else
            {
                // Caso 1: Prima visita, camera non ancora sbloccata
                dialogueToUse = firstDialogueFile;
                // Debug.Log("Dialogo: Primo (sblocca camera)");
            }
        }
        // ASSISTENTE REGIA
        else if (operatorID == TortaInTestaManager.instance.GetOperatorID(2))
        {
            if (TortaInTestaManager.instance.IsArmQuestCompleted())
            {
                dialogueToUse = completedDialogueFile;
                // Debug.Log("[OP2] Dialogo: Completato");
            }
            else if (TortaInTestaManager.instance.IsArmQuestUnlocked())
            {
                dialogueToUse = thirdDialogueFile != null ? thirdDialogueFile : secondDialogueFile;
                // Debug.Log("[OP2] Dialogo: Arm in corso (reminder)");
            }
            else if (TortaInTestaManager.instance.IsComputerQuestCompleted())
            {
                dialogueToUse = secondDialogueFile;
                // Debug.Log("[OP2] Dialogo: Secondo (sblocca arm)");
            }
            else if (TortaInTestaManager.instance.IsComputerQuestUnlocked())
            {
                dialogueToUse = firstDialogueFile;
                // Debug.Log("[OP2] Dialogo: Primo (reminder computer)");
            }
            else
            {
                dialogueToUse = firstDialogueFile;
                // Debug.Log("[OP2] Dialogo: Primo (sblocca computer)");
            }
        }
        else
        {
            dialogueToUse = firstDialogueFile;
        }

        if (dialogueToUse != null)
        {
            string[] lines = dialogueToUse.text.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();
                if (!string.IsNullOrEmpty(trimmedLine))
                {
                    dialogueLines.Add(trimmedLine);
                }
            }
        }
        else
        {
            Debug.LogError("Nessun file di dialogo assegnato a " + operatorName);
        }
    }

    private void HandleQuestProgress()
    {
        if (TortaInTestaManager.instance == null)
        {
            return;
        }

        // DOP
        if (operatorID == TortaInTestaManager.instance.GetOperatorID(1))
        {
            if (!TortaInTestaManager.instance.IsCameraQuestUnlocked())
            {
                TortaInTestaManager.instance.OnOperatorFirstInteraction(operatorID);
            }
            else if (TortaInTestaManager.instance.IsCameraQuestCompleted() && !TortaInTestaManager.instance.IsLightQuestUnlocked())
            {
                TortaInTestaManager.instance.OnOperatorSecondInteraction(operatorID);
            }
        }
        // ASSISTENTE REGIA
        else if (operatorID == TortaInTestaManager.instance.GetOperatorID(2))
        {
            if (!TortaInTestaManager.instance.IsComputerQuestUnlocked())
            {
                TortaInTestaManager.instance.OnOperatorFirstInteraction(operatorID);
                // Debug.Log("[OP2] Prima visita: sblocco computer");
            }
            else if (TortaInTestaManager.instance.IsComputerQuestCompleted() && !TortaInTestaManager.instance.IsArmQuestUnlocked())
            {
                TortaInTestaManager.instance.OnOperatorSecondInteraction(operatorID);
                // Debug.Log("[OP2] Seconda visita: sblocco arm");
            }
        }
    }

    public string GetInteractionText()
    {
        return interactionText;
    }

    private void StartDialogue()
    {
        dialoguePanel.ShowDialogue(operatorName, dialogueLines.ToArray(), this);

        // faccio girare l'NPC verso il giocatore
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
            rotationCoroutine = null;
        }
        rotationCoroutine = StartCoroutine(ReturnToOriginalRotation()); // faccio tornare l'NPC alla rotazione originale

        PlayerController.EnableMovement(true);
        PlayerController playerController = FindAnyObjectByType<PlayerController>();
        if (playerController != null)
        {
            playerController.OnDialogueEnded();
        }
    }

    private IEnumerator SmoothLookAtPlayer()
    {
        if (PlayerController.instance == null)
        {
            yield break;
        }

        Transform playerTransform = PlayerController.instance.transform;

        Vector3 direction = PlayerController.instance.transform.position - transform.position;
        direction.y = 0;

        if (direction.sqrMagnitude < 0.01f)
        {
            yield break;
        }

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
        while (Quaternion.Angle(transform. rotation, originalRotation) > 0.5f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, originalRotation, rotationSpeed * Time.deltaTime);
            yield return null;
        }

        transform.rotation = originalRotation;
        rotationCoroutine = null;
    }
}
