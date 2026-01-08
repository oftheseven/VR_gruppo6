using UnityEngine;
using UnityEngine.UI;

public class UI_InfoPanel : MonoBehaviour
{
    [SerializeField] private Sprite tutorialImage;

    [Header("UI Elements")]
    [SerializeField] private Button infoButton; // bottone per riaprire le info
    [SerializeField] private Button closeInfoButton; // bottone per chiudere le info

    private bool isOpen = false;
    public bool IsOpen => isOpen;
    private bool isFirstTime = true;
    public bool IsFirstTime => isFirstTime;

    void Start()
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
        if (this.gameObject != null)
        {
            this.gameObject.SetActive(true);
            isOpen = true;
        }

        // nascondo il bottone info quando il pannello è aperto
        if (infoButton != null)
        {
            infoButton.gameObject.SetActive(false);
        }
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
