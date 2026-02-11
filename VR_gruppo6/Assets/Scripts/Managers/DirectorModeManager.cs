using UnityEngine;
using UnityEngine.InputSystem;

public class DirectorModeManager : MonoBehaviour
{
    // singleton
    private static DirectorModeManager _instance;
    public static DirectorModeManager instance => _instance;

    [Header("Scene setup")]
    [SerializeField] private GameObject[] sceneActors;
    [SerializeField] private string[] actorAnimationTriggers;
    [SerializeField] private float defaultSceneDuration = 30f;

    [Header("Camera setup")]
    [SerializeField] private InteractableSlider sliderCamera;
    [SerializeField] private InteractableArm armCamera;
    [SerializeField] private Camera tripodCamera;

    [Header("UI")]
    [SerializeField] private UI_DirectorPanel directorPanel;

    private bool isDirectorModeActive = false;
    private bool isDirectorModeAvailable = false;
    public void SetDirectorModeAvailable(bool available)
    {
        isDirectorModeAvailable = available;
        // Debug.Log($"Director Mode availability set to: {available}");
    }
    private int currentCameraIndex = 1; // 1 = slider, 2 = tripod
    private float sceneStartTime;
    private float sceneDuration;

    public bool IsDirectorModeActive => isDirectorModeActive;
    public int CurrentCameraIndex => currentCameraIndex;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }
    
    void Start()
    {
        if (tripodCamera != null)
        {
            tripodCamera.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (!isDirectorModeActive)
        {
            if (Keyboard.current.pKey.wasPressedThisFrame)
            {
                StartDirectorMode();
            }
            return;
        }

        HandleCameraSwitch();

        if (Time.time - sceneStartTime >= sceneDuration)
        {
            EndDirectorMode();
        }
    }

    public bool IsDirectorModeAvailable()
    {
        return isDirectorModeAvailable;
    }

    public void StartDirectorMode()
    {
        if (isDirectorModeActive) return;

        Debug.Log("DIRECTOR MODE STARTED");

        isDirectorModeActive = true;

        PlayerController.EnableMovement(false);
        PlayerController.HideCursor();

        PrepareActors();
        StartActorAnimations();

        SetupCameras();

        if (sliderCamera != null && sliderCamera.CurrentRecording != null)
        {
            sceneDuration = sliderCamera.CurrentRecording.duration;
            // Debug.Log($"Durata scena impostata da recording: {sceneDuration:F1}s");
        }
        else
        {
            sceneDuration = defaultSceneDuration;
            // Debug.LogWarning($"Nessuna recording slider! Uso durata default: {sceneDuration}s");
        }

        sceneStartTime = Time.time;

        if (PlayerController.instance != null && PlayerController.instance.playerCamera != null)
        {
            PlayerController.instance.playerCamera.gameObject.SetActive(false);
        }

        currentCameraIndex = -1;
        SwitchToCamera(1);

        if (directorPanel != null)
        {
            directorPanel.ShowPanel(sceneDuration);
        }
    }

    public void EndDirectorMode()
    {
        if (!isDirectorModeActive) return;

        Debug.Log("DIRECTOR MODE ENDED");

        isDirectorModeActive = false;

        if (sliderCamera != null && sliderCamera.IsPlaying)
        {
            sliderCamera.StopPlayback();
        }

        if (sliderCamera != null && sliderCamera.SliderCamera != null)
        {
            sliderCamera.SliderCamera.gameObject.SetActive(false);
        }

        if (tripodCamera != null)
        {
            tripodCamera.gameObject.SetActive(false);
        }

        if (PlayerController.instance != null)
        {
            PlayerController.instance.playerCamera.gameObject.SetActive(true);
        }

        ResetActors();

        if (directorPanel != null)
        {
            directorPanel.HidePanel();
        }

        PlayerController.EnableMovement(true);

        Debug.Log("Scena completata! Ciak consumato.");
    }

    private void PrepareActors()
    {
        // if (sceneActors == null || sceneActors.Length == 0)
        // {
        //     Debug.LogWarning("Nessun attore assegnato!");
        //     return;
        // }

        // for (int i = 0; i < sceneActors.Length; i++)
        // {
        //     if (sceneActors[i] == null) continue;

        //     sceneActors[i].SetActive(true);

        //     Animator animator = sceneActors[i].GetComponent<Animator>();
        //     if (animator != null)
        //     {
        //         animator.Rebind();
        //         animator.Update(0f);
        //     }

        //     Debug.Log($"Attore {i} ({sceneActors[i].name}) ready");
        // }

        Debug.Log("Attori preparati per la scena");
    }

    private void StartActorAnimations()
    {
        // if (sceneActors == null || sceneActors.Length == 0) return;

        // for (int i = 0; i < sceneActors.Length; i++)
        // {
        //     if (sceneActors[i] == null) continue;

        //     Animator animator = sceneActors[i].GetComponent<Animator>();
        //     if (animator == null)
        //     {
        //         Debug.LogWarning($"Attore {sceneActors[i].name} non ha Animator!");
        //         continue;
        //     }

        //     if (actorAnimationTriggers != null && i < actorAnimationTriggers.Length)
        //     {
        //         string trigger = actorAnimationTriggers[i];
                
        //         if (!string.IsNullOrEmpty(trigger))
        //         {
        //             animator.SetTrigger(trigger);
        //             Debug.Log($"Attore {sceneActors[i].name} → Trigger: {trigger}");
        //         }
        //     }
        // }

        Debug.Log("Animazioni attori avviate");
    }

    private void ResetActors()
    {
        // if (sceneActors == null || sceneActors.Length == 0) return;

        // for (int i = 0; i < sceneActors.Length; i++)
        // {
        //     if (sceneActors[i] == null) continue;

        //     Animator animator = sceneActors[i].GetComponent<Animator>();
        //     if (animator != null)
        //     {
        //         animator.SetTrigger("Idle");
        //     }


        //     Debug.Log($"Attore {sceneActors[i].name} reset");
        // }

        Debug.Log("Attori resettati alla posizione iniziale");
    }

    private void SetupCameras()
    {
        if (sliderCamera != null)
        {
            if (sliderCamera.CurrentRecording != null && sliderCamera.CurrentRecording.GetKeyframeCount() > 0)
            {
                Debug.Log("Camera Slider: Recording trovata, pronta per playback");
            }
            else
            {
                Debug.LogWarning("Camera Slider: Nessuna recording! Usa posizione fissa.");
            }
        }

        // if (tripodCamera != null)
        // {
        //     Debug.Log("Camera Treppiede: Ready");
        // }
    }

    private void HandleCameraSwitch()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            SwitchToCamera(1);
        }

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            SwitchToCamera(2);
        }
    }

    private void SwitchToCamera(int cameraIndex)
    {

        currentCameraIndex = cameraIndex;

        if (sliderCamera != null && sliderCamera.SliderCamera != null)
        {
            sliderCamera.SliderCamera.gameObject.SetActive(false);
        }

        if (tripodCamera != null)
        {
            tripodCamera.gameObject.SetActive(false);
        }
        if (PlayerController.instance != null && PlayerController.instance.playerCamera != null)
        {
            PlayerController.instance.playerCamera.gameObject.SetActive(false);
        }

        switch (cameraIndex)
        {
            case 1:
                if (sliderCamera != null && sliderCamera.SliderCamera != null)
                {
                    sliderCamera.SliderCamera.gameObject.SetActive(true);
                    
                    if (sliderCamera.CurrentRecording != null && !sliderCamera.IsPlaying)
                    {
                        sliderCamera.StartPlayback();
                        // Debug.Log($"Playback AVVIATO! Keyframes: {sliderCamera.CurrentRecording.GetKeyframeCount()}");
                    }
                    // else if (sliderCamera.IsPlaying)
                    // {
                    //     Debug.Log("Playback già in corso, continua...");
                    // }

                    // Debug.Log("Switched to Camera 1: Slider");
                }
                break;

            case 2:
                if (tripodCamera != null)
                {
                    tripodCamera.gameObject.SetActive(true);
                    // Debug.Log("Switched to Camera 2: Treppiede");
                }
                break;
        }

        if (directorPanel != null)
        {
            directorPanel.UpdateCameraDisplay(cameraIndex);
        }
    }
}