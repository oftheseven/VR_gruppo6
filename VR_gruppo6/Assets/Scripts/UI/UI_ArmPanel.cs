using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro;

public class UI_ArmPanel : MonoBehaviour
{
    [Header("Camera timer settings")]
    [SerializeField] private float holdTimeToClose = 2f;
    [SerializeField] private float cooldownTime = 1f;

    [Header("Info panel reference")]
    [SerializeField] private UI_InfoPanel infoPanel; // reference al pannello info specifico per questa camera

    [Header("Hold to close UI")]
    [SerializeField] private GameObject holdIndicator; // container del cerchio
    [SerializeField] private Image holdFillImage; // image con fill radial

    [Header("Arm reference")]
    [SerializeField] private InteractableArm interactableArm; // reference all'InteractableArm da far muovere
    [SerializeField] private Camera armCamera; // reference alla camera del braccio meccanico usata per muoverlo

    [Header("Waypoint System")]
    [SerializeField] private bool enableWaypointChallenge = true;

    [Header("Pivot selection UI")]
    [SerializeField] private TextMeshProUGUI selectedPivotText; // testo che mostra il pivot selezionato

    [Header("Visual effect pivot selection")]
    [SerializeField] private Color selectedColor = Color.green;
    [SerializeField] private float outlineWidth = 5f;

    [Header("Accuracy feedback reference")]
    [SerializeField] private UI_AccuracyFeedback accuracyFeedback;

    private bool isOpen = false;
    public bool IsOpen => isOpen;
    private bool canInteract = true;
    public bool CanInteract => canInteract;
    private float holdTimer = 0f;
    public Camera ArmCamera => armCamera;
    public bool EnableWaypointChallenge => enableWaypointChallenge;

    private enum PivotSelection
    {
        Base,
        Pivot1,
        Pivot2
    }

    private PivotSelection currentSelection = PivotSelection.Base;

    private float targetPivot1X = 0f;
    private float targetPivot2X = 0f;

    void Start()
    {
        this.gameObject.SetActive(false);
        selectedPivotText.gameObject.SetActive(false);
        armCamera.gameObject.SetActive(false);
        canInteract = true;

        if (holdIndicator != null)
        {
            holdIndicator.SetActive(false);
        }

        if (holdFillImage != null)
        {
            holdFillImage.fillAmount = 0;
        }

        if (accuracyFeedback == null)
        {
            accuracyFeedback = UI_AccuracyFeedback.instance;
            if (accuracyFeedback != null)
            {
                // Debug.Log("AccuracyFeedback trovato via singleton");
            }
            else
            {
                // Debug.LogWarning("AccuracyFeedback non trovato!");
            }
        }

        SetupOutlines();
    }

    void Update()
    {
        if (isOpen)
        {
            HandlePivotSelection();
            HandleArmMovement();
        }
    }

    public void OpenArm()
    {
        this.gameObject.SetActive(true);
        selectedPivotText.gameObject.SetActive(true);
        isOpen = true;
        PlayerController.EnableMovement(false);

        PlayerController.instance.playerCamera.gameObject.SetActive(false);
        armCamera.gameObject.SetActive(true);

        PlayerController.ShowCursor();

        currentSelection = PivotSelection.Base;
        UpdateSelectionUI();
        UpdateSelectionHighlight();

        if (infoPanel != null)
        {
            infoPanel.OnDeviceOpened();
        }
    
        if (enableWaypointChallenge && WaypointManager.instance != null)
        {
            WaypointManager.instance.StartWaypointChallenge();
        }
    
        if (ArmAccuracyTracker.instance != null)
        {
            ArmAccuracyTracker.instance.StartTracking();
        }

        Transform armTip = FindArmTip();

        if (ArmMovementRecorder.instance != null && armTip != null)
        {
            ArmMovementRecorder.instance.StartRecording(interactableArm, armTip);
        }
    }

