// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;
// using System.Collections.Generic;

// public class UI_GreenScreenPickerPanel : MonoBehaviour
// {
//     [Header("UI References")]
//     [SerializeField] private Transform buttonContainer;
//     [SerializeField] private GameObject buttonPrefab;
//     [SerializeField] private TextMeshProUGUI titleText;
    
//     private InteractableComputer currentComputer;
//     private List<Button> spawnedButtons = new List<Button>();
    
//     private bool isOpen = false;
//     public bool IsOpen => isOpen;

//     void Start()
//     {
//         this.gameObject.SetActive(false);
//     }

//     public void OpenPicker(InteractableComputer computer, GreenScreenTarget[] greenScreens)
//     {
//         if (computer == null || greenScreens == null || greenScreens.Length == 0)
//         {
//             Debug.LogError("Dati invalidi per GreenScreenPicker!");
//             return;
//         }

//         currentComputer = computer;
        
//         ClearButtons();
        
//         foreach (GreenScreenTarget gs in greenScreens)
//         {
//             if (!gs.IsValid()) continue;
            
//             CreateButton(gs);
//         }
        
//         this.gameObject.SetActive(true);
//         isOpen = true;
        
//         if (titleText != null)
//         {
//             titleText.text = "Seleziona Green Screen";
//         }
        
//         PlayerController.EnableMovement(false);
//         PlayerController.instance.BasePanel.gameObject.SetActive(false);
//         PlayerController.ShowCursor();
        
//         Debug.Log($"GreenScreenPicker aperto con {greenScreens.Length} green screen");
//     }
//     private void CreateButton(GreenScreenTarget greenScreen)
//     {

//         if (buttonPrefab == null || buttonContainer == null)
//         {
//             Debug.LogError("ButtonPrefab o Container mancante!");
//             return;
//         }

//         GameObject buttonObj = Instantiate(buttonPrefab, buttonContainer);
//         Button button = buttonObj.GetComponent<Button>();
//         TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

//         if (button != null)
//         {
//             if (buttonText != null)
//             {
//                 string displayText = greenScreen.displayName;
                
//                 if (greenScreen.isCompleted)
//                 {
//                     displayText += " ✓";
//                 }
                
//                 buttonText.text = displayText;
//             }

//             button.onClick.AddListener(() => OnGreenScreenSelected(greenScreen));
            
//             spawnedButtons.Add(button);
//         }
//     }

//     private void OnGreenScreenSelected(GreenScreenTarget greenScreen)
//     {
//         Debug.Log($"Green Screen selezionato: {greenScreen.displayName}");
        
//         if (AudioManager.instance != null)
//         {
//             AudioManager.instance.PlayUIClick();
//         }
        
//         ClosePicker();
        
//         if (currentComputer != null)
//         {
//             currentComputer.OpenComputerPanel(greenScreen);
//         }
//     }

//     public void ClosePicker()
//     {
//         isOpen = false;
//         this.gameObject.SetActive(false);
        
//         ClearButtons();
        
//         PlayerController.EnableMovement(true);
//         PlayerController.HideCursor();
//     }

//     private void ClearButtons()
//     {
//         foreach (Button btn in spawnedButtons)
//         {
//             if (btn != null)
//             {
//                 Destroy(btn.gameObject);
//             }
//         }
//         spawnedButtons.Clear();
//     }

//     public void OnCloseButtonClicked()
//     {
//         ClosePicker();
//     }
// }