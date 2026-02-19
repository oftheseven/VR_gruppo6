using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class InteractableOperator : MonoBehaviour, IDialogueSource
{
    [Header("Operator configuration")]
    [SerializeField] private string operatorID = "dop"; // identificatore del ruolo dell'operatore, usato per sbloccare la quest corretta

    [Header("Interaction text")]
    [SerializeField] private string interactionText = "Premi E per parlare";
    
    [Header("Dialogue settings")]
    [SerializeField] private string operatorName = "Operatore";
    [SerializeField] private TextAsset dialogueFile;
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
    }

    private void LoadAppropriateDialogue()
    {
        dialogueLines.Clear();

        if (dialogueFile != null)
        {
            string[] lines = dialogueFile.text.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);

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
        PlayerController.instance.OnDialogueStarted();
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
