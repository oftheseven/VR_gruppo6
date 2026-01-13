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

    private int currentImageIndex = 0;
    private float lastInputTime = 0f;
    private UI_ComputerPanel computerPanel;

    void Start()
    {
        if (objectRenderer == null)
        {
            Debug.LogError("Manca la reference al renderer a cui applicare l'immagine!");
            return;
        }

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

            // controllo se l'immagine selezionata è quella corretta
            if (currentImageIndex == correctImageIndex)
            {
                UI_Screenplay.instance.AdvanceGreenScreenProgress();
            }
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
}
