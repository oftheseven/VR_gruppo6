using UnityEngine;

public class InteractableLight : MonoBehaviour
{
    [Header("Interaction text")]
    [SerializeField] private string interactionText = "Premi E per gestire la luce";
    
    [Header("Panel reference")]
    [SerializeField] private UI_LightPanel lightPanel;

    [Header("Light camera reference")]
    [SerializeField] private Camera lightCamera;

    [Header("Light settings")]
    [SerializeField] private Light[] controlledLights;

    [Header("Tutorial Tracking")]
    private bool tutorialPanelOpened = false;
    private bool tutorialSliderMoved = false;

    [Header("Audio")]
    [SerializeField] private AudioClip buzzSound;
    [SerializeField] [Range(0f, 1f)] private float buzzVolume = 0.3f;

    [Header("Quest Target (per quest salotto)")]
    [SerializeField, Range(0f, 1f)] private float livingTargetIntensity = 0.8f;
    [SerializeField] private float livingTargetTemperature = 5000f;
    [SerializeField] private float questThreshold = 0.05f;
    private bool livingRoomLightCompleted = false;

    [Header("Quest Target (per quest divination)")]
    [SerializeField, Range(0f, 1f)] private float divinationTargetIntensity = 0.8f;
    [SerializeField] private float divinationTargetTemperature = 5000f;
    private bool divinationRoomLightCompleted = false;

    private AudioSource audioSource;
    private bool isBuzzing = false;
    
    private bool isOn = true;
    private float currentIntensity = 1f;
    private float maxIntensity = 8f;
    private Color currentColor = Color.white;
    private float currentTemperature = 6500f;
    private bool isTemperatureMode = false;
    public void SetCameraActive(bool active)
    {
        if (lightCamera != null)
        {
            lightCamera.gameObject.SetActive(active);
        }
    }

    public bool IsOn => isOn;
    public float CurrentIntensity => currentIntensity;
    public float MaxIntensity => maxIntensity;
    public Color CurrentColor => currentColor;
    public float CurrentTemperature => currentTemperature;
    public bool IsTemperatureMode => isTemperatureMode;
    public Light[] ControlledLights => controlledLights;

    void Awake()
    {
        SetCameraActive(false);
    }

    void Start()
    {
        if (controlledLights == null || controlledLights.Length == 0)
        {
            controlledLights = new Light[] { GetComponent<Light>() };
        }

        if (controlledLights != null && controlledLights.Length > 0 && controlledLights[0] != null)
        {
            Light mainLight = controlledLights[0];
            
            isOn = mainLight.enabled;
            currentIntensity = mainLight.intensity;
            maxIntensity = mainLight.intensity; // come massimo prendo l'intensità settata della luce
            currentColor = mainLight.color;
        }

        UpdateLights();
        SetupAudio();

        if (isOn && buzzSound != null)
        {
            StartBuzz();
        }
    }

