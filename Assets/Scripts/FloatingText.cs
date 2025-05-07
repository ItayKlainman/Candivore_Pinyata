using TMPro;
using UnityEngine;
using DG.Tweening;

public class FloatingText : MonoBehaviour
{
    public TextMeshPro text;
    public float floatDistance = 1f;
    public float duration = 0.75f;

    public void Show(string content, Color color, Vector3 position)
    {
        text.text = content;
        text.color = color;
        transform.position = position;
        
        text.sortingLayerID = SortingLayer.NameToID("UI");
        text.sortingOrder = 100;


        Sequence seq = DOTween.Sequence();
        seq.Join(transform.DOMoveY(position.y + floatDistance, duration).SetEase(Ease.OutCubic));
        seq.Join(text.DOFade(0f, duration));
        seq.OnComplete(() =>
        {
            text.alpha = 1f;
            ObjectPool.Instance?.ReturnToPool("FloatingText", gameObject);
        });
    }

    private void OnDisable()
    {
        text.alpha = 1f;
    }
}