    public void CloseArm()
    {
        isOpen = false;
        holdTimer = 0f;
    
        DisableAllOutlines();

        if (holdIndicator != null)
        {
            holdIndicator.SetActive(false);
        }

        if (infoPanel != null)
        {
            infoPanel.OnDeviceClosed();
        }
    
        float timeTaken = 0f;
        if (ArmAccuracyTracker.instance != null)
        {
            timeTaken = ArmAccuracyTracker.instance.SessionDuration;
            ArmAccuracyTracker.instance.StopTracking();
        }
    
        if (enableWaypointChallenge && WaypointManager.instance != null)
        {
            WaypointManager.instance.StopWaypointChallenge();

            AccuracyResults results = WaypointManager.instance.CalculateFinalScore();

            if (accuracyFeedback == null)
            {
                accuracyFeedback = UI_AccuracyFeedback.instance;
                // Debug.Log($"Cercato via singleton - trovato: {(accuracyFeedback != null ? "OK" : "NULL")}");
            }

            if (accuracyFeedback != null)
            {
                accuracyFeedback.ShowResults(results, timeTaken, interactableArm, this);
            }
            else
            {
                // Debug.LogError("accuracyFeedback è NULL! Non posso mostrare risultati.");
            }
        }
    
        StartCoroutine(CooldownAndHide());
        this.gameObject.SetActive(false);
        selectedPivotText.gameObject.SetActive(false);
        canInteract = true;
        PlayerController.instance.playerCamera.gameObject.SetActive(true);
        // PlayerController.EnableMovement(true);
        armCamera.gameObject.SetActive(false);
        PlayerController.HideCursor();
    }

    public void HandleArmClose()
    {
        if (Keyboard.current.eKey.isPressed && !infoPanel.IsOpen)
        {
            holdTimer += Time.deltaTime;

            // mostro l'indicatore quando inizio a premere
            if (holdIndicator != null && !holdIndicator.activeSelf)
            {
                holdIndicator.SetActive(true);
            }

            // aggiorno il fill dell'immagine radiale
            if (holdFillImage != null)
            {
                holdFillImage.fillAmount = Mathf.Clamp01(holdTimer / holdTimeToClose);
            }
            
            if (holdTimer >= holdTimeToClose)
            {
                CloseArm();
            }
        }
        else
        {
            // resetto quando rilascio il tasto
            holdTimer = 0f;

            if (holdIndicator != null)
            {
                holdIndicator.SetActive(false);
            }

            if (holdFillImage != null)
            {
                holdFillImage.fillAmount = 0;
            }
        }
    }

    private IEnumerator CooldownAndHide()
    {
        canInteract = false;
        //Debug.Log("Cooldown iniziato - canInteract = " + canInteract);
        yield return new WaitForSeconds(cooldownTime);
    }

