using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class DamageText : MonoBehaviour
{
    Vector3 offset = new Vector3(0, 0.3f, 0);
    Vector3 scale = new Vector3(40, 40, 1);
    private void Start()
    {
        transform.localScale = scale;

        transform.DOMove(transform.position + offset, 0.3f).OnComplete(() => Invoke(nameof(OnDestroy), 0.5f));
    }

    private void OnDestroy()
    {
        Destroy(gameObject);
    }
}
