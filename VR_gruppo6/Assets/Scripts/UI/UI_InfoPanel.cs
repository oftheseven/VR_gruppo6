using UnityEngine;
using UnityEngine.UI;

public class UI_InfoPanel : MonoBehaviour
{
    [SerializeField] private Sprite tutorialImage;

    [Header("UI elements")]
    [SerializeField] private Button infoButton; // bottone per riaprire le info
    [SerializeField] private Button closeInfoButton; // bottone per chiudere le info

    private bool isOpen = false;
    public bool IsOpen => isOpen;
    private bool isFirstTime = true;
    public bool IsFirstTime => isFirstTime;

    void Awake()
    {
        this.gameObject.SetActive(false);

        if (infoButton != null)
        {
            infoButton.gameObject.SetActive(false);
        }

        this.GetComponent<Image>().sprite = tutorialImage;
    }

    public void OpenInfoPanel()
    {
        // controllo che il pannello di info sia associato e sia nella scena del tutorial
        if (this.gameObject != null && SceneZone.IsObjectInScene("TutorialScene"))
        {
            this.gameObject.SetActive(true);
            isOpen = true;
        }
        else if (this.gameObject == null)
        {
            Debug.LogWarning("Info panel GameObject is null.");
        }

        // nascondo il bottone info quando il pannello è aperto
        if (infoButton != null)
        {
            infoButton.gameObject.SetActive(false);
        }

        PlayerController.ShowCursor();
    }

    public void CloseInfoPanel()
    {
        if (this.gameObject != null)
        {
            this.gameObject.SetActive(false);
            isOpen = false;
        }

        // mostro il bottone info solo se non è la prima volta
        if (infoButton != null && !isFirstTime)
        {
            infoButton.gameObject.SetActive(true);
        }

        PlayerController.HideCursor();
    }

    public void OnDeviceOpened()
    {
        if (isFirstTime)
        {
            // prima volta: mostro automaticamente il tutorial
            OpenInfoPanel();
            isFirstTime = false;
        }
        else
        {
            // volte successive: mostro solo il bottone info
            if (infoButton != null)
            {
                infoButton.gameObject.SetActive(true);
            }

            if (this.gameObject != null && this.gameObject.activeSelf)
            {
                this.gameObject.SetActive(false);
                isOpen = false;
            }
        }
    }

    public void OnDeviceClosed()
    {
        if (isOpen)
        {
            CloseInfoPanel();
        }

        if (infoButton != null)
        {
            infoButton.gameObject.SetActive(false);
        }
    }
}
