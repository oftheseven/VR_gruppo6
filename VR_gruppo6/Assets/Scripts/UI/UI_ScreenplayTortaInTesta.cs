using System.Collections;
using UnityEngine;
using TMPro;

public class UI_Screenplay_TortaInTesta : MonoBehaviour
{
    private static UI_Screenplay_TortaInTesta _instance;
    public static UI_Screenplay_TortaInTesta instance => _instance;

    [Header("Quest List")]
    [SerializeField] private string[] questTexts = new string[5]
    {
        "Posiziona immagini sui computer",
        "Registra movimento braccio meccanico",
        "Configura la camera",
        "Sistema l'illuminazione",
        "Completa la registrazione"
    };

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI[] questLabels = new TextMeshProUGUI[5];

    private bool isOpen = false;
    public bool IsOpen => isOpen;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        this.gameObject.SetActive(false);
        InitializeQuestTexts();
    }

    private void InitializeQuestTexts()
    {
        for (int i = 0; i < questTexts.Length && i < questLabels.Length; i++)
        {
            if (questLabels[i] != null)
            {
                questLabels[i].text = questTexts[i];
                questLabels[i].gameObject.SetActive(true);
            }
        }
    }

    public void OpenScreenplay()
    {
        this.gameObject.SetActive(true);
        PlayerController.EnableMovement(false);
        // if (AudioManager.instance != null)
        // {
        //     AudioManager.instance.PlayUIScreenPlay();
        // }
        StartCoroutine(CooldownCoroutine());
    }

    public void CloseScreenplay()
    {
        this.gameObject.SetActive(false);
        PlayerController.EnableMovement(true);
        isOpen = false;
    }

    private IEnumerator CooldownCoroutine()
    {
        yield return new WaitForSeconds(0.1f);
        isOpen = true;
    }
}