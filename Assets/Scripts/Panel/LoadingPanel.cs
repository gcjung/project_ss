using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class LoadingPanel : PanelBase
{
    [Header("Loading Slider")]
    [SerializeField] private Slider loadingSlider;

    public static float SliderValue { get; set; }

    private new void Awake()
    {
        base.Awake();

        SliderValue = loadingSlider.value;
        SliderValue = 0f;
    }

    public override void InitPanel()
    {
        Debug.Log("로딩 패널 생성");
    }

    public override void OpenPanel()
    {

    }

    public override void ClosePanel()
    {

    }
}
