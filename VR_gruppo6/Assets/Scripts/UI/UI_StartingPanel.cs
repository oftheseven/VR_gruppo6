using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.UI;

public class GameIntroPanels : MonoBehaviour
{
    [Header("Continue text reference")]
    [SerializeField] private RawImage continueText;
    [SerializeField] private float showContinueDelay = 2f;

    [Header("Blinking")]
    [SerializeField] private float blinkSpeed = 2f;
    private float blinkTimer = 0f;

    public static bool ShownIntroPanels = false;

    void Start()
    {
        if (ShownIntroPanels) return;
        ShowPanel();
        PlayerController.EnableMovement(false);
        PlayerController.ForceCursorVisible(true);

        if (continueText != null)
            continueText.gameObject.SetActive(false);

        StartCoroutine(IntroPanelFlow());
    }

    void Update()
    {
        if (this.gameObject.activeSelf && continueText != null)
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

        ShownIntroPanels = true;
        if (continueText != null)
            continueText.gameObject.SetActive(false);
        this.gameObject.SetActive(false);

        PlayerController.EnableMovement(true);
        PlayerController.ForceCursorVisible(false);

        if (PlayerController.instance != null)
            PlayerController.instance.StartTutorialDialogueIfNeeded();
    }
}