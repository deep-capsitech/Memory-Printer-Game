using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

    public class UIHoverScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Range(1f, 1.5f)]
        public float hoverScale = 1.15f;
        public float duration = 0.22f;
        public Ease ease = Ease.OutBack;

        Vector3 normalScale;
        Tween tween;

        void Awake()
        {
            normalScale = transform.localScale;
        }

        public void OnPointerEnter(PointerEventData e)
        {
            Hover();
        }

        public void OnPointerExit(PointerEventData e)
        {
            ResetScale();
        }


        // Mobile press
        public void OnPointerDown(PointerEventData e)
        {
            Hover();
        }


        // Mobile release
        public void OnPointerUp(PointerEventData e)
        {
            ResetScale();
        }

        void Hover()
        {
            tween?.Kill();
            tween = transform.DOScale(normalScale * hoverScale, duration).SetEase(ease);
        }

        void ResetScale()
        {
            tween?.Kill();
            tween = transform.DOScale(normalScale, duration).SetEase(Ease.OutQuad);
        }
    }

