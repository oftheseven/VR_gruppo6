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

    [Header("Continue text reference")]
    [SerializeField] private RawImage continueText;
    [SerializeField] private float showContinueDelay = 2f;

    [Header("Blinking")]
    [SerializeField] private float blinkSpeed = 2f;
    private float blinkTimer = 0f;

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
        if (continueText != null) continueText.gameObject.SetActive(false);
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

        if (!canAcceptInput) return;
        if (Keyboard.current == null) return;
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            Application.Quit();
        }
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
        if (continueText != null) continueText.gameObject.SetActive(false);

        canAcceptInput = false;
        yield return new WaitForSeconds(showContinueDelay);

        if (continueText != null) continueText.gameObject.SetActive(true);
        canAcceptInput = true;
    }
}