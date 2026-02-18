using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

public class UI_GreenScreenPanel : MonoBehaviour
{
    [Header("Hold to close UI")]
    [SerializeField] private GameObject holdIndicator; // container del cerchio
    [SerializeField] private Image holdFillImage; // image con fill radial

    [Header("Computer timer settings")]
    [SerializeField] private float holdTimeToClose = 2f;
    [SerializeField] private float cooldownTime = 1f;

    [Header("Info panel reference")]
    [SerializeField] private UI_InfoPanel infoPanel;

    [Header("Green Screens gestiti da questo computer")]
    [SerializeField] private GreenScreenTarget[] greenScreens = new GreenScreenTarget[2];

    [Header("GS Selector Buttons")]
    [SerializeField] private Button gsSelectButton0;
    [SerializeField] private Button gsSelectButton1;

    [Header("Image selection buttons")]
    [SerializeField] private Button[] imageSelectButtons = new Button[4];

    [Header("Preview materials")]
    [SerializeField] private Material greenScreenMaterial;

    private int currentGSIndex = 0;
    private int currentImageIndex = -1;

    private bool isOpen = false;
    public bool IsOpen => isOpen;
    private bool canInteract = true;
    public bool CanInteract => canInteract;
    private float holdTimer = 0f;

    void Awake()
    {
        for (int i = 0; i < greenScreens.Length; i++)
        {
            if (greenScreens[i] != null && greenScreens[i].previewCamera != null)
                greenScreens[i].previewCamera.gameObject.SetActive(false);
        }
    }

    void Start()
    {
        this.gameObject.SetActive(false);
        gsSelectButton0.onClick.AddListener(() => SelectGS(0));
        gsSelectButton1.onClick.AddListener(() => SelectGS(1));
        canInteract = true;

        if (holdIndicator != null)
        {
            holdIndicator.SetActive(false);
        }

        if (holdFillImage != null)
        {
            holdFillImage.fillAmount = 0;
        }

        for (int i = 0; i < imageSelectButtons.Length; i++)
        {
            int idx = i;
            imageSelectButtons[i].onClick.AddListener(() => SelectImage(idx));
        }

        for (int i = 0; i < greenScreens.Length; i++)
        {
            if (greenScreens[i] != null && greenScreens[i].previewCamera != null)
                greenScreens[i].previewCamera.gameObject.SetActive(false);
            if (greenScreens[i] != null && greenScreens[i].targetRenderer != null && greenScreenMaterial != null)
                greenScreens[i].targetRenderer.material = greenScreenMaterial;
        }
    }

    public void SetGreenScreens(GreenScreenTarget[] screens) 
    {
        greenScreens = screens;
        for (int i = 0; i < greenScreens.Length; i++)
        {
            if (greenScreens[i] != null && greenScreens[i].previewCamera != null)
                greenScreens[i].previewCamera.gameObject.SetActive(false);
            if (greenScreens[i] != null && greenScreens[i].targetRenderer != null && greenScreenMaterial != null)
                greenScreens[i].targetRenderer.material = greenScreenMaterial;
        }
    }

    void Update()
    {
        if (isOpen)
        {
            HandleComputerClose();
        }

        if (Keyboard.current.tabKey.wasPressedThisFrame && infoPanel != null && !infoPanel.IsOpen)
        {
            infoPanel.OpenInfoPanel();
        }
        else if (Keyboard.current.tabKey.wasPressedThisFrame && infoPanel != null && infoPanel.IsOpen)
        {
            infoPanel.CloseInfoPanel();
        }
    }

    private void UpdateGSButtonSprites()
    {
        // bottone 1
        var img0 = gsSelectButton0.GetComponent<Image>();
        if (img0 != null)
            img0.sprite = (currentGSIndex == 0) ? gsSelectButton0.spriteState.pressedSprite : gsSelectButton0.spriteState.disabledSprite;

        // bottone 2
        var img1 = gsSelectButton1.GetComponent<Image>();
        if (img1 != null)
            img1.sprite = (currentGSIndex == 1) ? gsSelectButton1.spriteState.pressedSprite : gsSelectButton1.spriteState.disabledSprite;
    }

    public void OpenPanel()
    {
        this.gameObject.SetActive(true);
        isOpen = true;
        SelectGS(currentGSIndex);
        
        if (greenScreens[currentGSIndex] != null && greenScreens[currentGSIndex].previewCamera != null)
            greenScreens[currentGSIndex].previewCamera.gameObject.SetActive(true);
        PlayerController.EnableMovement(false);

        if (infoPanel != null)
        {
            infoPanel.OnDeviceOpened();
        }
        PlayerController.ShowCursor();
        PlayerController.instance.BasePanel.gameObject.SetActive(false);
    }

