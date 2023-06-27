using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Firebase.Firestore;

public class StatusSlotScroll : MonoBehaviour
{
    [Header("Create Slots")]
    [SerializeField] private GameObject statusSlot;
    [SerializeField] private Transform content;

    private List<StatusSlot> slots = new List<StatusSlot>();

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
    private void Start()
    {
        StatusSlot slot1 = new StatusSlot(1, "공격력", 523, 1, () => Debug.Log("슬릇1"));
        StatusSlot slot2 = new StatusSlot(1, "체력", 5233, 1, () => Debug.Log("슬릇2"));
        StatusSlot slot3 = new StatusSlot(1, "공격속도", 52333, 1, () => Debug.Log("슬릇3"));
        StatusSlot slot4 = new StatusSlot(1, "치명타 확률", 5233333, 1, () => Debug.Log("슬릇4"));

        slots.Add(slot1);
        slots.Add(slot2);
        slots.Add(slot3);
        slots.Add(slot4);

        CreateSlots();
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
                    valueText.GetComponent<TextMeshProUGUI>().text = Utill.BigNumCalculate(slots[i].statusValue);
                }
                
                var button = slot.transform.Find("LevelUp_Button").gameObject;
                if (button != null)
                {
                    button.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = $"{Utill.BigNumCalculate(slots[i].cost)}  G";

                    var buttonClick = button.GetComponent<Button>();
                    if (buttonClick != null)
                    {
                        buttonClick.onClick.RemoveAllListeners();
                        buttonClick.onClick.AddListener(slots[i].buttonClick);
                    }                   
                }
            }
        }
        else
        {
            Debug.LogError("배열이 비어있음");
        }
    }
}
