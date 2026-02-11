using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// codice per la selezione di immagini tramite frecce direzionali
public class GreenScreenSelector : MonoBehaviour
{
    [Header("Green screen images and references")]
    [SerializeField] private RawImage[] images;
    [SerializeField] private Renderer objectRenderer;

    [Header("Input settings")]
    [SerializeField] private float inputCooldown = 0.1f;

    [Header("Correct image index")]
    [SerializeField] private int correctImageIndex = 0; // indice dell'immagine corretta da scegliere
    [SerializeField] private string computerID = "computer1";

    private int currentImageIndex = 0;
    private float lastInputTime = 0f;
    private UI_ComputerPanel computerPanel;
    private bool imageConfirmed = false;

    void Start()
    {
        if (objectRenderer == null)
        {
            Debug.LogError("Manca la reference al renderer a cui applicare l'immagine!");
            return;
        }

        computerPanel = GetComponent<UI_ComputerPanel>();

        if (computerPanel != null)
        {
            computerPanel.SetComputerID(computerID);
        }

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
            CheckImageCorrectness();
            computerPanel.CloseComputerImmediate();
        }
    }

    private void ApplyCurrentImage()
    {
        RawImage selectedImage = images[currentImageIndex];
        if (selectedImage != null)
        {
            objectRenderer.material.color = Color.white;
            objectRenderer.material.mainTexture = selectedImage.texture;
        }
        else
        {
            Debug.LogWarning("L'immagine all'indice " + currentImageIndex + " è null.");
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

    private void CheckImageCorrectness()
    {
        if (currentImageIndex == correctImageIndex)
        {
            imageConfirmed = true;

            Debug.Log($"Immagine corretta selezionata su {computerID}");

            if (TortaInTestaManager.instance != null)
            {
                TortaInTestaManager.instance.OnComputerImageCorrect(computerID);
            }
        }
        else
        {
            Debug.Log($"Immagine sbagliata selezionata su {computerID}");
        }
    }

    public bool IsCompleted => imageConfirmed;
}