    private void SelectGS(int gsIdx)
    {
        currentGSIndex = gsIdx;
        
        for (int i = 0; i < greenScreens.Length; i++)
        {
            if (greenScreens[i] != null && greenScreens[i].previewCamera != null)
                greenScreens[i].previewCamera.gameObject.SetActive(i == gsIdx);
        }

        for (int i = 0; i < imageSelectButtons.Length; i++)
        {
            if (greenScreens[gsIdx] != null && greenScreens[gsIdx].availableImages.Length > i)
            {
                var imgComponent = imageSelectButtons[i].GetComponent<Image>();
                if (imgComponent != null)
                {
                    var tex = greenScreens[gsIdx].availableImages[i];
                    if (tex != null)
                    {
                        
                        Rect rect = new Rect(0, 0, tex.width, tex.height);
                        Vector2 pivot = new Vector2(0.5f, 0.5f);
                        imgComponent.sprite = Sprite.Create(tex, rect, pivot);
                        imgComponent.color = Color.white;
                    }
                    else
                    {
                        imgComponent.sprite = null;
                        imgComponent.color = Color.clear;
                    }
                }
            }

            if (greenScreens[gsIdx].previewRenderer != null)
                greenScreens[gsIdx].previewRenderer.gameObject.GetComponent<Renderer>().material.color = new Color(1, 1, 1, 0);
        }

        currentImageIndex = greenScreens[gsIdx].appliedImageIndex;
        UpdateImageHighlight();

        if (greenScreens[gsIdx].targetRenderer != null && greenScreenMaterial != null)
            greenScreens[gsIdx].targetRenderer.material = greenScreenMaterial;
    
        UpdateGSButtonSprites();
    }

    private void SelectImage(int imgIdx)
    {
        currentImageIndex = imgIdx;
        var gs = greenScreens[currentGSIndex];
        gs.appliedImageIndex = imgIdx;

        if (gs.previewRenderer != null && gs.availableImages.Length > imgIdx && gs.availableImages[imgIdx] != null)
        {
            gs.previewRenderer.gameObject.SetActive(true);
            if (gs.defaultMaterial != null)
            {
                gs.previewRenderer.material = gs.defaultMaterial;
            }
            gs.previewRenderer.material.mainTexture = gs.availableImages[imgIdx];
            gs.previewRenderer.material.color = new Color(1, 1, 1, 1);
        }

        UpdateImageHighlight();
    }

    private void UpdateImageHighlight()
    {
        for (int i = 0; i < imageSelectButtons.Length; i++)
        {
            var outline = imageSelectButtons[i].GetComponent<UnityEngine.UI.Outline>();
            if (outline != null)
                outline.enabled = (i == currentImageIndex);
        }
    }

    public void ClosePanel()
    {
        isOpen = false;
        holdTimer = 0f;

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
        canInteract = true;

        for (int i = 0; i < greenScreens.Length; i++)
        {
            if (greenScreens[i] != null && greenScreens[i].previewCamera != null)
                greenScreens[i].previewCamera.gameObject.SetActive(false);
            if (greenScreens[i]?.previewRenderer != null)
                greenScreens[i].previewRenderer.gameObject.SetActive(false);
        }

        for (int i = 0; i < greenScreens.Length; i++)
        {
            if (greenScreens[i] != null && greenScreens[i].targetRenderer != null && greenScreenMaterial != null)
            {
                greenScreens[i].targetRenderer.material = greenScreenMaterial;
                greenScreens[i].targetRenderer.material.mainTexture = null;
            }
        }

        PlayerController.EnableMovement(true);
        PlayerController.instance.BasePanel.gameObject.SetActive(true);
        PlayerController.HideCursor();
    }

    public void HandleComputerClose()
    {
        if (Keyboard.current.eKey.isPressed && !infoPanel.IsOpen)
        {
            holdTimer += Time.deltaTime;

            if (holdIndicator != null && !holdIndicator.activeSelf)
            {
                holdIndicator.SetActive(true);
            }

            if (holdFillImage != null)
            {
                holdFillImage.fillAmount = Mathf.Clamp01(holdTimer / holdTimeToClose);
            }
            
            if (holdTimer >= holdTimeToClose)
            {
                ClosePanel();
            }
        }
        else
        {
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
        yield return new WaitForSeconds(cooldownTime);
    }
}