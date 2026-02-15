using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.AI;

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
    private int currentCameraIndex = 1; // 1 = slider, 2 = tripod, 3 = arm
    private float sceneStartTime;
    private float sceneDuration;

    public bool IsDirectorModeActive => isDirectorModeActive;
    public int CurrentCameraIndex => currentCameraIndex;
    private List<int> availableCameras = new List<int>();

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning($"DirectorModeManager già esistente! Distruggo il vecchio ({_instance.gameObject.scene.name}) per usare il nuovo ({gameObject.scene.name})");
            Destroy(_instance.gameObject);
        }
        
        _instance = this;
        Debug.Log($"DirectorModeManager inizializzato per scena: {gameObject.scene.name}");


        if (tripodCamera != null)
        {
            tripodCamera.gameObject.SetActive(false);
        }
    }

    void OnDestroy()
    {
        if (_instance == this)
        {
            Debug.Log($"DirectorModeManager distrutto per scena: {gameObject.scene.name}");
            _instance = null;
        }
    }
    
    void Start()
    {
        DetectAvailableCameras();
    }

    void Update()
    {
        if (!isDirectorModeActive)
        {
            if (Keyboard.current.pKey.wasPressedThisFrame)
            {
                if (IsDirectorModeAvailable())
                {
                    StartDirectorMode();
                }
            }
            return;
        }

        HandleCameraSwitch();

        if (Time.time - sceneStartTime >= sceneDuration)
        {
            EndDirectorMode();
        }
    }

    private void DetectAvailableCameras()
    {
        availableCameras.Clear();

        // camera 1: slider
        if (sliderCamera != null && sliderCamera.SliderCamera != null)
        {
            availableCameras.Add(1);
            Debug.Log("Camera 1 disponibile: SLIDER");
        }

        // camera 2: tripod
        if (tripodCamera != null)
        {
            availableCameras.Add(2);
            Debug.Log("Camera 2 disponibile: TREPPIEDE");
        }

        // camera 3: arm
        if (armCamera != null && 
        armCamera.DirectorModeCamera != null)
        {
            availableCameras.Add(3);
            Debug.Log($"Camera 3 disponibile: BRACCIO MECCANICO ({armCamera.WaypointCount} waypoint)");
        }
        else if (armCamera != null)
        {
            if (armCamera.DirectorModeCamera == null)
                Debug.LogWarning("Camera Arm: Director Mode Camera non assegnata!");
            else
                Debug.LogWarning($"Camera Arm: Solo {armCamera.WaypointCount} waypoint (servono almeno 2)");
        }

        if (availableCameras.Count == 0)
        {
            Debug.LogError("NESSUNA CAMERA DISPONIBILE!");
        }
        else
        {
            Debug.Log($"{availableCameras.Count} camere disponibili: {string.Join(", ", availableCameras)}");
        }
    }

    public bool IsDirectorModeAvailable()
    {
        return isDirectorModeAvailable;
    }

    public void SetDirectorModeAvailable(bool available)
    {
        isDirectorModeAvailable = available;
        // Debug.Log($"Director Mode availability: {available}");
    }

    public void StartDirectorMode()
    {
        if (isDirectorModeActive) return;

        if (availableCameras.Count == 0)
        {
            Debug.LogError("Impossibile avviare Director Mode: nessuna camera disponibile!");
            return;
        }

        Debug.Log("DIRECTOR MODE STARTED");

        isDirectorModeActive = true;

        PlayerController.EnableMovement(false);
        PlayerController.HideCursor();

        PrepareActors();
        StartActorAnimations();

        SetupCameras();

        CalculateSceneDuration();

        sceneStartTime = Time.time;

        if (PlayerController.instance != null && PlayerController.instance.playerCamera != null)
        {
            PlayerController.instance.playerCamera.gameObject.SetActive(false);
        }

        SwitchToCamera(availableCameras[0]);

        if (directorPanel != null)
        {
            directorPanel.ShowPanel(sceneDuration, availableCameras);
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

        if (ArmWaypointPlayback.instance != null && ArmWaypointPlayback.instance.IsPlayingBack)
        {
            ArmWaypointPlayback.instance.StopPlayback();
        }

        // disattivo tutte le camere
        if (sliderCamera != null && sliderCamera.SliderCamera != null)
        {
            sliderCamera.SliderCamera.gameObject.GetComponentInChildren<Camera>().enabled = false;
        }

        if (tripodCamera != null)
        {
            tripodCamera.gameObject.SetActive(false);
        }

        if (armCamera != null && armCamera.DirectorModeCamera != null)
        {
            armCamera.DirectorModeCamera.enabled = false;
            Debug.Log("Arm Director Mode Camera disabilitata");
        }

        DisableAllInteractableCameras();

        // riattivo camera del player
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

        Debug.Log("Scena completata!");

        UnlockExitDoor();
    }

    private void DisableAllInteractableCameras()
    {
        InteractableCamera[] allCameras = FindObjectsByType<InteractableCamera>(FindObjectsSortMode.None);

        foreach (InteractableCamera cam in allCameras)
        {
            if (cam != null && cam.ViewCamera != null)
            {
                cam.ViewCamera.gameObject.SetActive(false);
                Debug.Log($"Disattivata ViewCamera di {cam.name}");
            }
        }
    }

    private void UnlockExitDoor()
    {
        bool doorUnlocked = false;

        if (TutorialManager.instance != null && TutorialManager.instance.ExitDoor != null)
        {
            TutorialManager.instance.ExitDoor.Unlock();
            Debug.Log("Porta Tutorial sbloccata!");
            doorUnlocked = true;
        }

        if (TortaInTestaManager.instance != null && TortaInTestaManager.instance.ExitDoor != null)
        {
            TortaInTestaManager.instance.ExitDoor.Unlock();
            Debug.Log("Porta TortaInTesta sbloccata!");
            doorUnlocked = true;
        }

        if (!doorUnlocked)
        {
            Debug.LogWarning("Nessuna porta da sbloccare trovata!");
        }
    }

    private void CalculateSceneDuration()
    {
        float maxDuration = 0f;

        if (sliderCamera != null && sliderCamera.CurrentRecording != null)
        {
            float sliderDuration = sliderCamera.CurrentRecording.duration;
            maxDuration = Mathf.Max(maxDuration, sliderDuration);
            // Debug.Log($"Slider recording: {sliderDuration:F1}s ({sliderCamera.CurrentRecording.GetKeyframeCount()} keyframes)");
        }

        if (armCamera != null && armCamera.WaypointCount >= 2)
        {
            float armDuration = armCamera.GetPlaybackDuration();
            maxDuration = Mathf.Max(maxDuration, armDuration);
            Debug.Log($"Arm recording: {armDuration:F1}s ({armCamera.WaypointCount} waypoint)");
        }

        sceneDuration = maxDuration > 0 ? maxDuration : defaultSceneDuration;

        if (maxDuration <= 0)
        {
            Debug.LogWarning($"Nessuna recording disponibile! Uso durata default: {sceneDuration}s");
        }
        else
        {
            Debug.Log($"Durata scena: {sceneDuration:F1}s");
        }
    }

    private void PrepareActors()
    {
        if (sceneActors == null || sceneActors.Length == 0)
        {
            Debug.LogWarning("Nessun attore assegnato!");
            return;
        }

        for (int i = 0; i < sceneActors.Length; i++)
        {
            if (sceneActors[i] == null) continue;

            sceneActors[i].SetActive(true);

            Animator animator = sceneActors[i].GetComponent<Animator>();
            if (animator != null)
            {
                animator.Rebind();
                // animator.Update(0f);
            }

            Debug.Log($"Attore {i} ({sceneActors[i].name}) ready");
        }

        Debug.Log("Attori preparati per la scena");
    }

    private void StartActorAnimations()
    {
        if (sceneActors == null || sceneActors.Length == 0)
        {
            Debug.LogWarning("Nessun attore assegnato!");
            return;
        }

        for (int i = 0; i < sceneActors.Length; i++)
        {
            if (sceneActors[i] == null) continue;

            ActorController navController = sceneActors[i].GetComponent<ActorController>();
            if (navController != null)
            {
                string trigger = "DoAction";
                if (actorAnimationTriggers != null && i < actorAnimationTriggers.Length)
                {
                    trigger = actorAnimationTriggers[i];
                }

                switch (trigger)
                {
                    case "WalkToTarget":
                        navController.WalkToTarget();
                        Debug.Log($"Attore {sceneActors[i].name} → Cammina verso target");
                        break;

                    case "DoAction":
                        navController.TriggerAction();
                        Debug.Log($"Attore {sceneActors[i].name} → Action");
                        break;

                    case "WalkAndAction":
                        StartCoroutine(WalkThenAction(navController));
                        Debug.Log($"Attore {sceneActors[i].name} → Walk + Action");
                        break;

                    default:
                        navController.TriggerAction();
                        break;
                }

                continue;
            }
        }

        Debug.Log("Animazioni attori avviate");
    }
    
    private System.Collections.IEnumerator WalkThenAction(ActorController controller)
    {
        controller.WalkToTarget();

        NavMeshAgent agent = controller.GetComponent<NavMeshAgent>();
        while (agent != null && agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        controller.TriggerAction();
    }

    private void ResetActors()
    {
        if (sceneActors == null || sceneActors.Length == 0) return;

        for (int i = 0; i < sceneActors.Length; i++)
        {
            if (sceneActors[i] == null) continue;

            ActorController navController = sceneActors[i].GetComponent<ActorController>();
            if (navController != null)
            {
                navController.ResetToIdle();
                Debug.Log($"Attore {sceneActors[i].name} reset");
                continue;
            }

            Animator animator = sceneActors[i].GetComponent<Animator>();
            if (animator != null)
            {
                animator.Play("Idle");
                Debug.Log($"Attore {sceneActors[i].name} reset a Idle");
            }
            }

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

        if (tripodCamera != null)
        {
            Debug.Log("Camera Treppiede: Ready");
        }

        if (armCamera != null)
        {
            if (armCamera.DirectorModeCamera != null)
            {
                Debug.Log($"Camera Arm: Ready ({armCamera.WaypointCount} waypoint)");
            }
            else if (armCamera.DirectorModeCamera == null)
            {
                Debug.LogWarning("Camera Arm: Director Mode Camera non assegnata!");
            }
            else
            {
                Debug.LogWarning($"Camera Arm: Solo {armCamera.WaypointCount} waypoint (servono almeno 2)");
            }
        }
    }

    private void HandleCameraSwitch()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame && availableCameras.Contains(1))
        {
            SwitchToCamera(1);
        }

        if (Keyboard.current.digit2Key.wasPressedThisFrame && availableCameras.Contains(2))
        {
            SwitchToCamera(2);
        }

        if (Keyboard.current.digit3Key.wasPressedThisFrame && availableCameras.Contains(3))
        {
            SwitchToCamera(3);
        }
    }

    private void SwitchToCamera(int cameraIndex)
    {
        if (!availableCameras.Contains(cameraIndex))
        {
            return;
        }

        currentCameraIndex = cameraIndex;

        if (sliderCamera != null && sliderCamera.SliderCamera != null)
        {
            sliderCamera.SliderCamera.gameObject.GetComponentInChildren<Camera>().enabled = false;
        }

        if (tripodCamera != null)
        {
            tripodCamera.gameObject.SetActive(false);
        }

        if (armCamera != null && armCamera.DirectorModeCamera != null)
        {
            armCamera.DirectorModeCamera.enabled = false;
        }

        if (PlayerController.instance != null && PlayerController.instance.playerCamera != null)
        {
            PlayerController.instance.playerCamera.gameObject.SetActive(false);
        }

        switch (cameraIndex)
        {
            case 1: // slider
                if (sliderCamera != null && sliderCamera.SliderCamera != null)
                {
                    sliderCamera.SliderCamera.gameObject.GetComponentInChildren<Camera>().enabled = true;

                    if (sliderCamera.CurrentRecording != null && !sliderCamera.IsPlaying)
                    {
                        sliderCamera.StartPlayback();
                        Debug.Log("Camera 1: Slider playback avviato");
                    }
                }
                break;

            case 2: // tripod
                if (tripodCamera != null)
                {
                    tripodCamera.gameObject.SetActive(true);
                    Debug.Log("Camera 2: Treppiede");
                }
                break;

            case 3: // arm
                if (armCamera != null && armCamera.DirectorModeCamera != null)
                {
                    armCamera.DirectorModeCamera.enabled = true;
                    Debug.Log($"Camera 3: Arm Director Mode Camera attivata");

                    // Avvia playback se non già attivo
                    if (ArmWaypointPlayback.instance != null && 
                        armCamera.WaypointCount >= 2 &&
                        !ArmWaypointPlayback.instance.IsPlayingBack)
                    {
                        ArmWaypointPlayback.instance.StartPlayback(armCamera);
                        Debug.Log($"Arm playback avviato ({armCamera.WaypointCount} waypoint)");
                    }
                    else if (ArmWaypointPlayback.instance != null && 
                             ArmWaypointPlayback.instance.IsPlayingBack)
                    {
                        Debug.Log($"Arm playback @ {ArmWaypointPlayback.instance.PlaybackProgress * 100:F0}%");
                    }
                }
                else
                {
                    Debug.LogError("Arm Director Mode Camera non disponibile!");
                }
                break;
        }

        if (directorPanel != null)
        {
            directorPanel.UpdateCameraDisplay(cameraIndex);
        }
    }
}