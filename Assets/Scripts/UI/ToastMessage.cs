using TMPro;
using UnityEngine;
using System.Collections;

public class ToastMessage : MonoBehaviour
{
    public static ToastMessage Instance;

    public TextMeshProUGUI text;
    public CanvasGroup canvasGroup;
    bool isShowing = false;
    void Awake()
    {
        Instance = this;
        //gameObject.SetActive(false);
    }

    void Start()
    {
        canvasGroup.alpha = 0;
    }
    public void Show(string message, float duration = 2f)
    {
        if (isShowing) return;
        StopAllCoroutines();
        text.text = message;
        //gameObject.SetActive(true);
        StartCoroutine(ShowRoutine(duration));
    }

    IEnumerator ShowRoutine(float duration)
    {
        isShowing = true;
        // Fade In
        canvasGroup.alpha = 0;
        float t = 0;
        while (t < 0.2f)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = t / 0.2f;
            yield return null;
        }

        canvasGroup.alpha = 1;

        yield return new WaitForSeconds(duration);

        // Fade Out
        t = 0;
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = 1 - (t / 0.3f);
            yield return null;
        }

        canvasGroup.alpha = 0;
        isShowing = false;
    }
}