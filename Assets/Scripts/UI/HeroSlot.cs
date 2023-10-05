using System.Collections;
using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameDataManager;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using Unity.VisualScripting;

public enum HeroSlotState
{
    None,          // �⺻ ����
    Lock,          // ���� ��ȹ�� (��� ����)
    Equipped,      // ������
}

public class HeroSlot : MonoBehaviour
{
    private HeroSlotState state;

    public HeroSlotState State
    {
        get { return state; }
        set
        {
            if (state != value)
            {
                state = value;

                HandleStateChange();
            }
        }
    }

    const string atlasName = "HeroIconAtlas";

    public double heroId;
    private string heroName;
    private string grade;

    private Button button;
    private TextMeshProUGUI levelText;
    private Image frameImage;
    private Image iconImage;
    private GameObject selectedImage;
    private GameObject equipText;
    private GameObject lockImage;
    private EventTrigger eventTrigger;

    private bool isSelect = false;
    public bool IsSelect
    {
        get { return isSelect; }
        set
        {
            isSelect = value;
            selectedImage.SetActive(value);
        }
    }

    private bool isClick;
    private float clickTime;
    private float minClickTime = 2.5f;

    private void Awake()
    {
        frameImage = GetComponent<Image>();
        button = GetComponent<Button>();
        eventTrigger = GetComponent<EventTrigger>();

        if (transform.Find("Level_Text").TryGetComponent<TextMeshProUGUI>(out var _levelText))
        {
            levelText = _levelText;
        }

        if (transform.Find("Icon_Image").TryGetComponent<Image>(out var _iconImage))
        {
            iconImage = _iconImage;
        }

        if (transform.Find("Selected_Image").TryGetComponent<Image>(out var _selectedImage))
        {
            selectedImage = _selectedImage.gameObject;
        }

        if (transform.Find("Equip_Text").TryGetComponent<TextMeshProUGUI>(out var _equipText))
        {
            equipText = _equipText.gameObject;
            equipText.SetActive(false);
        }

        if (transform.Find("UpperRightImage/Lock_Image").TryGetComponent<Image>(out var _lockImage))
        {
            lockImage = _lockImage.gameObject;
            lockImage.SetActive(false);
        }
    }

    private void Update()
    {
        if (isClick)
        {
            clickTime += Time.deltaTime;
        }
        else
        {
            clickTime = 0;
        }
    }
    public void Init(double _heroId)
    {
        heroId = _heroId;
        heroName = HeroTemplate[_heroId.ToString()][(int)HeroTemplate_.Name];
        grade = HeroTemplate[_heroId.ToString()][(int)HeroTemplate_.Grade];

        frameImage.color = Util.ConvertGradeToColor(grade);
        iconImage.sprite = CommonFunction.GetSprite_Atlas($"{heroName}_Icon", atlasName);

        SetHeroSlotState();
    }

    public void SetSelected(bool selected)
    {
        IsSelect = selected;
    }

    private void HandleStateChange()
    {
        //State���� �ٲ� ������ ���� �⺻ ���·� �ʱ�ȭ
        equipText.SetActive(false);
        button.interactable = true;
        lockImage.SetActive(false);

        if (State == HeroSlotState.Equipped)
        {
            equipText.SetActive(true);
        }
        else if (State == HeroSlotState.Lock)
        {
            button.interactable = false;
            lockImage.SetActive(true);
            iconImage.color = Util.skillSlot_Lock_Color;
        }
    }

    Dictionary<int, int> userHeroDic = new Dictionary<int, int>();
    public void SetHeroSlotState()
    {
        var userHeroData = GlobalManager.Instance.DBManager.GetUserStringData(UserStringDataType.HeroData).Split('@');
        var currentHeroId = GlobalManager.Instance.DBManager.GetUserDoubleData(UserDoubleDataType.CurrentHeroId, 1);

        foreach (string heroData in userHeroData)
        {
            string[] datas = heroData.Split(',');

            int key = int.Parse(datas[0]);
            int value = int.Parse(datas[1]);

            // �̹� Ű�� �����ϸ� ���� ���� ���ؼ� �ٸ� ��쿡�� ������Ʈ
            if (userHeroDic.ContainsKey(key) && userHeroDic[key] != value)
            {
                userHeroDic[key] = value;
            }
            else if (!userHeroDic.ContainsKey(key))
            {
                userHeroDic[key] = value;
            }
        }

        if (!userHeroDic.ContainsKey((int)heroId))
        {
            //���� ���� �ƴ� ���
            State = HeroSlotState.Lock;
            levelText.text = $"Lv 1";
        }
        else
        {
            //���� ���� ���
            if (heroId == currentHeroId)
            {
                //���� ���� ������ ���
                State = HeroSlotState.Equipped;
            }
            else
            {
                //���� ���� �ƴ� ���
                State = HeroSlotState.None;
            }

            levelText.text = $"Lv {userHeroDic[(int)heroId]}";
        }
    }
}
