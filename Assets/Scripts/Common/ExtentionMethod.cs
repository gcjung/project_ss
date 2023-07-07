using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System;
using UnityEngine.UI;
using DG.Tweening.Core.Easing;

public static class ExtentionMethod
{
    public static void SetParent(this GameObject gameObject, Transform transform)
    {
        if(transform == null)
        {
            Debug.LogError($" {nameof(SetParent)} null");
            return;
        }
        gameObject.transform.parent = transform;
        gameObject.transform.localPosition = Vector3.zero;
        gameObject.transform.localScale = Vector3.one;
        gameObject.transform.localRotation = Quaternion.identity;
    }
    
}
