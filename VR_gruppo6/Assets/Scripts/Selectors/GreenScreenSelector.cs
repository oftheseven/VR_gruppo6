using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// codice per la selezione di immagini tramite frecce direzionali
public class GreenScreenSelector : MonoBehaviour
{
    [Header("Green screen images and references")]
    [SerializeField] private RawImage[] images;

    [Header("Input settings")]
    [SerializeField] private float inputCooldown = 0.1f;

    private int currentImageIndex = 0;
    private float lastInputTime = 0f;
    private UI_ComputerPanel computerPanel;
    private GreenScreenTarget currentTarget;

    void Start()
    {
        computerPanel = GetComponent<UI_ComputerPanel>();
        UpdateImageColors();
    }

    void Update()
    {
        if (computerPanel != null && computerPanel.IsOpen)
        {
            HandleImageSelection();
            HandleConfirmation();
        }
    }

    public void SetTarget(GreenScreenTarget target)
    {
        currentTarget = target;
        currentImageIndex = 0;
        UpdateImageColors();
        
        Debug.Log($"Target green screen impostato: {target.displayName}");
    }

    private void HandleImageSelection()
    {
        if (Time.time < lastInputTime + inputCooldown)
        {
            return;
        }
        
        bool inputDetected = false;

        if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            currentImageIndex = (currentImageIndex + 1) % images.Length;
            inputDetected = true;
        }
        else if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            currentImageIndex = (currentImageIndex - 1 + images.Length) % images.Length;
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

    // private void ApplyCurrentImage()
    // {
    //     RawImage selectedImage = images[currentImageIndex];
    //     if (selectedImage != null)
    //     {
    //         objectRenderer.material.color = Color.white;
    //         objectRenderer.material.mainTexture = selectedImage.texture;
    //     }
    //     else
    //     {
    //         Debug.LogWarning("L'immagine all'indice " + currentImageIndex + " è null.");
    //     }
    // }

    private void ApplyCurrentImage()
    {
        RawImage selectedImage = images[currentImageIndex];
        
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
            QuestManager.instance.IsQuestActive(QuestManager.TutorialQuest.GreenScreen))
        {
            Debug.Log("Tutorial GreenScreen completato!");
            QuestManager.instance.CompleteCurrentQuest();
        }
        
        // tortaintesta: deve essere l'immagine corretta
        if (TortaInTestaManager.instance != null && currentTarget != null)
        {
            if (currentTarget.correctImageIndex != -1 && 
                currentImageIndex == currentTarget.correctImageIndex)
            {
                currentTarget.isCompleted = true;
                TortaInTestaManager.instance.OnComputerImageCorrect(currentTarget.id);
                
                Debug.Log($"Immagine corretta per {currentTarget.displayName}!");
            }
            else if (currentTarget.correctImageIndex != -1)
            {
                Debug.Log($"Immagine sbagliata per {currentTarget.displayName}");
            }
        }
    }

    private void UpdateImageColors()
    {
        for (int i = 0; i < images.Length; i++)
        {
            if (i == currentImageIndex)
            {
                images[i].color = Color.green;
            }
            else
            {
                images[i].color = Color.white;
            }
        }
    }
}
