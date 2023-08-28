using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UIElements;

public class DamageText : MonoBehaviour
{
    private float offset = 45.0f;
    private float durationTime = 0.5f;
    private RectTransform rectTransform;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        Init();
    }

    private void Init()
    {
        rectTransform.DOAnchorPosY(rectTransform.anchoredPosition.y + offset, durationTime).SetEase(Ease.OutQuad).OnComplete(() => OnDestroy());
    }

    private void OnDestroy()
    {
        Destroy(gameObject);
    }
}
