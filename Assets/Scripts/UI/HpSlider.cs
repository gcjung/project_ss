using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class HpSlider : MonoBehaviour
{
    private GameObject target;
    private Transform targetTransform;

    public float sliderValue;
    private float SliderValue
    {
        get { return sliderValue; }
        set
        {
            sliderValue = value;
            Debug.Log($"현재 체력 : {sliderValue}");

            if (sliderValue <= 0)
            {
                Destroy(gameObject);
            }            
        }
    }

    private Vector3 offset = new Vector3(0, -0.3f, 0);
    private Slider slider;
    private RectTransform sliderRect;
    private TextMeshProUGUI hpText;

    private void Update()
    {
        if (targetTransform != null)
        {
            Vector3 targetscreenposition = targetTransform.position + offset;   //Screen Space - Camera에선 WorldToScreenPoint 쓸 필요없음

            transform.position = targetscreenposition;
        }
    }

    public void SetTarget(GameObject _target)
    {
        if (_target.TryGetComponent<Player>(out var player))
        {
            slider = GetComponent<Slider>();
            sliderRect = GetComponent<RectTransform>();

            if (transform.Find("Hp_Text").TryGetComponent<TextMeshProUGUI>(out var _hpText))
            {
                hpText = _hpText;
            }

            target = _target;
            targetTransform = _target.transform;

            sliderValue = (float)player.CurrentHp;
            //sliderValue = (float)(player.CurrentHp / player.TotalHp);
        }
    }
}
