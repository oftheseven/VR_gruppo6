using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// codice per la selezione delle lenti tramite frecce direzionali
public class LensesSelector : MonoBehaviour
{
    [Header("Lenses images and references")]
    [SerializeField] private RawImage[] images; // immagini delle lenti nell'UI
    [SerializeField] private CameraLens[] cameraLenses; // reference agli script delle lenti

    [Header("Correct lens index for task completion")]
    [SerializeField] private int correctLensIndex = 2; // indice della lente corretta per completare la missione

    [Header("Input settings")]
    [SerializeField] private float inputCooldown = 0.2f;

    private int currentImageIndex = 0;
    private float lastInputTime = 0f;
    private UI_CameraPanel cameraPanel;

    void Start()
    {
        cameraPanel = GetComponent<UI_CameraPanel>();
        UpdateImageColors();
    }

    void Update()
    {
        if (cameraPanel != null && cameraPanel.IsOpen)
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

        if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            currentImageIndex = (currentImageIndex + 1) % images.Length;
            inputDetected = true;
        }
        else if (Keyboard.current.upArrowKey.wasPressedThisFrame)
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
            ApplyCurrentLens();
        }
    }

    private void ApplyCurrentLens()
    {
        if (cameraPanel == null || cameraPanel.InteractableCamera == null)
        {
            Debug.LogError("Camera reference mancante!");
            return;
        }

        Camera viewCamera = cameraPanel.InteractableCamera.ViewCamera;
        
        if (viewCamera == null)
        {
            Debug.LogError("ViewCamera è null!");
            return;
        }

        for (int i = 0; i < cameraLenses.Length; i++)
        {
            if (i == currentImageIndex)
            {
                cameraLenses[i].gameObject.SetActive(true);
                cameraLenses[i].ApplyToCamera(viewCamera);
                if (GameManager.instance != null && currentImageIndex == correctLensIndex && SceneManager.GetActiveScene().name == "TortaInTesta")
                {
                    GameManager.instance.OnCameraCompleted();
                }
            }
            else
            {
                cameraLenses[i].gameObject.SetActive(false);
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