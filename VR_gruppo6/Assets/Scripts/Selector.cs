using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// codice per la selezione di immagini tramite frecce direzionali
public class Selector : MonoBehaviour
{
    [Header("Green screen images and references")]
    [SerializeField] private RawImage[] images;
    [SerializeField] private Renderer objectRenderer;

    private int currentImageIndex = 0;
    private float inputCooldown = 0.2f;
    private float lastInputTime = 0f;

    void Start()
    {
        if (objectRenderer == null)
        {
            Debug.LogError("Manca la reference al renderer a cui applicare l'immagine!");
            return;
        }
    }

    void Update()
    {
        HandleImageSelection();
        HandleConfirmation();
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
            //ApplyCurrentImage();
        }
    }

    private void HandleConfirmation()
    {
        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            ApplyCurrentImage();
            UI_ComputerPanel.instance.CloseComputerImmediate();
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
}
