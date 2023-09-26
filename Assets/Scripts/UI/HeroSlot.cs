using System.Collections;
using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameDataManager;
using System.Collections.Generic;

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
    public string heroId;
    private string heroName;
    private string grade;

    private Button button;
    private TextMeshProUGUI levelText;
    private Image frameImage;
    private Image iconImage;
    private GameObject selectedImage;
    private GameObject equipText;
    private GameObject lockImage;

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

    private void Awake()
    {
        frameImage = GetComponent<Image>();
        button = GetComponent<Button>();

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
        }

        if (transform.Find("UpperRightImage/Lock_Image").TryGetComponent<Image>(out var _lockImage))
        {
            lockImage = _lockImage.gameObject;
        }

        Debug.Log("�׽�Ʈ1");
    }
    public void Init(string _heroId)
    {
        Debug.Log("�׽�Ʈ2");

        heroId = _heroId;
        heroName = HeroTemplate[_heroId][(int)HeroTemplate_.Name];
        grade = HeroTemplate[_heroId][(int)HeroTemplate_.Grade];

        frameImage.color = Util.ConvertGradeToColor(grade);
        iconImage.sprite = CommonFunction.GetSprite_Atlas($"{heroName}_Icon", atlasName);

        {// �����Ǿ� �ִ� �������� üũ
            var currentHeroId = GlobalManager.Instance.DBManager.GetUserDoubleData(UserDoubleDataType.CurrentHeroId, 1);

            if (double.Parse(heroId) == currentHeroId)
            {
                //���� ���� ������ ���
                State = HeroSlotState.Equipped;
            }
        }

        {//���� ���� �������� üũ
            Dictionary<int, int> userHeroDic = new Dictionary<int, int>();
            var userHeroData = GlobalManager.Instance.DBManager.GetUserStringData(UserStringDataType.HeroData).Split('@');

            foreach (string heroData in userHeroData)
            {
                string[] datas = heroData.Split(',');

                int key = int.Parse(datas[0]);
                int value = int.Parse(datas[1]);

                userHeroDic[key] = value;
            }

            if (!userHeroDic.ContainsKey(int.Parse(heroId)))
            {
                //���� ���� �ƴ� ���
                State = HeroSlotState.Lock;
            }
            else
            {
                //���� ���� ���
                levelText.text = $"Lv {userHeroDic[int.Parse(heroId)]}";
            }
        }
    }

    public void SetSelected(bool selected)
    {
        IsSelect = selected;
    }

    private void HandleStateChange()
    {
        Debug.Log($"{equipText}, {button}, {lockImage}");

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
}
