using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

public class DirectorModeManager : MonoBehaviour
{
    // singleton
    private static DirectorModeManager _instance;
    public static DirectorModeManager instance => _instance;

    [Header("Scene setup")]
    [SerializeField] private GameObject[] salottoActors;
    [SerializeField] private DirectorModeAnimStarter[] salottoAnimStarters;
    [SerializeField] private GameObject[] divinationActors;
    [SerializeField] private string[] actorAnimationTriggers;
    [SerializeField] private float defaultSceneDuration = 30f;

    [Header("Camera setup")]
    [SerializeField] private InteractableSlider sliderCamera;
    [SerializeField] private InteractableArm armCamera;
    [SerializeField] private InteractableCamera livingRoomTripodCamera;
    [SerializeField] private InteractableCamera divinationTripodCamera;
    private InteractableCamera currentTripodCamera;

    [Header("Tripod camera duration")]
    [SerializeField] private float fixedCameraDuration = 4f; // durata fissa per camera senza recording

    [Header("UI")]
    [SerializeField] private UI_DirectorPanel directorPanel;

    private bool isDirectorModeActive = false;
    private bool isDirectorModeAvailable = false;
    private int currentCameraIndex = 1; // 1 = slider, 2 = tripod, 3 = arm
    private float sceneStartTime;
    private float sceneDuration;

    public GameObject[] SalottoActors => salottoActors;
    public GameObject[] DivinationActors => divinationActors;

    public bool IsDirectorModeActive => isDirectorModeActive;
    public int CurrentCameraIndex => currentCameraIndex;
    private List<int> availableCameras = new List<int>();

    private enum PhaseType { LivingRoom, Divination }
    private PhaseType currentPhase;

    private int animatedCameraIndex;     // 1 = slider, 3 = arm
    private int fixedCameraIndex;        // 2 = tripod

