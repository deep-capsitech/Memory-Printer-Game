using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class UIHoverScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Range(1f, 1.5f)]
    public float hoverScale = 1.15f;
    public float duration = 0.22f;
    public Ease ease = Ease.OutBack;

    Vector3 normalScale;
    Tween tween;

    Button button; // 🔥 reference

    void Awake()
    {
        normalScale = transform.localScale;
        button = GetComponent<Button>(); // 🔥 get button
    }

    public void OnPointerEnter(PointerEventData e)
    {
        Hover();
    }

    public void OnPointerExit(PointerEventData e)
    {
        ResetScale();
    }

    public void OnPointerDown(PointerEventData e)
    {
        // 🔥 ONLY PLAY SOUND IF BUTTON IS INTERACTABLE
        if (button != null && button.interactable)
        {
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlayButtonClick();
        }

        Hover();
    }

    public void OnPointerUp(PointerEventData e)
    {
        ResetScale();
    }

    void Hover()
    {
        tween?.Kill();
        tween = transform.DOScale(normalScale * hoverScale, duration)
            .SetEase(ease)
            .SetUpdate(true);
    }

    void ResetScale()
    {
        tween?.Kill();
        tween = transform.DOScale(normalScale, duration)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true);
    }
}