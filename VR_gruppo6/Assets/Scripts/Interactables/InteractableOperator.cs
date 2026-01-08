using UnityEngine;
using System.Collections.Generic;

public class InteractableOperator : MonoBehaviour
{
    [Header("Interaction text")]
    [SerializeField] private string interactionText = "Premi E per eseguire l'interazione";
    
    [Header("Dialogue settings")]
    [SerializeField] private string operatorName = "Operatore";
    [SerializeField] private TextAsset dialogueFile; // qui viene assegnato il file di testo che contiene il dialogo dell'NPC

    [Header("UI reference")]
    [SerializeField] private UI_DialoguePanel dialoguePanel; // riferimento al pannello di dialogo UI

    private List<string> dialogueLines = new List<string>(); // lista per memorizzare le linee di dialogo

    void Start()
    {
        LoadDialogue();
    }

    private void LoadDialogue()
    {
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

            Debug.Log($"Caricato dialogo per {operatorName}: {dialogueLines.Count} righe");
        }
        else
        {
            Debug.LogError("Nessun file di dialogo assegnato a " + operatorName);
        }
    }

    public void Interact()
    {
        Debug.Log("Interazione con " + this.gameObject.name);

        if (dialoguePanel != null)
        {
            StartDialogue();
        }
    }

    public string getInteractionText()
    {
        return interactionText;
    }

    private void StartDialogue()
    {
        dialoguePanel.ShowDialogue(operatorName, dialogueLines.ToArray(), this);
        PlayerController.EnableMovement(false);
    }

    public void OnDialogueEnd()
    {
        PlayerController.EnableMovement(true);
        Debug.Log($"Dialogo con {operatorName} terminato");
    }
}
