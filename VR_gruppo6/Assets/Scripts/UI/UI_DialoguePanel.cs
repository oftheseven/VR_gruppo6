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
    private InteractableOperator currentOperator;
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

    public void ShowDialogue(string characterName, string[] lines, InteractableOperator operator_)
    {
        currentDialogueLines = lines;
        currentLineIndex = 0;
        currentOperator = operator_;
        dialogueActive = true;

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

        if (currentOperator != null)
        {
            currentOperator.OnDialogueEnd();
        }
    }
}
