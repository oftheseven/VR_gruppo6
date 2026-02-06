using TMPro;
using UnityEngine;
using System.Collections;

public class UI_LoadingIcon : MonoBehaviour
{
    private static UI_LoadingIcon _instance;
    public static UI_LoadingIcon instance => _instance;

    [Header("UI references")]
    [SerializeField] private GameObject loadingIcon;
    [SerializeField] private RectTransform iconTransform;
    [SerializeField] TextMeshProUGUI loadingText;

    [Header("Rotation settings")]
    [SerializeField] private float rotationSpeed = 180f;

    [Header("Fade settings")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 0.3f;

    private bool isRotating = false;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (loadingIcon != null)
        {
            loadingIcon.SetActive(false);
        }

        if (canvasGroup == null && loadingIcon != null)
        {
            canvasGroup = loadingIcon.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = loadingIcon.AddComponent<CanvasGroup>();
            }
        }
    }

    void Update()
    {
        if (isRotating && iconTransform != null)
        {
            iconTransform.Rotate(0f, 0f, -rotationSpeed * Time.deltaTime);
        }
    }

    public void Show(string text = "Loading...")
    {
        if (loadingIcon != null)
        {
            loadingIcon.SetActive(true);    
        }

        if (loadingText != null)
        {
            loadingText.text = text;
        }

        isRotating = true;
        StartCoroutine(FadeIn());
    }

    public void Hide()
    {
        StartCoroutine(HideCoroutine());
    }

    private IEnumerator HideCoroutine()
    {
        yield return StartCoroutine(FadeOut());
        
        isRotating = false;

        if (loadingIcon != null)
        {
            loadingIcon.SetActive(false);
        }
    }

    private IEnumerator FadeIn()
    {
        if (canvasGroup == null) yield break;

        float elapsed = 0f;
        canvasGroup.alpha = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    private IEnumerator FadeOut()
    {
        if (canvasGroup == null) yield break;

        float elapsed = 0f;
        canvasGroup.alpha = 1f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(1f - (elapsed / fadeDuration));
            yield return null;
        }

        canvasGroup.alpha = 0f;
    }
}