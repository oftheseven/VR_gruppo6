using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;

public class UI_DialoguePanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI operatorNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject continueIndicator;

    [Header("Typing Settings")]
    [SerializeField] private float typingSpeed = 0.05f; // velocità di digitazione del testo

    private string[] currentDialogueLines;
    private int currentLineIndex = 0;
    private IDialogueSource currentDialogueSource;
    private bool isTyping = false;
    private bool dialogueActive = false;

    void Start()
    {
        this.gameObject.SetActive(false);
    }

    void Update()
    {
        if (dialogueActive)
        {
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                if (isTyping)
                {
                    StopAllCoroutines();
                    dialogueText.text = currentDialogueLines[currentLineIndex];
                    isTyping = false;
                    continueIndicator.SetActive(true);
                }
                else
                {
                    ShowNextLine();
                }
            }
        }
    }

    public void ShowDialogue(string characterName, string[] lines, IDialogueSource source)
    {
        currentDialogueLines = lines;
        currentLineIndex = 0;
        currentDialogueSource = source;
        dialogueActive = true;

        PlayerController.instance.BasePanel.gameObject.SetActive(false); // nascondo il pannello di base del player durante il dialogo
        this.gameObject.SetActive(true);
        operatorNameText.text = characterName;

        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        if (currentLineIndex < currentDialogueLines.Length)
        {
            string line = currentDialogueLines[currentLineIndex];

            StartCoroutine(TypeLine(line));
        }
        else
        {
            EndDialogue();
        }
    }

    private void ShowNextLine()
    {
        currentLineIndex++;
        ShowContinueIndicator(false);
        ShowCurrentLine();
    }

    private IEnumerator TypeLine(string line)
    {
        isTyping = true;
        dialogueText.text = "";
        ShowContinueIndicator(false);

        foreach (char letter in line.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        ShowContinueIndicator(true);
    }

    private void ShowContinueIndicator(bool show)
    {
        if (continueIndicator != null)
        {
            continueIndicator.SetActive(show);
        }
    }

    public void EndDialogue()
    {
        dialogueActive = false;
        this.gameObject.SetActive(false);

        if (currentDialogueSource != null)
        {
            currentDialogueSource.OnDialogueEnd();
        }
        currentDialogueSource = null;
        PlayerController.instance.BasePanel.gameObject.SetActive(true); // riattivo il pannello di base del player dopo il dialogo
    }
}
