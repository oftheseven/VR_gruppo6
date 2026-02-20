using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.UI;
using Unity.VisualScripting;

public class UI_StartingPanel : MonoBehaviour
{
    [Header("Continue text reference")]
    [SerializeField] private RawImage continueText;
    [SerializeField] private RawImage panelImage;
    [SerializeField] private Texture2D secondPanelImage;
    [SerializeField] private float showContinueDelay = 2f;

    [Header("Blinking")]
    [SerializeField] private float blinkSpeed = 2f;
    private float blinkTimer = 0f;

    public static bool ShownIntroPanels = false;
    public static bool IsBlockingInput = false;

    void Start()
    {
        if (ShownIntroPanels) return;
        ShowPanel();
        PlayerController.EnableMovement(false);

        if (continueText != null)
            continueText.gameObject.SetActive(false);

        StartCoroutine(IntroPanelFlow());
    }

    void Update()
    {
        if (this.gameObject.activeSelf && continueText != null && continueText.gameObject.activeSelf)
        {
            blinkTimer += Time.deltaTime * blinkSpeed;
            float alpha = Mathf.Lerp(0.2f, 1f, (Mathf.Sin(blinkTimer * Mathf.PI) + 1f) / 2f);
            var c = continueText.color;
            c.a = alpha;
            continueText.color = c;
        }
        else if (continueText != null)
        {
            var c = continueText.color;
            c.a = 1f;
            continueText.color = c;
        }
    }

    public void ShowPanel()
    {
        this.gameObject.SetActive(true);
        IsBlockingInput = true;
        PlayerController.SetBasePanelActive(false);
        PlayerController.EnableMovement(false);
    }

    private IEnumerator IntroPanelFlow()
    {
        yield return new WaitForSeconds(showContinueDelay);

        if (continueText != null)
            continueText.gameObject.SetActive(true);

        while (true)
        {
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                break;
            yield return null;
        }

        if (continueText != null)
            continueText.gameObject.SetActive(false);

        if (panelImage != null && secondPanelImage != null)
            panelImage.texture = secondPanelImage;

        yield return new WaitForSeconds(showContinueDelay);
        if (continueText != null)
            continueText.gameObject.SetActive(true);

        while (true)
        {
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                break;
            yield return null;
        }

        ShownIntroPanels = true;
        if (continueText != null)
            continueText.gameObject.SetActive(false);

        if (PlayerController.instance != null)
            StartCoroutine(TriggerDialogueAfterInputReleased());

        this.gameObject.SetActive(false);

        PlayerController.EnableMovement(true);
        PlayerController.SetBasePanelActive(true);
        IsBlockingInput = false;
    }

    private IEnumerator TriggerDialogueAfterInputReleased()
    {
        // attendo che E venga rilasciato per evitare che venga catturato dal dialogue panel appena si apre
        while (Keyboard.current != null && Keyboard.current.eKey.isPressed)
            yield return null;

        yield return null;

        PlayerController.instance.StartTutorialDialogueIfNeeded();
    }
}