    private void HandlePivotSelection()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            currentSelection = PivotSelection.Base;
            UpdateSelectionUI();
            UpdateSelectionHighlight();
        }
        else if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            currentSelection = PivotSelection.Pivot1;
            UpdateSelectionUI();
            UpdateSelectionHighlight();
        }
        else if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            currentSelection = PivotSelection.Pivot2;
            UpdateSelectionUI();
            UpdateSelectionHighlight();
        }
    }

    private void UpdateSelectionUI()
    {
        if (selectedPivotText != null)
        {
            switch (currentSelection)
            {
                case PivotSelection.Base:
                    selectedPivotText.text = "Selezionato: Base";
                    break;
                case PivotSelection.Pivot1:
                    selectedPivotText.text = "Selezionato: Pivot 1";
                    break;
                case PivotSelection.Pivot2:
                    selectedPivotText.text = "Selezionato: Pivot 2";
                    break;
            }
        }
    }

    private void UpdateSelectionHighlight()
    {
        DisableAllOutlines();

        switch (currentSelection)
        {
            case PivotSelection.Base:
                if (interactableArm.MechanicalArmPivot != null)
                    SetOutlinesEnabled(interactableArm.MechanicalArmPivot, true);
                break;
            case PivotSelection.Pivot1:
                if (interactableArm.Pivot1 != null)
                    SetOutlinesEnabled(interactableArm.Pivot1, true);
                break;
            case PivotSelection.Pivot2:
                if (interactableArm.Pivot2 != null)
                    SetOutlinesEnabled(interactableArm.Pivot2, true);
                break;
        }
    }

    private void SetOutlinesEnabled(GameObject parent, bool enabled)
    {
        Outline[] outlines = parent.GetComponentsInChildren<Outline>();
        foreach (Outline outline in outlines)
        {
            outline.enabled = enabled;
        }
    }

    private void DisableAllOutlines()
    {
        if (interactableArm.MechanicalArmPivot != null)
            SetOutlinesEnabled(interactableArm.MechanicalArmPivot, false);

        if (interactableArm.Pivot1 != null)
            SetOutlinesEnabled(interactableArm.Pivot1, false);

        if (interactableArm.Pivot2 != null)
            SetOutlinesEnabled(interactableArm.Pivot2, false);
    }
    
    private void SetupOutlines()
    {
        if (interactableArm.MechanicalArmPivot != null)
        {
            GetOrAddOutline(interactableArm.MechanicalArmPivot);
        }

        if (interactableArm.Pivot1 != null)
        {
            GetOrAddOutline(interactableArm.Pivot1);
        }

        if (interactableArm.Pivot2 != null)
        {
            GetOrAddOutline(interactableArm.Pivot2);
        }

        DisableAllOutlines();
    }

    private void GetOrAddOutline(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
    
        foreach (Renderer renderer in renderers)
        {
            Outline outline = renderer.gameObject.GetComponent<Outline>();
            if (outline == null)
            {
                outline = renderer.gameObject.AddComponent<Outline>();
            }

            outline.OutlineColor = selectedColor;
            outline.OutlineWidth = outlineWidth;
            outline.enabled = false;
        }
    }

    // funzione che gestisce le varie rotazioni del braccio meccanico
    private void HandleArmMovement()
    {
        if (currentSelection == PivotSelection.Base && interactableArm.MechanicalArmPivot != null)
        {
            if (Keyboard.current.rightArrowKey.isPressed)
            {
                interactableArm.MechanicalArmPivot.transform.Rotate(Vector3.up * 20f * Time.deltaTime);
            }
            else if (Keyboard.current.leftArrowKey.isPressed)
            {
                interactableArm.MechanicalArmPivot.transform.Rotate(Vector3.up * -20f * Time.deltaTime);
            }
        }
 
        if (currentSelection == PivotSelection.Pivot1 && interactableArm.Pivot1 != null)
        {
            if (Keyboard.current.upArrowKey.isPressed)
            {
                targetPivot1X -= interactableArm.RotationSpeed * Time.deltaTime;
            }
            else if (Keyboard.current.downArrowKey.isPressed)
            {
                targetPivot1X += interactableArm.RotationSpeed * Time.deltaTime;
            }

            targetPivot1X = Mathf.Clamp(targetPivot1X, interactableArm.MinPivot1X, interactableArm.MaxPivot1X);
            Quaternion targetRotation = Quaternion.Euler(targetPivot1X, 0, 0);
            interactableArm.Pivot1.transform.localRotation = Quaternion.Lerp(
                interactableArm.Pivot1.transform.localRotation,
                targetRotation,
                Time.deltaTime * 5f
            );
        }

        if (currentSelection == PivotSelection.Pivot2 && interactableArm.Pivot2 != null)
        {
            if (Keyboard.current.upArrowKey.isPressed)
            {
                targetPivot2X -= interactableArm.RotationSpeed * Time.deltaTime;
            }
            else if (Keyboard.current.downArrowKey.isPressed)
            {
                targetPivot2X += interactableArm.RotationSpeed * Time.deltaTime;
            }

            targetPivot2X = Mathf.Clamp(targetPivot2X, interactableArm.MinPivot2X, interactableArm.MaxPivot2X);
            Quaternion targetRotation = Quaternion.Euler(targetPivot2X, 0, 0);
            interactableArm.Pivot2.transform.localRotation = Quaternion.Lerp(
                interactableArm.Pivot2.transform.localRotation,
                targetRotation,
                Time.deltaTime * 5f
            );
        }
    }

    private Transform FindArmTip()
    {
        if (interactableArm == null)
        {
            Debug.LogError("interactableArm è null!");
            return null;
        }
    
        Transform armTip = interactableArm.transform.Find("ArmTip");
        
        if (armTip == null)
        {
            armTip = interactableArm.GetComponentInChildren<Transform>().Find("ArmTip");
        }
    
        if (armTip == null)
        {
            foreach (Transform child in interactableArm.GetComponentsInChildren<Transform>())
            {
                if (child.name == "ArmTip")
                {
                    armTip = child;
                    break;
                }
            }
        }
    
        if (armTip != null)
        {
            Debug.Log($"ArmTip trovato: {armTip.name}");
        }
        else
        {
            Debug.LogError($"ArmTip NON trovato nei children di {interactableArm.name}");
        }
    
        return armTip;
    }
}