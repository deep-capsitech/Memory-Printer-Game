using UnityEngine;
using UnityEngine.EventSystems;

public class BtnClickEffect : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    private Vector3 originalScale;
    private Vector3 targetScale;

    public float pressedScale = 0.9f;
    public float speed = 12f; // smoothness

    void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    void Update()
    {
        // Smooth scaling
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.deltaTime * speed
        );
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        targetScale = originalScale * pressedScale;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        targetScale = originalScale * 1.05f;
        Invoke(nameof(ResetScale), 0.05f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ResetScale();
    }

    private void ResetScale()
    {
        targetScale = originalScale;
    }
}