using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class StatusSlot : MonoBehaviour
{
    private TextMeshProUGUI levelText;
    private TextMeshProUGUI nameText;
    private TextMeshProUGUI valueText;
    private Button button;
    private TextMeshProUGUI costText;

    private string statusName;
    private int statusLevel;
    private string statusNameKr;
    private double statusValue;
    private double costValue;
    private double val_calc;
    private double cost_calc;
    private void Start()
    {
        if (transform.Find("StatusLevel_Text").TryGetComponent<TextMeshProUGUI>(out var _levelText))
        {
            levelText = _levelText;
        }

        if (transform.Find("StatusName_Text").TryGetComponent<TextMeshProUGUI>(out var _nameText))
        {
            nameText = _nameText;
        }

        if (transform.Find("StatusValue_Text").TryGetComponent<TextMeshProUGUI>(out var _valueText))
        {
            valueText = _valueText;
        }

        if (transform.Find("LevelUp_Button").TryGetComponent<Button>(out var _button))
        {
            button = _button;
            button.onClick.AddListener(UpgradeStatus);
        }

        if (transform.Find("LevelUp_Button/Cost_Text").TryGetComponent<TextMeshProUGUI>(out var _costText))
        {
            costText = _costText;
        }

        Init();
    }

    private void Init()
    {
        levelText.text = $"LV {this.statusLevel}";
        nameText.text = this.statusNameKr;
        valueText.text = Util.BigNumCalculate(this.statusValue);
        costText.text = $"{Util.BigNumCalculate(this.costValue)} G";
    }

    private void UpgradeStatus()
    {
        if (MainScene.Instance.UseGolds(costValue))
        {
            statusLevel += 1;
            levelText.text = $"LV {statusLevel}";

            statusValue = statusLevel * val_calc;
            valueText.text = Util.BigNumCalculate(statusValue);

            costValue = statusLevel * cost_calc;
            costText.text = $"{Util.BigNumCalculate(costValue)} G";

            GlobalManager.Instance.DBManager.UpdateUserData(statusName, statusLevel);
            MainScene.Instance.UpdateStatusLevel(statusName, statusLevel);
        }        
    }

    public void SetSlot(string statusName, int statusLevel, string statusNameKr, double statusValue, double costValue ,double val_calc, double cost_calc)
    {
        this.statusName = statusName;
        this.statusLevel = statusLevel;
        this.statusNameKr = statusNameKr;
        this.statusValue = statusValue;
        this.costValue = costValue;
        this.val_calc = val_calc;
        this.cost_calc = cost_calc;            
    }
}
