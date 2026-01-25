using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using System.Runtime.InteropServices;
using TMPro;

public class UI_ArmPanel :  MonoBehaviour
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

    [Header("Pivot selection UI")]
    [SerializeField] private TextMeshProUGUI selectedPivotText; // testo che mostra il pivot selezionato

    [Header("Visual effect pivot selection")]
    [SerializeField] private Color selectedColor = Color.green;
    [SerializeField] private float outlineWidth = 5f;

    private bool isOpen = false;
    public bool IsOpen => isOpen;
    private bool canInteract = true;
    public bool CanInteract => canInteract;
    private float holdTimer = 0f;

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
        canInteract = true;

        if (holdIndicator != null)
        {
            holdIndicator.SetActive(false);
        }

        if (holdFillImage != null)
        {
            holdFillImage.fillAmount = 0;
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

        currentSelection = PivotSelection.Base;
        UpdateSelectionUI();
        UpdateSelectionHighlight();

        if (infoPanel != null)
        {
            infoPanel.OnDeviceOpened();
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
        
        StartCoroutine(CooldownAndHide());
        this.gameObject.SetActive(false);
        selectedPivotText.gameObject.SetActive(false);
        canInteract = true;
        PlayerController.EnableMovement(true);
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
                if (InteractableArm.instance.MechanicalArmPivot != null)
                    SetOutlinesEnabled(InteractableArm.instance.MechanicalArmPivot, true);
                break;
            case PivotSelection.Pivot1:
                if (InteractableArm.instance.Pivot1 != null)
                    SetOutlinesEnabled(InteractableArm.instance.Pivot1, true);
                break;
            case PivotSelection.Pivot2:
                if (InteractableArm.instance.Pivot2 != null)
                    SetOutlinesEnabled(InteractableArm.instance.Pivot2, true);
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
        if (InteractableArm.instance.MechanicalArmPivot != null)
            SetOutlinesEnabled(InteractableArm.instance.MechanicalArmPivot, false);

        if (InteractableArm.instance.Pivot1 != null)
            SetOutlinesEnabled(InteractableArm.instance.Pivot1, false);

        if (InteractableArm.instance.Pivot2 != null)
            SetOutlinesEnabled(InteractableArm.instance.Pivot2, false);
    }
    
    private void SetupOutlines()
    {
        if (InteractableArm.instance.MechanicalArmPivot != null)
        {
            GetOrAddOutline(InteractableArm.instance.MechanicalArmPivot);
        }

        if (InteractableArm.instance.Pivot1 != null)
        {
            GetOrAddOutline(InteractableArm.instance.Pivot1);
        }

        if (InteractableArm.instance.Pivot2 != null)
        {
            GetOrAddOutline(InteractableArm.instance.Pivot2);
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

            outline.effectColor = selectedColor;
            outline.effectDistance = new Vector2(outlineWidth, outlineWidth);
            outline.enabled = false;
        }
    }

    // funzione che gestisce le varie rotazioni del braccio meccanico
    private void HandleArmMovement()
    {
        if (currentSelection == PivotSelection.Base && InteractableArm.instance.MechanicalArmPivot != null)
        {
            if (Keyboard.current.rightArrowKey.isPressed)
            {
                InteractableArm.instance.MechanicalArmPivot.transform.Rotate(Vector3.up * 20f * Time.deltaTime);
            }
            else if (Keyboard.current.leftArrowKey.isPressed)
            {
                InteractableArm.instance.MechanicalArmPivot.transform.Rotate(Vector3.up * -20f * Time.deltaTime);
            }
        }
 
        if (currentSelection == PivotSelection.Pivot1 && InteractableArm.instance.Pivot1 != null)
        {
            if (Keyboard.current.upArrowKey.isPressed)
            {
                targetPivot1X += InteractableArm.instance.RotationSpeed * Time.deltaTime;
            }
            else if (Keyboard.current.downArrowKey.isPressed)
            {
                targetPivot1X -= InteractableArm.instance.RotationSpeed * Time.deltaTime;
            }

            targetPivot1X = Mathf.Clamp(targetPivot1X, InteractableArm.instance.MinPivot1X, InteractableArm.instance.MaxPivot1X);
            Quaternion targetRotation = Quaternion.Euler(targetPivot1X, 0, 0);
            InteractableArm.instance.Pivot1.transform.localRotation = Quaternion.Lerp(
                InteractableArm.instance.Pivot1.transform.localRotation,
                targetRotation,
                Time.deltaTime * 5f
            );
        }

        if (currentSelection == PivotSelection.Pivot2 && InteractableArm.instance.Pivot2 != null)
        {
            if (Keyboard.current.upArrowKey.isPressed)
            {
                targetPivot2X -= InteractableArm.instance.RotationSpeed * Time.deltaTime;
            }
            else if (Keyboard.current.downArrowKey.isPressed)
            {
                targetPivot2X += InteractableArm.instance.RotationSpeed * Time.deltaTime;
            }

            targetPivot2X = Mathf.Clamp(targetPivot2X, InteractableArm.instance.MinPivot2X, InteractableArm.instance.MaxPivot2X);
            Quaternion targetRotation = Quaternion.Euler(targetPivot2X, 0, 0);
            InteractableArm.instance.Pivot2.transform.localRotation = Quaternion.Lerp(
                InteractableArm.instance.Pivot2.transform.localRotation,
                targetRotation,
                Time.deltaTime * 5f
            );
        }
    }
}