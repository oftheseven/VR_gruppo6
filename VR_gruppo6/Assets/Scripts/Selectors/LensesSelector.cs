using UnityEngine;
using UnityEngine.UI;

public class LensesSelector : MonoBehaviour
{
    [Header("Lenses UI")]
    [SerializeField] private Button[] lensButtons;
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
            
            var btnImage = lensButtons[i].GetComponent<Image>();
            var state = lensButtons[i].spriteState;
            if (btnImage != null)
                btnImage.sprite = (i == lensIndex && state.pressedSprite != null)
                    ? state.pressedSprite
                    : state.disabledSprite;
        }

        if (AudioManager.instance != null)
            AudioManager.instance.PlayCameraLensChange();

        CheckTutorialCompletion();
        CheckLivingRoomQuest(lensIndex);
        CheckDivinationRoomQuest(lensIndex);
    }

    private void CheckTutorialCompletion()
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

    private void CheckDivinationRoomQuest(int lensIndex)
    {
        if (QuestManager.instance != null && 
            QuestManager.instance.IsQuestActive(QuestManager.MainQuest.DivinationCamera) &&
            QuestManager.instance.DivinationLensIndex == lensIndex)
        {
            Debug.Log("Quest camera sala divinazione COMPLETATA: lente corretta selezionata");
            QuestManager.instance.CompleteCurrentQuest();
        }
    }

    public void ResetTutorialFlag()
    {
        tutorialQuestCompleted = false;
    }
}