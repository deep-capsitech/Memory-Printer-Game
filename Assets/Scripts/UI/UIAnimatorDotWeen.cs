using DG.Tweening;
//using Unity.VisualScripting;
using UnityEngine;

public class UIAnimatorDOTween : MonoBehaviour
    {
        [Header("Main")]
        public bool playOnEnable = true;
        public float delay = 0f;
        public float duration = 0.5f;
        public Ease ease = Ease.OutBack;
        public bool ignoreTimeScale = true;

        RectTransform rect;
        CanvasGroup canvasGroup;

        [Header("Fade")]
        public bool fadeIn;
        public bool fadeOut;

        [Header("Scale")]
        public bool scaleIn;
        public bool scaleOut;
        public Vector3 startScale = Vector3.zero;
        public Vector3 endScale = Vector3.one;

        [Header("Slide")]
        public bool slideFromLeft;
        public bool slideFromRight;
        public bool slideFromTop;
        public bool slideFromBottom;
        public float slideDistance = 800f;

        [Header("Rotation")]
        public bool rotateIn;
        public Vector3 rotationAmount = new Vector3(0, 0, 180);

        [Header("Punch")]
        public bool punchScale;
        public Vector3 punch = new Vector3(0.2f, 0.2f, 0);
        public int vibrato = 10;

        [Header("Loop")]
        public bool loop;
        public LoopType loopType = LoopType.Yoyo;
        public int loopCount = -1;

        Vector2 originalPos;

        void Awake()
        {
            rect = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();

            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();

            originalPos = rect.anchoredPosition;
        }

        void OnEnable()
        {
            if (playOnEnable)
                PlayAnimation();
        }

        public void PlayAnimation()
        {
            rect.DOKill();
            canvasGroup.DOKill();

            Sequence seq = DOTween.Sequence();
            seq.SetUpdate(ignoreTimeScale);

            // Fade
            if (fadeIn)
            {
                canvasGroup.alpha = 0;
                seq.Append(canvasGroup.DOFade(1, duration));
            }
            if (fadeOut)
            {
                canvasGroup.alpha = 1;
                seq.Append(canvasGroup.DOFade(0, duration));
            }

            // Scale
            if (scaleIn)
            {
                rect.localScale = startScale;
                seq.Join(rect.DOScale(endScale, duration).SetEase(ease));
            }
            if (scaleOut)
            {
                rect.localScale = endScale;
                seq.Join(rect.DOScale(startScale, duration).SetEase(ease));
            }

            // Slide
            if (slideFromLeft)
            {
                rect.anchoredPosition = originalPos + Vector2.left * slideDistance;
                seq.Join(rect.DOAnchorPos(originalPos, duration).SetEase(ease));
            }
            if (slideFromRight)
            {
                rect.anchoredPosition = originalPos + Vector2.right * slideDistance;
                seq.Join(rect.DOAnchorPos(originalPos, duration).SetEase(ease));
            }
            if (slideFromTop)
            {
                rect.anchoredPosition = originalPos + Vector2.up * slideDistance;
                seq.Join(rect.DOAnchorPos(originalPos, duration).SetEase(ease));
            }
            if (slideFromBottom)
            {
                rect.anchoredPosition = originalPos + Vector2.down * slideDistance;
                seq.Join(rect.DOAnchorPos(originalPos, duration).SetEase(ease));
            }

            // Rotation
            if (rotateIn)
            {
                rect.localRotation = Quaternion.Euler(rotationAmount);
                seq.Join(rect.DORotate(Vector3.zero, duration));
            }

            // Punch
            if (punchScale)
            {
                seq.Append(rect.DOPunchScale(punch, duration, vibrato));
            }

            // Delay
            seq.SetDelay(delay);

            // Loop
            if (loop)
            {
                seq.SetLoops(loopCount, loopType);
            }

            seq.Play();
        }
    }

