using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{
    Tween tween;

    void OnEnable() {
        transform.localScale = 1.8f * Vector3.one;

        tween = transform.DOScale(2.2f, 1)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    void OnDisable() {
        tween.Kill();
        tween = null;
    }
}
