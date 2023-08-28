using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour, IHpProvider
{
    public event Action<double, double> OnHealthChanged;
    public Canvas canvas;
    private void Start()
    {
        Init();
    }

    void Init()
    {
        var hpBar = CommonFunction.GetPrefab("Slider_HealthBar_Hero", canvas.transform);   //체력바 세팅
        hpBar.GetComponent<HpSlider>().SetTarget(this.gameObject);
        //Vector3 hpBarScale = new Vector3(55f, 55f, 1f);
        //hpBar.transform.localScale = hpBarScale;
        //hpBar.transform.localScale = Vector3.one;
    }
}
