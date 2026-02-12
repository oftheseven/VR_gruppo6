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
    [SerializeField] private Camera armCameraView;
    [SerializeField] private Camera tripodCamera;

    [Header("UI")]
    [SerializeField] private UI_DirectorPanel directorPanel;

    private bool isDirectorModeActive = false;
    private bool isDirectorModeAvailable = false;
    private int currentCameraIndex = 1; // 1 = slider, 2 = tripod
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

        if (armCameraView != null)
        {
            armCameraView.gameObject.SetActive(false);
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
        if (armCamera != null && armCameraView != null)
        {
            availableCameras.Add(3);
            Debug.Log("Camera 3 disponibile: BRACCIO MECCANICO");
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

        if (ArmMovementPlayback.instance != null && ArmMovementPlayback.instance.IsPlayingBack)
        {
            ArmMovementPlayback.instance.StopPlayback();
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

        if (armCameraView != null)
        {
            armCameraView.gameObject.SetActive(false);
        }

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

        // if (TutorialManager.instance != null)
        // {
        //     TutorialManager.instance.ExitDoor.Unlock(); // sblocco la porta solo ALLA FINE DELLA DIRECTOR MODE
        // }

        // if (TortaInTestaManager.instance != null)
        // {
        //     TortaInTestaManager.instance.ExitDoor.Unlock(); // sblocco la porta solo ALLA FINE DELLA DIRECTOR MODE
        // }

        UnlockExitDoor();
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

        if (armCamera != null && ArmMovementRecorder.instance != null && ArmMovementRecorder.instance.RecordedSnapshots != null)
        {
            var snapshots = ArmMovementRecorder.instance.RecordedSnapshots;
            
            if (snapshots.Count > 0)
            {
                float armDuration = snapshots[snapshots.Count - 1].timestamp;
                maxDuration = Mathf.Max(maxDuration, armDuration);
                // Debug.Log($"Arm recording: {armDuration:F1}s ({snapshots.Count} snapshots)");
            }
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

        if (armCamera != null && ArmMovementRecorder.instance != null)
        {
            int snapshotCount = ArmMovementRecorder.instance.SnapshotCount;
            
            if (snapshotCount > 0)
            {
                Debug.Log($"Camera Arm: Recording trovata ({snapshotCount} snapshots)");
            }
            else
            {
                Debug.LogWarning("Camera Arm: Nessuna recording!");
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
            // Debug.LogWarning($"Camera {cameraIndex} non disponibile in questa scena!");
            return;
        }

        currentCameraIndex = cameraIndex;

        // disattivo TUTTE le camere
        if (sliderCamera != null && sliderCamera.SliderCamera != null)
        {
            sliderCamera.SliderCamera.gameObject.GetComponentInChildren<Camera>().enabled = false;
        }

        if (tripodCamera != null)
        {
            tripodCamera.gameObject.SetActive(false);
        }

        if (armCameraView != null)
        {
            armCameraView.gameObject.SetActive(false);
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
                        Debug.Log("Camera 1: Slider playback started");
                    }
                    else if (sliderCamera.IsPlaying)
                    {
                        Debug.Log($"Camera 1: Slider (playback @ {sliderCamera.CurrentPosition:F2})");
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
                if (armCameraView != null)
                {
                    armCameraView.gameObject.SetActive(true);

                    if (ArmMovementRecorder.instance != null && 
                        ArmMovementRecorder.instance.SnapshotCount > 0 &&
                        ArmMovementPlayback.instance != null &&
                        !ArmMovementPlayback.instance.IsPlayingBack)
                    {
                        ArmMovementPlayback.instance.StartPlayback(
                            ArmMovementRecorder.instance.RecordedSnapshots,
                            armCamera,
                            armCameraView,
                            null
                        );
                        
                        Debug.Log($"Camera 3: Arm playback started ({ArmMovementRecorder.instance.SnapshotCount} snapshots)");
                    }
                    else if (ArmMovementPlayback.instance != null && 
                             ArmMovementPlayback.instance.IsPlayingBack)
                    {
                        Debug.Log($"Camera 3: Arm (playback @ {ArmMovementPlayback.instance.PlaybackProgress * 100:F0}%)");
                    }
                }
                break;
        }

        if (directorPanel != null)
        {
            directorPanel.UpdateCameraDisplay(cameraIndex);
        }
    }
}