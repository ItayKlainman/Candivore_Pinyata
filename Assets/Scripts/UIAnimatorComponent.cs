using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public enum UIAnimationType
{
    ScalePunch,
    ScaleTo,      
    FadeIn,
    FadeOut,
    MoveIn,
    MoveOut,
    RotateTo     
}


[System.Serializable]
public class UIAnimationStep
{
    public UIAnimationType type;
    public float duration = 0.3f;
    public float delay = 0f;
    public Ease ease = Ease.OutBack;
    public Vector3 targetVector = Vector3.zero;

    public bool loop = false;
    public bool pingPong = false;
}

public class UIAnimatorComponent : MonoBehaviour
{
    public List<UIAnimationStep> animationSteps = new();
    public bool playOnEnable = false;

    private RectTransform rect;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    void OnEnable()
    {
        if (playOnEnable)
            Play();
    }

    public void Play(System.Action onComplete = null)
    {
        Sequence seq = DOTween.Sequence();

        foreach (var step in animationSteps)
        {
            Tween tween = null;

            switch (step.type)
            {
                case UIAnimationType.ScalePunch:
                    tween = rect.DOPunchScale(Vector3.one * 0.2f, step.duration, 10, 1);
                    break;
                
                case UIAnimationType.ScaleTo:
                    tween = rect.DOScale(step.targetVector, step.duration);
                    break;

                case UIAnimationType.FadeIn:
                    canvasGroup.alpha = 0f;
                    tween = canvasGroup.DOFade(1f, step.duration);
                    break;

                case UIAnimationType.FadeOut:
                    canvasGroup.alpha = 1f;
                    tween = canvasGroup.DOFade(0f, step.duration);
                    break;

                case UIAnimationType.MoveIn:
                    rect.anchoredPosition += (Vector2)step.targetVector;
                    tween = rect.DOAnchorPos(rect.anchoredPosition - (Vector2)step.targetVector, step.duration);
                    break;

                case UIAnimationType.MoveOut:
                    tween = rect.DOAnchorPos(rect.anchoredPosition + (Vector2)step.targetVector, step.duration);
                    break;
                
                case UIAnimationType.RotateTo:
                    tween = rect.DORotate(step.targetVector, step.duration, RotateMode.FastBeyond360);
                    break;
            }

            if (tween != null)
            {
                tween.SetEase(step.ease).SetDelay(step.delay);

                if (step.loop)
                {
                    tween.SetLoops(-1, step.pingPong ? LoopType.Yoyo : LoopType.Restart);
                    tween.Play(); // Infinite tweens don't go into sequence
                }
                else
                {
                    seq.Insert(step.delay, tween);
                }
            }
        }

        if (onComplete != null)
            seq.OnComplete(() => onComplete.Invoke());
    }
}
