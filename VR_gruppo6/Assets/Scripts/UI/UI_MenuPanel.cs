using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UI_MenuPanel : MonoBehaviour
{
    // singleton
    private static UI_MenuPanel _menuPanelUI;
    public static UI_MenuPanel instance => _menuPanelUI;

    [Header("UI Elements")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button muteButton;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private TextMeshProUGUI masterVolumeText;
    [SerializeField] private Slider ambienceVolumeSlider;
    [SerializeField] private TextMeshProUGUI ambienceVolumeText;
    [SerializeField] private Slider soundVolumeSlider;
    [SerializeField] private TextMeshProUGUI soundVolumeText;

    private bool isOpen = false;
    public bool IsOpen => isOpen;
    private bool isMuted = false;

    void Awake()
    {
        if (_menuPanelUI != null && _menuPanelUI != this)
        {
            Destroy(gameObject);
            return;
        }
        _menuPanelUI = this;
    }

    void Start()
    {
        this.gameObject.SetActive(false); // all'avvio disattivo l'oggetto UI
        if (muteButton != null)
        {
            muteButton.onClick.AddListener(ToggleMuteAll);
        }

        SetupSliders();
    }

    public void OpenMenu()
    {
        this.gameObject.SetActive(true); // attivo l'oggetto UI se clicco il bottone di apertura
        PlayerController.EnableMovement(false); // disabilito il movimento del player quando apro il menu
        StartCoroutine(CooldownCoroutine());

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayUIOpenPanel();
        }
    }

    public void CloseMenu()
    {
        this.gameObject.SetActive(false); // disattivo l'oggetto UI se clicco il bottone di chiusura
        PlayerController.EnableMovement(true); // riabilito il movimento del player quando chiudo il menu
        isOpen = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void SetupSliders()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.minValue = 0f;
            masterVolumeSlider.maxValue = 1f;
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            masterVolumeSlider.value = AudioManager.instance != null ? AudioManager.instance.GetMasterVolume() : 1f;
            masterVolumeText.text = $"{(int)(masterVolumeSlider.value * 100)}%";
        }
        
        if (ambienceVolumeSlider != null)
        {
            ambienceVolumeSlider.minValue = 0f;
            ambienceVolumeSlider.maxValue = 1f;
            ambienceVolumeSlider.onValueChanged.AddListener(OnAmbienceVolumeChanged);
            ambienceVolumeSlider.value = AudioManager.instance != null ? AudioManager.instance.GetAmbientVolume() : 1f;
            ambienceVolumeText.text = $"{(int)(ambienceVolumeSlider.value * 100)}%";
        }
        
        if (soundVolumeSlider != null)
        {
            soundVolumeSlider.minValue = 0f;
            soundVolumeSlider.maxValue = 1f;
            soundVolumeSlider.onValueChanged.AddListener(OnSoundVolumeChanged);
            soundVolumeSlider.value = AudioManager.instance != null ? AudioManager.instance.GetUIVolume() : 1f;
            soundVolumeText.text = $"{(int)(soundVolumeSlider.value * 100)}%";
        }
    }

    private void ToggleMuteAll()
    {
        isMuted = !isMuted;

        if (AudioManager.instance != null)
        {
            AudioManager.instance.MuteAll(isMuted);
        }
    }

    private void OnMasterVolumeChanged(float value)
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.SetMasterVolume(value);
            masterVolumeText.text = $"{(int)(value * 100)}%";
        }
    }

    private void OnAmbienceVolumeChanged(float value)
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.SetAmbientVolume(value);
            ambienceVolumeText.text = $"{(int)(value * 100)}%";
        }
    }

    private void OnSoundVolumeChanged(float value)
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.SetUIVolume(value);
            soundVolumeText.text = $"{(int)(value * 100)}%";
        }
    }

    public void ExitGame()
    {
        // Application.Quit();
        SceneManager.LoadSceneAsync(0);
    }

    private IEnumerator CooldownCoroutine()
    {   
        yield return new WaitForSeconds(0.1f);
        isOpen = true;
    }
}