    private bool isAnimatingCamera = false;
    private bool isRunningFixedCamera = false;
    private float fixedCameraTimer = 0f;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning($"DirectorModeManager già esistente! Distruggo il vecchio ({_instance.gameObject.scene.name}) per usare il nuovo ({gameObject.scene.name})");
            Destroy(_instance.gameObject);
        }
        
        _instance = this;
        Debug.Log($"DirectorModeManager inizializzato per scena: {gameObject.scene.name}");

        if (livingRoomTripodCamera != null || divinationTripodCamera != null)
        {
            livingRoomTripodCamera.gameObject.GetComponentInChildren<Camera>().gameObject.SetActive(false);
            divinationTripodCamera.gameObject.GetComponentInChildren<Camera>().gameObject.SetActive(false);
        }

        SetActorsActive(salottoActors, false);
        SetActorsActive(divinationActors, false);
    }

    public void SetActorsActive(GameObject[] actors, bool active)
    {
        if (actors == null) return;
        foreach (var actor in actors)
        {
            if (actor != null)
            {
                actor.SetActive(active);
            }
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

    void Update()
    {
        if (!isDirectorModeActive) return;

        if (isAnimatingCamera)
        {
            // salotto: playback slider
            if (currentPhase == PhaseType.LivingRoom && sliderCamera != null)
            {
                if (!sliderCamera.IsPlaying)
                {
                    // playback finito, passa a camera fissa
                    SwitchToCamera(fixedCameraIndex);
                    isAnimatingCamera = false;
                    isRunningFixedCamera = true;
                    fixedCameraTimer = 0f;
                }
            }
            // divination: playback arm
            else if (currentPhase == PhaseType.Divination && ArmWaypointPlayback.instance != null)
            {
                if (!ArmWaypointPlayback.instance.IsPlayingBack)
                {
                    // playback finito, passo a camera fissa
                    SwitchToCamera(fixedCameraIndex);
                    isAnimatingCamera = false;
                    isRunningFixedCamera = true;
                    fixedCameraTimer = 0f;
                }
            }
        }
        else if (isRunningFixedCamera)
        {
            fixedCameraTimer += Time.deltaTime;
            if (fixedCameraTimer >= fixedCameraDuration)
            {
                isRunningFixedCamera = false;
                EndDirectorMode();
            }
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
        availableCameras.Clear();

        // camera 1: slider
        if (sliderCamera != null && sliderCamera.SliderCamera != null && sliderCamera.CurrentRecording != null)
        {
            availableCameras.Add(1);
            Debug.Log("Camera 1 disponibile: SLIDER");
        }
        // camera 2: tripod livingroom
        if (livingRoomTripodCamera != null && currentPhase != PhaseType.Divination)
        {
            availableCameras.Add(2);
            Debug.Log("Camera 2 disponibile: TREPPIEDE SALOTTO");
        }
        // camera 2: tripod divination
        if (divinationTripodCamera != null && currentPhase == PhaseType.Divination)
        {
            availableCameras.Add(2);
            Debug.Log("Camera 2 disponibile: TREPPIEDE DIVINATION");
        }
        // camera 3: arm
        if (armCamera != null && armCamera.DirectorModeCamera != null && armCamera.WaypointCount >= 2)
        {
            availableCameras.Add(3);
            Debug.Log($"Camera 3 disponibile: BRACCIO MECCANICO ({armCamera.WaypointCount} waypoint)");
        }
        if (isDirectorModeActive) return;
        if (availableCameras.Count == 0)
        {
            Debug.LogWarning("Impossibile avviare Director Mode: nessuna camera disponibile!");
            return;
        }

        Debug.Log("DIRECTOR MODE STARTED");

        if (sliderCamera != null && sliderCamera.CurrentRecording != null)
        {
            currentPhase = PhaseType.LivingRoom;
            animatedCameraIndex = 1; // slider
            fixedCameraIndex = 2;    // tripod
            currentTripodCamera = livingRoomTripodCamera;
        }
        else if (armCamera != null && armCamera.WaypointCount >= 2)
        {
            currentPhase = PhaseType.Divination;
            animatedCameraIndex = 3; // arm
            fixedCameraIndex = 2;    // tripod
            currentTripodCamera = divinationTripodCamera;
        }
        else
        {
            Debug.LogWarning("Non è definita una fase valida per Director Mode!");
            return;
        }

        isDirectorModeActive = true;
        isAnimatingCamera = true;
        isRunningFixedCamera = false;

        PlayerController.EnableMovement(false);
        PlayerController.HideCursor();
        PlayerController.BlockBasePanel = true;
        PlayerController.SetBasePanelActive(false);

        PrepareActors();
        StartActorAnimations();
        StartObjectsAnimations();
        CalculateSceneDuration();

        sceneStartTime = Time.time;

        if (PlayerController.instance != null && PlayerController.instance.playerCamera != null)
            PlayerController.instance.playerCamera.gameObject.SetActive(false);

        SwitchToCamera(animatedCameraIndex);

        // START playback
        if (currentPhase == PhaseType.LivingRoom && sliderCamera != null)
            sliderCamera.StartPlayback();
        else if (currentPhase == PhaseType.Divination && armCamera != null)
            ArmWaypointPlayback.instance.StartPlayback(armCamera);

        if (directorPanel != null)
            directorPanel.ShowPanel(sceneDuration, availableCameras);
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

        if (livingRoomTripodCamera != null && livingRoomTripodCamera.ViewCamera != null)
            livingRoomTripodCamera.ViewCamera.gameObject.SetActive(false);
        if (divinationTripodCamera != null && divinationTripodCamera.ViewCamera != null)
            divinationTripodCamera.ViewCamera.gameObject.SetActive(false);

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
        ResetObjects();

        if (directorPanel != null)
        {
            directorPanel.HidePanel();
        }

        PlayerController.BlockBasePanel = false;
        PlayerController.SetBasePanelActive(true);

        PlayerController.EnableMovement(true);

        Debug.Log("Scena completata!");

        if (!isDirectorModeActive) return;
        isDirectorModeActive = false;
        isAnimatingCamera = false;
        isRunningFixedCamera = false;
        fixedCameraTimer = 0f;
        isDirectorModeAvailable= false;
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
        sceneDuration += fixedCameraDuration;

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
        GameObject[] activeActors = currentPhase switch
        {
            PhaseType.LivingRoom => salottoActors,
            PhaseType.Divination => divinationActors,
            _ => null
        };

        if (activeActors == null || activeActors.Length == 0)
        {
            Debug.LogWarning("Nessun attore assegnato!");
            return;
        }

        foreach (var actor in activeActors)
        {
            if (actor == null) continue;

            actor.SetActive(true);
            Animator animator = actor.GetComponent<Animator>();

            if (animator != null)
            {
                animator.Rebind();
            }
            Debug.Log($"Attore scene ready: {actor.name}");
        }
        Debug.Log("Attori preparati per la scena (zona: " + currentPhase + ")");
    }

    private void StartActorAnimations()
    {
        GameObject[] activeActors = currentPhase switch
        {
            PhaseType.LivingRoom => salottoActors,
            PhaseType.Divination => divinationActors,
            _ => null
        };

        if (activeActors == null || activeActors.Length == 0)
        {
            Debug.LogWarning("Nessun attore assegnato!");
            return;
        }

        for (int i = 0; i < activeActors.Length; i++)
        {
            if (activeActors[i] == null) continue;

            ActorController navController = activeActors[i].GetComponent<ActorController>();
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
                        Debug.Log($"Attore {activeActors[i].name} → Cammina verso target");
                        break;

                    case "DoAction":
                        navController.TriggerAction();
                        Debug.Log($"Attore {activeActors[i].name} → Action");
                        break;

                    case "WalkAndAction":
                        StartCoroutine(WalkThenAction(navController));
                        Debug.Log($"Attore {activeActors[i].name} → Walk + Action");
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

    private void StartObjectsAnimations()
    {
        foreach (var starter in salottoAnimStarters)
        {
            if (starter != null)
            {
                starter.PlayDirectorAnim();
            }
        }
    }

    private void ResetObjects()
    {
        foreach (var starter in salottoAnimStarters)
        {
            if (starter != null)
            {
                starter.Reset();
            }
        }
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
        GameObject[] activeActors = currentPhase switch
        {
            PhaseType.LivingRoom => salottoActors,
            PhaseType.Divination => divinationActors,
            _ => null
        };

        if (activeActors == null || activeActors.Length == 0) return;

        for (int i = 0; i < activeActors.Length; i++)
        {
            if (activeActors[i] == null) continue;

            ActorController navController = activeActors[i].GetComponent<ActorController>();
            if (navController != null)
            {
                navController.ResetToIdle();
                Debug.Log($"Attore {activeActors[i].name} reset");
                continue;
            }

            Animator animator = activeActors[i].GetComponent<Animator>();
            if (animator != null)
            {
                animator.Play("Idle");
                Debug.Log($"Attore {activeActors[i].name} reset a Idle");
            }
            }

        Debug.Log("Attori resettati alla posizione iniziale");
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

        if (armCamera != null && armCamera.DirectorModeCamera != null)
        {
            armCamera.DirectorModeCamera.enabled = false;
        }

        if (PlayerController.instance != null && PlayerController.instance.playerCamera != null)
        {
            PlayerController.instance.playerCamera.gameObject.SetActive(false);
        }

        if (livingRoomTripodCamera != null && livingRoomTripodCamera.ViewCamera != null)
            livingRoomTripodCamera.ViewCamera.gameObject.SetActive(false);
        if (divinationTripodCamera != null && divinationTripodCamera.ViewCamera != null)
            divinationTripodCamera.ViewCamera.gameObject.SetActive(false);

        switch (cameraIndex)
        {
            case 1: // slider
                if (sliderCamera != null && sliderCamera.SliderCamera != null)
                {
                    sliderCamera.SliderCamera.gameObject.GetComponentInChildren<Camera>().enabled = true;

                    if (sliderCamera.CurrentRecording != null && !sliderCamera.IsPlaying)
                    {
                        sliderCamera.StartPlayback();
                        UI_DirectorPanel.instance.SetCameraPanel(1);
                        Debug.Log("Camera 1: Slider playback avviato");
                    }
                }
                break;

            case 2: // tripod
                if (currentTripodCamera != null && currentTripodCamera.ViewCamera != null)
                {
                    currentTripodCamera.ViewCamera.gameObject.SetActive(true);
                    UI_DirectorPanel.instance.SetCameraPanel(2);
                    Debug.Log("Tripod camera attivata: " + currentTripodCamera.name);
                }
                break;

            case 3: // arm
                if (armCamera != null && armCamera.DirectorModeCamera != null)
                {
                    armCamera.DirectorModeCamera.enabled = true;
                    Debug.Log($"Camera 3: Arm Director Mode Camera attivata");

                    if (ArmWaypointPlayback.instance != null && 
                        armCamera.WaypointCount >= 2 &&
                        !ArmWaypointPlayback.instance.IsPlayingBack)
                    {
                        ArmWaypointPlayback.instance.StartPlayback(armCamera);
                        UI_DirectorPanel.instance.SetCameraPanel(1);
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
                    Debug.LogWarning("Arm Director Mode Camera non disponibile!");
                }
                break;
        }
    }
}