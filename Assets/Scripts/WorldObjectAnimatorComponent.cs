using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public enum WorldAnimationType
{
    ScaleTo,
    MoveBy,
    RotateTo
}

[System.Serializable]
public class WorldAnimationStep
{
    public WorldAnimationType type;
    public float duration = 0.5f;
    public float delay = 0f;
    public Ease ease = Ease.OutSine;
    public Vector3 targetVector = Vector3.zero;
    public bool loop = false;
    public bool pingPong = false;
}

public class WorldObjectAnimatorComponent : MonoBehaviour
{
    public List<WorldAnimationStep> animationSteps = new();
    public bool playOnStart = true;
    public bool resetTransformOnReplay = true;

    private Vector3 originalPosition;
    private Vector3 originalScale;
    private Vector3 originalRotation;
    private List<Tween> activeTweens = new();

    void Awake()
    {
        originalPosition = transform.position;
        originalScale = transform.localScale;
        originalRotation = transform.eulerAngles;
    }

    void OnEnable()
    {
        if (playOnStart)
            Play();
    }

    public void Play(System.Action onComplete = null)
    {
        KillAllTweens();

        if (resetTransformOnReplay)
        {
            transform.position = originalPosition;
            transform.localScale = originalScale;
            transform.eulerAngles = originalRotation;
        }

        foreach (var step in animationSteps)
        {
            Tween tween = null;

            switch (step.type)
            {
                case WorldAnimationType.ScaleTo:
                    tween = transform.DOScale(step.targetVector, step.duration);
                    break;

                case WorldAnimationType.MoveBy:
                    tween = transform.DOMove(transform.position + step.targetVector, step.duration);
                    break;

                case WorldAnimationType.RotateTo:
                    tween = transform.DORotate(step.targetVector, step.duration, RotateMode.FastBeyond360);
                    break;
            }

            if (tween != null)
            {
                tween.SetEase(step.ease).SetDelay(step.delay);

                if (step.loop)
                    tween.SetLoops(-1, step.pingPong ? LoopType.Yoyo : LoopType.Restart);

                tween.Play();
                activeTweens.Add(tween);
            }
        }

        onComplete?.Invoke();
    }

    private void KillAllTweens()
    {
        foreach (var t in activeTweens)
        {
            if (t.IsActive())
                t.Kill();
        }

        activeTweens.Clear();
    }
}
