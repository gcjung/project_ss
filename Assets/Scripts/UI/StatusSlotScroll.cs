using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using static GameDataManager;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Misc;

public class StatusSlotScroll : MonoBehaviour
{
    [Header("Create Slots")]
    [SerializeField] private GameObject statusSlot;
    [SerializeField] private Transform content;

    private List<StatusSlot> slots = new List<StatusSlot>();
    private List<GameObject> slotObjects = new List<GameObject>();

    public class StatusSlot
    {
        public int statusLevel;
        public string statusName;
        public double statusValue;
        public double cost;
        public UnityAction buttonClick;
        public StatusSlot(int statusLevel, string statusName, double statusValue, double cost, UnityAction buttonClick)
        {// statusLevel만 데이터테이블에 저장된 값으로 받아와서 계산해주면 됨
            this.statusLevel = statusLevel;
            this.statusName = statusName;
            this.statusValue = statusValue;
            this.cost = cost;
            this.buttonClick += buttonClick;
        }
    }
    private IEnumerator Start()
    {
        yield return CommonIEnumerator.IEWaitUntil(
           predicate: () => { return GlobalManager.Instance.Initialized; },
           onFinish: () =>
           {
               Init();
               CreateSlots();
           });
    }

    void Init()
    {
        foreach (var data in StatusTemplate.OrderBy(x => x.Value[(int)StatusTemplate_.Order])) 
        {
            string statLevelStr = data.Key + "Level";

            double val_calc = double.Parse(data.Value[(int)StatusTemplate_.Value_Calc]);
            double cost_calc = double.Parse(data.Value[(int)StatusTemplate_.Cost_Calc]);

            int index = int.Parse(data.Value[(int)StatusTemplate_.Order]) - 1;
            int statLevel = (int)GlobalManager.Instance.DBManager.GetUserDoubleData(statLevelStr, 1);
            string typeName = data.Value[(int)StatusTemplate_.TypeName];
            double statusValue = statLevel * val_calc;
            double costValue = statLevel * cost_calc;

            StatusSlot slot = new StatusSlot(statLevel, typeName, statusValue, costValue,
                () => UpgradeStatus(statLevelStr, val_calc, cost_calc, index));
            slots.Add(slot);
        }
    }

    void UpgradeStatus(string stat, double val_calc, double cost_calc, int index)
    {
        int level = (int)GlobalManager.Instance.DBManager.GetUserDoubleData(stat, 1);
        var slot = slotObjects[index];

        var levelText = slot.transform.Find("StatusLevel_Text").gameObject;
        if (levelText != null)
        {
            level += 1;
            levelText.GetComponent<TextMeshProUGUI>().text = $"LV {level}";
        }

        var valueText = slot.transform.Find("StatusValue_Text").gameObject;
        if (valueText != null)
        {
            double value = level * val_calc;

            valueText.GetComponent<TextMeshProUGUI>().text = Util.BigNumCalculate(value);
        }

        var button = slot.transform.Find("LevelUp_Button").gameObject;
        if (button != null)
        {
            double cost = level * cost_calc;

            var costText = button.transform.Find("Cost_Text").gameObject;
            if (costText != null)
            {
                costText.GetComponent<TextMeshProUGUI>().text = $"{Util.BigNumCalculate(cost)}  G";
            }
        }

        GlobalManager.Instance.DBManager.UpdateUserData(stat, level);
    }

    private void CreateSlots()
    {
        if (slots.Count > 0)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                var slot = Instantiate(statusSlot, content);
                
                var levelText = slot.transform.Find("StatusLevel_Text").gameObject;
                if (levelText != null)
                {
                    levelText.GetComponent<TextMeshProUGUI>().text = $"LV {slots[i].statusLevel}";
                }
                    
                var nameText = slot.transform.Find("StatusName_Text").gameObject;
                if (nameText != null)
                {
                    nameText.GetComponent<TextMeshProUGUI>().text = slots[i].statusName;
                }
                    
                var valueText = slot.transform.Find("StatusValue_Text").gameObject;
                if (valueText != null)
                {
                    valueText.GetComponent<TextMeshProUGUI>().text = Util.BigNumCalculate(slots[i].statusValue);
                }
                
                var button = slot.transform.Find("LevelUp_Button").gameObject;
                if (button != null)
                {
                    var costText = button.transform.Find("Cost_Text").gameObject;
                    if(costText!=null)
                    {
                        costText.GetComponent<TextMeshProUGUI>().text = $"{Util.BigNumCalculate(slots[i].cost)}  G";
                    }

                    var buttonClick = button.GetComponent<Button>();
                    if (buttonClick != null)
                    {
                        buttonClick.onClick.RemoveAllListeners();
                        buttonClick.onClick.AddListener(slots[i].buttonClick);
                    }                   
                }

                slotObjects.Add(slot);
            }
        }
        else
        {
            Debug.LogError("배열이 비어있음");
        }
    }
}
