using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;

public class UI_EndPanel : MonoBehaviour
{
    // singleton
    private static UI_EndPanel _instance;
    public static UI_EndPanel instance => _instance;

    [SerializeField] private GameObject pressToContinueGroup;
    [SerializeField] private RawImage finalImage;
    [SerializeField] private TextMeshProUGUI pressKeyText;
    [SerializeField] private float waitTime = 3f;

    private bool canAcceptInput = false;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
        gameObject.SetActive(false);
        if (pressToContinueGroup != null) pressToContinueGroup.SetActive(false);
        if (finalImage != null) finalImage.enabled = false;
    }

    public void ShowEndPanel()
    {
        gameObject.SetActive(true);
        PlayerController.EnableMovement(false);
        PlayerController.SetBasePanelActive(false);
        StartCoroutine(FinaleCoroutine());
    }

    private IEnumerator FinaleCoroutine()
    {
        if (finalImage != null) finalImage.enabled = true;
        if (pressToContinueGroup != null) pressToContinueGroup.SetActive(false);

        canAcceptInput = false;
        yield return new WaitForSeconds(waitTime);

        if (pressToContinueGroup != null) pressToContinueGroup.SetActive(true);
        canAcceptInput = true;
    }

    void Update()
    {
        if (!canAcceptInput) return;
        if (Keyboard.current == null) return;
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            Application.Quit();
        }
    }
}