    public void Interact()
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayInteractionSFX();
        }
        
        if (lightPanel != null)
        {
            lightPanel.OpenPanel(this);
            if (QuestManager.instance != null && 
                QuestManager.instance.IsQuestActive(QuestManager.MainQuest.TutorialLights))
            {
                tutorialPanelOpened = true;
                CheckTutorialComplete();
            }
        }
        else
        {
            Debug.LogError("LightPanel non assegnato su " + this.name);
        }

        // if (TutorialManager.instance != null)
        // {
        //     TutorialManager.instance.OnLightCompleted();
        // }

        // if (GameManager.instance != null)
        // {
        //     GameManager.instance.OnLightCompleted();
        // }
    }

    public void SetLightState(bool state)
    {
        isOn = state;
        if (isOn)
        {
            StartBuzz();
        }
        else
        {
            StopBuzz();
        }
        UpdateLights();
    }

    public void SetIntensity(float intensity)
    {
        currentIntensity = Mathf.Clamp(intensity, 0f, maxIntensity);
        UpdateLights();

        OnSliderMoved();
    }

    public void SetTemperature(float temperature)
    {
        if (!isTemperatureMode) return;
        
        currentTemperature = Mathf.Clamp(temperature, 1000f, 12000f);
        currentColor = KelvinToColor(currentTemperature);
        UpdateLights();

        OnSliderMoved();
    }

    public void SetColor(Color color)
    {
        if (isTemperatureMode) return;
        
        currentColor = color;
        UpdateLights();

        OnSliderMoved();
    }

    private void OnSliderMoved()
    {
        // TUTORIAL
        if (QuestManager.instance != null && QuestManager.instance.IsQuestActive(QuestManager.MainQuest.TutorialLights))
        {
            tutorialSliderMoved = true;
            CheckTutorialComplete();
        }

        // SALOTTO
        if (QuestManager.instance != null && QuestManager.instance.IsQuestActive(QuestManager.MainQuest.LivingRoomLight))
        {
            CheckLivingRoomLightQuest();
        }
    }

    private void CheckTutorialComplete()
    {
        if (tutorialPanelOpened && tutorialSliderMoved)
        {
            Debug.Log("Tutorial Luci completato!");
            QuestManager.instance.CompleteCurrentQuest();
            
            tutorialPanelOpened = false;
            tutorialSliderMoved = false;
        }
    }

    private void CheckLivingRoomLightQuest()
    {
        if (livingRoomLightCompleted) return;

        bool allOk = true;

        foreach (InteractableLight light in QuestManager.instance.LivingRoomLights)
        {
            float normalizedIntensity = light.CurrentIntensity / light.MaxIntensity;
            float sliderValue = Mathf.Sqrt(normalizedIntensity) * light.MaxIntensity;
            float percentage = sliderValue / light.MaxIntensity;
            bool okIntensity = Mathf.Abs(percentage - livingTargetIntensity) < 0.05f;
            bool okTemp = Mathf.Abs(light.CurrentTemperature - livingTargetTemperature) < 100f;

            if (!(okIntensity && okTemp))
                allOk = false;
        }

        if (allOk)
        {
            Debug.Log("Quest luce salotto COMPLETATA: intensità e temperatura OK");
            livingRoomLightCompleted = true;
            QuestManager.instance.CompleteCurrentQuest();
        }
    }

    private void CheckDivinationLightQuest()
    {
        if (divinationRoomLightCompleted) return;

        bool allOk = true;

        foreach (InteractableLight light in QuestManager.instance.DivinationLights)
        {
            float normalizedIntensity = light.CurrentIntensity / light.MaxIntensity;
            float sliderValue = Mathf.Sqrt(normalizedIntensity) * light.MaxIntensity;
            float percentage = sliderValue / light.MaxIntensity;
            bool okIntensity = Mathf.Abs(percentage - divinationTargetIntensity) < 0.05f;
            bool okTemp = Mathf.Abs(light.CurrentTemperature - divinationTargetTemperature) < 100f;
            
            if (!(okIntensity && okTemp))
                allOk = false;
        }

        if (allOk)
        {
            Debug.Log("Quest luce divination room COMPLETATA: intensità e temperatura OK");
            divinationRoomLightCompleted = true;
            QuestManager.instance.CompleteCurrentQuest();
        }
    }

    public void SetColorMode(bool useTemperature)
    {
        isTemperatureMode = useTemperature;
        
        if (isTemperatureMode)
        {
            currentColor = Color.white;
            currentTemperature = 6500f;
        }
        
        UpdateLights();
    }

    private void UpdateLights()
    {
        if (controlledLights != null)
        {
            foreach (Light light in controlledLights)
            {
                if (light != null)
                {
                    light.enabled = isOn;
                    light.intensity = currentIntensity;
                    light.color = currentColor;
                }
            }
        }
    }

    private Color KelvinToColor(float kelvin)
    {
        float temp = kelvin / 100f;
        float r, g, b;

        // RED
        if (temp <= 66f)
        {
            r = 255f;
        }
        else
        {
            r = temp - 60f;
            r = 329.698727446f * Mathf.Pow(r, -0.1332047592f);
            r = Mathf.Clamp(r, 0f, 255f);
        }

        // GREEN
        if (temp <= 66f)
        {
            g = temp;
            g = 99.4708025861f * Mathf.Log(g) - 161.1195681661f;
            g = Mathf.Clamp(g, 0f, 255f);
        }
        else
        {
            g = temp - 60f;
            g = 288.1221695283f * Mathf.Pow(g, -0.0755148492f);
            g = Mathf.Clamp(g, 0f, 255f);
        }

        // BLUE
        if (temp >= 66f)
        {
            b = 255f;
        }
        else if (temp <= 19f)
        {
            b = 0f;
        }
        else
        {
            b = temp - 10f;
            b = 138.5177312231f * Mathf.Log(b) - 305.0447927307f;
            b = Mathf.Clamp(b, 0f, 255f);
        }

        return new Color(r / 255f, g / 255f, b / 255f);
    }

    private void SetupAudio()
    {
        if (buzzSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = buzzSound;
            audioSource.playOnAwake = false;
            audioSource.loop = true;
            audioSource.spatialBlend = 1f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.minDistance = 1f;
            audioSource.maxDistance = 10f;
            audioSource.dopplerLevel = 0f;
            audioSource.volume = 0.8f;

            if (AudioManager.instance != null && AudioManager.instance.AudioMixer != null)
            {
                audioSource.outputAudioMixerGroup = AudioManager.instance.AudioMixer.FindMatchingGroups("SFX")[0];
                Debug.Log($"{gameObject.name} audio assegnato a: {audioSource.outputAudioMixerGroup.name}");
            }
        }
    }

    private void StartBuzz()
    {
        if (buzzSound != null && audioSource != null && !isBuzzing)
        {
            audioSource.clip = buzzSound;
            audioSource.loop = true;
            audioSource.volume = buzzVolume;
            audioSource.Play();
            isBuzzing = true;
        }
    }

    private void StopBuzz()
    {
        if (isBuzzing && audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
            isBuzzing = false;
        }
    }

    public string GetInteractionText()
    {
        return interactionText;
    }

    public UI_LightPanel GetLightPanel()
    {
        return lightPanel;
    }
}
