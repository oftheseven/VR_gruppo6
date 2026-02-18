// using UnityEngine;
// using UnityEngine.InputSystem;
// using UnityEngine.UI;

// // codice per la selezione delle lenti tramite frecce direzionali
// public class LensesSelector : MonoBehaviour
// {
//     [Header("Lenses images and references")]
//     [SerializeField] private RawImage[] images; // immagini delle lenti nell'UI
//     [SerializeField] private CameraLens[] cameraLenses; // reference agli script delle lenti

//     [Header("Input settings")]
//     [SerializeField] private float inputCooldown = 0.2f;

//     private int currentImageIndex = 2;
//     private float lastInputTime = 0f;
//     private UI_CameraPanel cameraPanel;
//     private bool tutorialQuestCompleted = false;

//     void Start()
//     {
//         cameraPanel = GetComponent<UI_CameraPanel>();
//         UpdateImageColors();
//     }

//     void Update()
//     {
//         if (cameraPanel != null && cameraPanel.IsOpen)
//         {
//             HandleImageSelection();
//             HandleConfirmation();
//         }
//     }

//     private void HandleImageSelection()
//     {
//         if (Time.time < lastInputTime + inputCooldown)
//         {
//             return;
//         }
        
//         bool inputDetected = false;

//         if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
//         {
//             currentImageIndex = (currentImageIndex + 1) % images.Length;
//             inputDetected = true;
//         }
//         else if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
//         {
//             currentImageIndex = (currentImageIndex - 1 + images.Length) % images.Length;
//             inputDetected = true;
//         }

//         if (inputDetected)
//         {
//             lastInputTime = Time.time;
//             UpdateImageColors();
//         }
//     }

//     private void HandleConfirmation()
//     {
//         if (Keyboard.current.enterKey.wasPressedThisFrame)
//         {
//             ApplyCurrentLens();
//         }
//     }

//     private void ApplyCurrentLens()
//     {
//         if (cameraPanel == null || cameraPanel.InteractableCamera == null)
//         {
//             Debug.LogError("Camera reference mancante!");
//             return;
//         }

//         Camera viewCamera = cameraPanel.InteractableCamera.ViewCamera;
        
//         if (viewCamera == null)
//         {
//             Debug.LogError("ViewCamera è null!");
//             return;
//         }

//         for (int i = 0; i < cameraLenses.Length; i++)
//         {
//             if (i == currentImageIndex)
//             {
//                 cameraLenses[i].gameObject.SetActive(true);
//                 cameraLenses[i].ApplyToCamera(viewCamera);
//                 if (AudioManager.instance != null)
//                 {
//                     AudioManager.instance.PlayCameraLensChange();
//                 }
//             }
//             else
//             {
//                 cameraLenses[i].gameObject.SetActive(false);
//             }
//         }

//         CheckTutorialCompletion();

//         CheckLivingRoomQuest();
//     }

//     private void CheckTutorialCompletion()
//     {
//         // tutorial: Qualsiasi lente va bene, basta cambiarla
//         if (QuestManager.instance != null && 
//             QuestManager.instance.IsQuestActive(QuestManager.MainQuest.TutorialCamera) &&
//             !tutorialQuestCompleted)
//         {
//             tutorialQuestCompleted = true;
//             QuestManager.instance.CompleteCurrentQuest();
//         }
//     }

//     private void CheckLivingRoomQuest()
//     {
//         if (QuestManager.instance != null && 
//             QuestManager.instance.IsQuestActive(QuestManager.MainQuest.LivingRoomCamera) &&
//             QuestManager.instance.LivingLensIndex == currentImageIndex) // indice della lente corretta per la quest del salotto
//         {
//             Debug.Log("Quest camera salotto COMPLETATA: lente corretta selezionata");
//             QuestManager.instance.CompleteCurrentQuest();
//         }
//     }

//     private void UpdateImageColors()
//     {
//         for (int i = 0; i < images.Length; i++)
//         {
//             if (i == currentImageIndex)
//             {
//                 images[i].color = Color.green;
//             }
//             else
//             {
//                 images[i].color = Color.white;
//             }
//         }
//     }

//     public void ResetTutorialFlag()
//     {
//         tutorialQuestCompleted = false;
//     }
// }

using UnityEngine;
using UnityEngine.UI;

public class LensesSelector : MonoBehaviour
{
    [Header("Lenses UI")]
    [SerializeField] private Button[] lensButtons;
    [SerializeField] private RawImage[] images;
    [SerializeField] private CameraLens[] cameraLenses;

    private UI_CameraPanel cameraPanel;
    private bool tutorialQuestCompleted = false;

    void Start()
    {
        cameraPanel = GetComponent<UI_CameraPanel>();
        for (int i = 0; i < lensButtons.Length; i++)
        {
            int idx = i;
            lensButtons[i].onClick.AddListener(() => ApplyLens(idx));
        }
        UpdateImageColors(-1);
    }

    private void ApplyLens(int lensIndex)
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
            cameraLenses[i].gameObject.SetActive(i == lensIndex);
            if (i == lensIndex)
                cameraLenses[i].ApplyToCamera(viewCamera);
        }

        if (AudioManager.instance != null)
            AudioManager.instance.PlayCameraLensChange();

        UpdateImageColors(lensIndex);

        CheckTutorialCompletion(lensIndex);
        CheckLivingRoomQuest(lensIndex);
    }

    private void CheckTutorialCompletion(int lensIndex)
    {
        if (QuestManager.instance != null && 
            QuestManager.instance.IsQuestActive(QuestManager.MainQuest.TutorialCamera) &&
            !tutorialQuestCompleted)
        {
            tutorialQuestCompleted = true;
            QuestManager.instance.CompleteCurrentQuest();
        }
    }

    private void CheckLivingRoomQuest(int lensIndex)
    {
        if (QuestManager.instance != null && 
            QuestManager.instance.IsQuestActive(QuestManager.MainQuest.LivingRoomCamera) &&
            QuestManager.instance.LivingLensIndex == lensIndex)
        {
            Debug.Log("Quest camera salotto COMPLETATA: lente corretta selezionata");
            QuestManager.instance.CompleteCurrentQuest();
        }
    }

    private void UpdateImageColors(int selectedIndex)
    {
        for (int i = 0; i < images.Length; i++)
        {
            images[i].color = (i == selectedIndex) ? Color.green : Color.white;
        }
    }

    public void ResetTutorialFlag()
    {
        tutorialQuestCompleted = false;
        UpdateImageColors(-1);
    }
}