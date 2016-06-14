using UnityEngine;
using System.Collections;
using DG.Tweening;
using System;

public class Diamond : MonoBehaviour
{
    Vector3 startScale;

    void Start()
    {
        startScale = transform.localScale;
        transform.localScale = Vector3.zero;
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        Disappear(null);
    }

    public void Disappear(Action OnCompleted = null)
    {
        var anim = transform.DOScale(0f, 0.3f).SetEase(Ease.InBack);

        if (OnCompleted != null)
            anim.OnComplete(() => OnCompleted());
    }

    public void AppearAtPosition(Vector2 position)
    {
        Disappear(() =>
        {
            transform.DOScale(startScale, 0.3f).SetEase(Ease.OutBack);
            transform.position = position;
        });
    }
}
