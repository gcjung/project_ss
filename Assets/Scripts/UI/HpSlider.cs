using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class HpSlider : MonoBehaviour
{
    private IHpProvider hpProvider;
    private Transform targetTransform;

    private Vector3 targetScreenPosition;
    private Vector3 offset = new Vector3(0, -0.3f, 0);
    private Slider slider;
    private TextMeshProUGUI hpText;
    private RectTransform hpRectTransform;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        hpRectTransform = GetComponent<RectTransform>();
        slider.value = 1.0f;

        if (transform.Find("Hp_Text").TryGetComponent<TextMeshProUGUI>(out var _hpText))
        {
            hpText = _hpText;
        }
    }
    private void Update()
    {
        if (targetTransform != null)
        {
            SetRectTransform();
        }
    }

    public void SetTarget(GameObject _target)
    {
        targetTransform = _target.transform;      

        if (_target.TryGetComponent<Player>(out var _player))
        {
            hpText.text = Util.BigNumCalculate(_player.CurrentHp);
        }
        else if (_target.TryGetComponent<Monster>(out var _monster))
        {
            hpText.text = Util.BigNumCalculate(_monster.CurrentHp);
        }

        if (_target.TryGetComponent<IHpProvider>(out var _hpProvider))
        {
            this.hpProvider = _hpProvider;
            this.hpProvider.OnHealthChanged += UpdateHealth;
        }

        SetRectTransform();
    }

    private void SetRectTransform()
    {
        targetScreenPosition = targetTransform.position + offset;

        hpRectTransform.position = Camera.main.WorldToScreenPoint(targetScreenPosition);
    }

    private void UpdateHealth(double curHp, double maxHp)
    {
        slider.value = (float)(curHp / maxHp);
        hpText.text = Util.BigNumCalculate(curHp);

        if (curHp <= 0)
        {
            this.hpProvider.OnHealthChanged -= UpdateHealth;

            Destroy(this.gameObject, 1.0f);
        }
    }
}
