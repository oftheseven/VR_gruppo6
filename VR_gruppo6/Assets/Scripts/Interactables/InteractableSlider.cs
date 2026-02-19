using UnityEngine;

public class InteractableSlider : MonoBehaviour
{
    [Header("Slider references")]
    [SerializeField] private Transform sliderCart;
    [SerializeField] private GameObject sliderCamera;
    [SerializeField] private Transform railStart;
    [SerializeField] private Transform railEnd;

    [Header("Collider quest slider")]
    [SerializeField] private Collider questLookTarget;
    [SerializeField] private float raycastDistance = 40f;
    private float lookTimer = 0f;
    private bool livingRoomSliderCompleted = false;

    [Header("Slider settings")]
    [SerializeField] private string sliderName = "Slider tutorial";
    [SerializeField] private float currentPosition = 0.5f;

    [Header("Interaction settings")]
    [SerializeField] private string interactionText = "Premi E per interagire con lo slider";
    [SerializeField] private UI_SliderPanel sliderPanel;

    [Header("Camera settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float cameraRotationSpeed = 5f;

    private bool canInteract = true;
    public bool CanInteract => canInteract;
    
    public GameObject SliderCamera => sliderCamera;
    public float CurrentPosition => currentPosition;
    public string SliderName => sliderName;
    public float CameraRotationSpeed => cameraRotationSpeed;
    public float MoveSpeed => moveSpeed;

    [Header("Recording variables & settings")]
    [SerializeField] private float recordingSampleRate = 0.05f;

    [Header("Recording max duration")]
    [SerializeField] private float maxRecordingDuration = 3f;
    public float MaxRecordingDuration => maxRecordingDuration;
    private SliderRecording currentRecording;
    private bool isRecording = false;
    private bool isPlaying = false;
    private float recordingStartTime;
    private float playbackStartTime;
    private int currentPlaybackIndex = 0;

    public bool IsRecording => isRecording;
    public bool IsPlaying => isPlaying;
    public SliderRecording CurrentRecording => currentRecording;

    private Quaternion startingCameraRotation;
    public Quaternion CameraStartingRotation() => startingCameraRotation;

    private bool tutorialQuestCompleted = false;

    void Awake()
    {
        sliderCamera.gameObject.GetComponentInChildren<AudioListener>().enabled = false;
    }

    void Start()
    {
        if (sliderCamera != null)
        {
            startingCameraRotation = sliderCamera.transform.localRotation;
        }

        UpdateSliderPosition(currentPosition);

        if (sliderCamera != null)
        {
            sliderCamera.gameObject.GetComponentInChildren<Camera>().enabled = false;
        }

        questLookTarget.enabled = false;
    }

    void Update()
    {
        if (sliderPanel != null && sliderPanel.IsOpen)
        {
            UpdateSliderPosition(currentPosition);

            if (isRecording)
            {
                UpdateRecording();
            }
        }

        if (isPlaying)
        {
            UpdatePlayback();
        }
    }

    public Vector3 RailDirection()
    {
        if (railStart == null || railEnd == null) return Vector3.forward;
        return (railEnd.position - railStart.position).normalized;
    }

    public void Interact()
    {
        if (!canInteract)
        {
            return;
        }
        
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayInteractionSFX();
        }

        if (sliderPanel != null)
        {
            sliderPanel.OpenPanel(this);
            if (questLookTarget != null)
            {
                questLookTarget.enabled = false;
            }
        }
        else
        {
            Debug.LogError("UI_SliderPanel non assegnato!");
        }
    }

    public void SetPosition(float normalizedPosition)
    {
        currentPosition = Mathf.Clamp01(normalizedPosition);
        UpdateSliderPosition(currentPosition);
    }

    public void MoveSlider(float delta)
    {
        if (isPlaying) return;

        currentPosition = Mathf.Clamp01(currentPosition + (delta * moveSpeed));
        UpdateSliderPosition(currentPosition);
    }

    public void RotateCamera(float horizontal, float vertical)
    {
        if (sliderCamera == null || isPlaying) return;

        Transform camTransform = sliderCamera.transform;

        camTransform.Rotate(Vector3.up, horizontal * cameraRotationSpeed * Time.deltaTime, Space.World);
    }

    private void UpdateSliderPosition(float t)
    {
        if (sliderCart == null || railStart == null || railEnd == null)
        {
            Debug.LogWarning("Slider references non configurate!");
            return;
        }

        Vector3 newPosition = Vector3.Lerp(railStart.position, railEnd.position, t);
        sliderCart.position = newPosition;
    }

    public void StartRecording()
    {
        if (isRecording || isPlaying) return;

        currentRecording = new SliderRecording($"Recording_{System.DateTime.Now:HHmmss}");
        isRecording = true;
        recordingStartTime = Time.time;

        lookTimer = 0f;
        livingRoomSliderCompleted = false;
        if (questLookTarget != null)
        {
            questLookTarget.enabled = true;
        }

        Debug.Log($"Recording STARTED: {currentRecording.recordingName}");
    }

    public void StopRecording()
    {
        if (!isRecording) return;

        isRecording = false;

        if (questLookTarget != null)
        {
            questLookTarget.enabled = false;
        }

        CheckSliderCompletion();
    }

    // private void UpdateRecording()
    // {
    //     float elapsedTime = Time.time - recordingStartTime;

    //     if (currentRecording.keyframes.Count == 0 || 
    //         elapsedTime - currentRecording.duration >= recordingSampleRate)
    //     {
    //         Quaternion cameraRot = sliderCamera != null ? sliderCamera.transform.localRotation : Quaternion.identity;
    //         currentRecording.AddKeyframe(elapsedTime, currentPosition, cameraRot);
    //     }

    //     if (QuestManager.instance != null && 
    //         QuestManager.instance.IsQuestActive(QuestManager.MainQuest.LivingRoomSlider))
    //     {
    //         if (IsLookingAtTarget())
    //         {
    //             lookTimer += Time.deltaTime;
    //             Debug.Log($"SLIDER QUEST lookTimer={lookTimer:F2}s");
    //         }
    //     }
    // }

    private void UpdateRecording()
    {
        float elapsedTime = Time.time - recordingStartTime;

        if (elapsedTime >= maxRecordingDuration)
        {
            Debug.LogWarning($"Registrazione fermata: durata massima ({maxRecordingDuration}s) superata!");
            StopRecording();
            QuestManager.instance.ShowMessage("DURATA MASSIMA DI REGISTRAZIONE RAGGIUNTA!");
            return;
        }

        if (currentRecording.keyframes.Count == 0 || 
            elapsedTime - currentRecording.duration >= recordingSampleRate)
        {
            Quaternion cameraRot = sliderCamera != null ? sliderCamera.transform.localRotation : Quaternion.identity;
            currentRecording.AddKeyframe(elapsedTime, currentPosition, cameraRot);
        }

        if (QuestManager.instance != null && 
            QuestManager.instance.IsQuestActive(QuestManager.MainQuest.LivingRoomSlider))
        {
            if (IsLookingAtTarget())
            {
                lookTimer += Time.deltaTime;
                Debug.Log($"SLIDER QUEST lookTimer={lookTimer:F2}s");
            }
        }
    }

    private void CheckSliderCompletion()
    {
        // tutorial: deve aver creato almeno 1 keyframe
        if (QuestManager.instance != null && 
            QuestManager.instance.IsQuestActive(QuestManager.MainQuest.TutorialSlider) &&
            !tutorialQuestCompleted)
        {
            if (currentRecording != null && currentRecording.keyframes.Count >= 1)
            {
                tutorialQuestCompleted = true;
                QuestManager.instance.CompleteCurrentQuest();
            }
        }

        // salotto: deve guardare verso il target per un tot di secondi durante la registrazione
        if (livingRoomSliderCompleted) return;

        if (QuestManager.instance != null && QuestManager.instance.IsQuestActive(QuestManager.MainQuest.LivingRoomSlider))
        {
            if (lookTimer >= QuestManager.instance.LookDurationRequired)
            {
                livingRoomSliderCompleted = true;
                Debug.Log($"Quest slider salotto COMPLETATA! Guardato verso scena per {lookTimer:F2}s");
                QuestManager.instance.CompleteCurrentQuest();
            }
            else
            {
                QuestManager.instance.ShowMessage("Devi guardare verso la scena per almeno 3 secondi per completare l'incarico!");
            }
        }
    }

    public void StartPlayback()
    {
        if (currentRecording == null || currentRecording.keyframes.Count == 0)
        {
            Debug.LogWarning("Nessuna registrazione da riprodurre!");
            return;
        }

        if (isRecording || isPlaying) return;

        isPlaying = true;
        playbackStartTime = Time.time;
        currentPlaybackIndex = 0;

        Debug.Log($"Playback STARTED: {currentRecording.recordingName}");
    }

    public void StopPlayback()
    {
        if (!isPlaying) return;

        isPlaying = false;
        currentPlaybackIndex = 0;

        Debug.Log("Playback STOPPED");
    }

    private void UpdatePlayback()
    {
        float elapsedTime = Time.time - playbackStartTime;

        // fine playback
        if (elapsedTime >= currentRecording.duration)
        {
            StopPlayback();
            return;
        }

        // trova keyframes da interpolare
        while (currentPlaybackIndex < currentRecording.keyframes.Count - 1 &&
               currentRecording.keyframes[currentPlaybackIndex + 1].time <= elapsedTime)
        {
            currentPlaybackIndex++;
        }

        // interpola tra keyframes
        if (currentPlaybackIndex < currentRecording.keyframes.Count - 1)
        {
            SliderKeyframe current = currentRecording.keyframes[currentPlaybackIndex];
            SliderKeyframe next = currentRecording.keyframes[currentPlaybackIndex + 1];

            float t = Mathf.InverseLerp(current.time, next.time, elapsedTime);

            // interpola posizione
            currentPosition = Mathf.Lerp(current.position, next.position, t);
            UpdateSliderPosition(currentPosition);

            // interpola rotazione camera
            if (sliderCamera != null)
            {
                sliderCamera.transform.localRotation = Quaternion.Slerp(current.cameraRotation, next.cameraRotation, t);
            }
        }
        else
        {
            SliderKeyframe last = currentRecording.keyframes[currentPlaybackIndex];
            currentPosition = last.position;
            UpdateSliderPosition(currentPosition);

            if (sliderCamera != null)
            {
                sliderCamera.transform.localRotation = last.cameraRotation;
            }
        }
    }

    public void ClearRecording()
    {
        if (isRecording) StopRecording();
        if (isPlaying) StopPlayback();

        currentRecording = null;
        tutorialQuestCompleted = false;
        Debug.Log("Recording cancellata");
    }

    public float GetDistanceInMeters()
    {
        if (railStart == null || railEnd == null) return 0f;
        
        float totalLength = Vector3.Distance(railStart.position, railEnd.position);
        return totalLength * currentPosition;
    }

    public float GetTotalLength()
    {
        if (railStart == null || railEnd == null) return 0f;
        return Vector3.Distance(railStart.position, railEnd.position);
    }

    private bool IsLookingAtTarget()
    {
        if (sliderCamera == null || questLookTarget == null) return false;

        Camera cam = sliderCamera.GetComponentInChildren<Camera>();
        if (cam == null) return false;
        
        RaycastHit hit;

        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, raycastDistance))
        {
            if (hit.collider == questLookTarget)
            {
                Debug.DrawRay(cam.transform.position, cam.transform.forward * hit.distance, Color.red, 0.15f);
                return true;
            }
        }
        return false;
    }
    
    public string GetInteractionText()
    {
        if (!canInteract)
        {
            return $"{sliderName} (Non disponibile)";
        }
        return interactionText;
    }

    public UI_SliderPanel GetSliderPanel()
    {
        return sliderPanel;
    }

    public string GetSliderName()
    {
        return sliderName;
    }
}