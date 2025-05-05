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

    void Start()
    {
        if (playOnStart)
            Play();
    }

    public void Play(System.Action onComplete = null)
    {
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
                {
                    tween.SetLoops(-1, step.pingPong ? LoopType.Yoyo : LoopType.Restart);
                    tween.Play();
                }
                else
                {
                    tween.Play();
                }
            }
        }

        onComplete?.Invoke();
    }
}