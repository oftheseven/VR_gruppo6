using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// codice per la selezione di immagini tramite frecce direzionali
public class GreenScreenSelector : MonoBehaviour
{
    [Header("Green screen images and references")]
    [SerializeField] private RawImage[] imageSlots; // slot UI per mostrare le immagini disponibili

    [Header("Input settings")]
    [SerializeField] private float inputCooldown = 0.1f;

    [Header("Outline Settings")]
    [SerializeField] private Color outlineColorSelected = Color.green;
    [SerializeField] private float outlineDistance = 4f;

    private int currentImageIndex = 0;
    private float lastInputTime = 0f;
    private UI_ComputerPanel computerPanel;
    private GreenScreenTarget currentTarget;
    private UnityEngine.UI.Outline[] imageOutlines;

    void Start()
    {
        computerPanel = GetComponent<UI_ComputerPanel>();
        SetupOutlines();
    }

    void Update()
    {
        if (computerPanel != null && computerPanel.IsOpen)
        {
            HandleImageSelection();
            HandleConfirmation();
        }
    }

    private void SetupOutlines()
    {
        imageOutlines = new UnityEngine.UI.Outline[imageSlots.Length];
        
        for (int i = 0; i < imageSlots.Length; i++)
        {
            if (imageSlots[i] != null)
            {
                UnityEngine.UI.Outline outline = imageSlots[i].GetComponent<UnityEngine.UI.Outline>();
                if (outline == null)
                {
                    outline = imageSlots[i].gameObject.AddComponent<UnityEngine.UI.Outline>();
                }

                outline.effectColor = outlineColorSelected;
                outline.effectDistance = new Vector2(outlineDistance, outlineDistance);
                outline.enabled = false;
                imageOutlines[i] = outline;
            }
        }
    }

    public void SetTarget(GreenScreenTarget target)
    {
        if (target == null || !target.IsValid())
        {
            Debug.LogError("Target green screen invalido!");
            return;
        }

        currentTarget = target;
        currentImageIndex = 0;
        
        LoadTargetImages();
        
        UpdateImageColors();
    }

    private void LoadTargetImages()
    {
        if (currentTarget == null || currentTarget.availableImages == null)
        {
            Debug.LogWarning("Nessuna immagine disponibile per questo target!");
            return;
        }

        DisableAllOutlines();
        
        for (int i = 0; i < imageSlots.Length; i++)
        {
            if (imageSlots[i] != null)
            {
                if (i < currentTarget.availableImages.Length && currentTarget.availableImages[i] != null)
                {
                    imageSlots[i].texture = currentTarget.availableImages[i];
                    imageSlots[i].gameObject.SetActive(true);
                    imageSlots[i].color = Color.white;
                }
                else
                {
                    imageSlots[i].texture = null;
                    imageSlots[i].gameObject.SetActive(false);
                }
            }
        }
    }

    private void DisableAllOutlines()
    {
        if (imageOutlines == null) return;
        
        for (int i = 0; i < imageOutlines.Length; i++)
        {
            if (imageOutlines[i] != null)
            {
                imageOutlines[i].enabled = false;
            }
        }
    }

    private void HandleImageSelection()
    {
        if (Time.time < lastInputTime + inputCooldown)
        {
            return;
        }
        
        bool inputDetected = false;

        int validImageCount = currentTarget != null ? currentTarget.GetValidImageCount() : imageSlots.Length;

        if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            currentImageIndex = (currentImageIndex + 1) % validImageCount;
            inputDetected = true;
        }
        else if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            currentImageIndex = (currentImageIndex - 1 + validImageCount) % validImageCount;
            inputDetected = true;
        }

        if (inputDetected)
        {
            lastInputTime = Time.time;
            UpdateImageColors();
        }
    }

    private void HandleConfirmation()
    {
        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            ApplyCurrentImage();
            CheckCompletion();
            computerPanel.CloseComputerImmediate();
        }
    }

    private void ApplyCurrentImage()
    {
        RawImage selectedImage = imageSlots[currentImageIndex];
        
        if (selectedImage != null && currentTarget != null && currentTarget.targetRenderer != null)
        {
            currentTarget.targetRenderer.material.color = Color.white;
            currentTarget.targetRenderer.material.mainTexture = selectedImage.texture;
            
            Debug.Log($"Immagine {currentImageIndex} applicata a {currentTarget.displayName}");
            
            if (AudioManager.instance != null)
            {
                AudioManager.instance.PlayUIClick();
            }
        }
        else
        {
            Debug.LogWarning("Immagine o renderer mancante!");
        }
    }

    private void CheckCompletion()
    {
        // tutorial: qualsiasi immagine va bene
        if (QuestManager.instance != null && 
            QuestManager.instance.IsQuestActive(QuestManager.MainQuest.TutorialGreenScreen))
        {
            Debug.Log("Tutorial GreenScreen completato!");
            QuestManager.instance.CompleteCurrentQuest();
        }
        
        // // tortaintesta: deve essere l'immagine corretta
        // if (TortaInTestaManager.instance != null && currentTarget != null)
        // {
        //     if (currentTarget.correctImageIndex != -1 && 
        //         currentImageIndex == currentTarget.correctImageIndex)
        //     {
        //         currentTarget.isCompleted = true;
        //         // TortaInTestaManager.instance.OnComputerImageCorrect(currentTarget.id);
                
        //         Debug.Log($"Immagine corretta per {currentTarget.displayName}!");
        //     }
        //     else if (currentTarget.correctImageIndex != -1)
        //     {
        //         Debug.Log($"Immagine sbagliata per {currentTarget.displayName}");
        //     }
        // }
    }

    private void UpdateImageColors()
    {
        for (int i = 0; i < imageSlots.Length; i++)
        {
            if (imageSlots[i] != null && imageSlots[i].gameObject.activeSelf)
            {
                imageSlots[i].color = Color.white;
                
                if (imageOutlines != null && i < imageOutlines.Length && imageOutlines[i] != null)
                {
                    bool shouldEnable = (i == currentImageIndex);
                    imageOutlines[i].enabled = shouldEnable;
                }
            }
        }
    }
}
