using System.Collections;
using UnityEngine;
using TMPro;

public class UI_Screenplay_DivinationClass : MonoBehaviour
{
    private static UI_Screenplay_DivinationClass _instance;
    public static UI_Screenplay_DivinationClass instance => _instance;

    [Header("Quest List")]
    [SerializeField] private string[] questTexts = new string[6]
    {
        "Registra movimento slider",
        "Imposta luci",
        "Quest Divination 3",
        "Quest Divination 4",
        "Quest Divination 5",
        "Quest Divination 6"
    };

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI[] questLabels = new TextMeshProUGUI[6];

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