using UnityEngine;
using System.Collections;
using DG.Tweening;

public class Blink : MonoBehaviour
{
    public float BlinkDuration;
	void Start ()
    {
        var sr = GetComponent<SpriteRenderer>();
        sr.color = new Color(1f, 1f, 1f, 0f);

        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(Random.Range(1f, 5f));
        sequence.Append(sr.DOFade(1f, BlinkDuration));
        sequence.AppendInterval(Random.Range(1f, 2f));
        sequence.Append(sr.DOFade(0f, BlinkDuration));
        sequence.SetLoops(-1);

        //sr.DOFade(0f, BlinkDuration).SetEase(Ease.Flash).SetLoops(-1);
	}
}
