using TMPro;
using UnityEngine;

public class UI_DialoguePanel : MonoBehaviour
{
    // singleton
    private static UI_DialoguePanel _instance;
    public static UI_DialoguePanel instance => _instance;
    private bool isOpen = false;
    public bool IsOpen => isOpen;
    public TextMeshProUGUI dialogueText;

    public void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }
    void Start()
    {
        this.gameObject.SetActive(false);
        dialogueText = this.GetComponentInChildren<TextMeshProUGUI>();
    }
    public void OpenDialogue()
    {
        this.gameObject.SetActive(true);
        PlayerController.EnableMovement(false);
        isOpen = true;
    }

    public void CloseDialogue()
    {
        this.gameObject.SetActive(false);
        PlayerController.EnableMovement(true);
        isOpen = false;
    }
}
