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
    private Transform content;
    private List<GameObject> slots = new List<GameObject>();

    private IEnumerator Start()
    {
        content = transform.Find("Viewport/Content");

        yield return CommonIEnumerator.IEWaitUntil(
           predicate: () => { return GlobalManager.Instance.Initialized; },
           onFinish: () =>
           {
               Init();
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

            var slot = CommonFunction.GetPrefabInstance("StatusSlot", content);
            slot.AddComponent<StatusSlot>().SetSlot(statLevelStr, statLevel, typeName, statusValue, costValue, val_calc, cost_calc);

            slots.Add(slot);
        }
    }
}
