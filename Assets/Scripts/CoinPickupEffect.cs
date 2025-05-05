using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class CoinPickupEffect : MonoBehaviour
{
    private bool pickedUp = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!pickedUp || collision.collider.CompareTag("Ground") || collision.collider.CompareTag("Coin"))
        {
            pickedUp = true;
            StartCoroutine(AnimateToUI());
        }
    }

    private IEnumerator AnimateToUI()
    {
        yield return new WaitForSeconds(0.5f);
        Vector3 screenTarget = CoinUIManager.Instance.GetWorldTarget();
        transform.DOMove(screenTarget, 0.5f).SetEase(Ease.InOutSine).OnComplete(() =>
        {
            ObjectPool.Instance?.ReturnToPool("Coin", gameObject);
        });
    }
}