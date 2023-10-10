using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameDataManager;
public enum SkillSlotState
{
    None = 0,
    Lock = 1 << 0,          // ��ų ��ȹ�� (��� ����)
    Equipped = 1 << 1,      // ������
    Upgradeable = 1 << 2,   // ���׷��̵� ����
}

public class SkillSlot : MonoBehaviour
{
    public SkillSlotState State { get; set; } = SkillSlotState.None;
    string skillID;

    //int currentLevel = 0;
    //int holdingCount = 0;
    //int targetValue = 0;
    //public int CurrentLevel
    //{
    //    get { return currentLevel; }
    //}
    //public int HoldingCount
    //{
    //    get { return holdingCount; }
    //}
    public int CurrentLevel { get; private set; } = 1;
    public int HoldingCount { get; private set; } = 0;
    public int TargetValue { get; private set; } = 0;

    public void Init(string skillId)
    {
        skillID = skillId;

        RectTransform upArrowMark = transform.Find("LevelUpArrow").GetComponent<RectTransform>();
        upArrowMark.DOLocalMoveY(-10, 0.4f).SetLoops(-1, LoopType.Yoyo);

        var equippedSkillData = GlobalManager.Instance.DBManager.GetUserStringData(UserStringDataType.EquippedSkill).Split('@');
        var userSkillData = GlobalManager.Instance.DBManager.GetUserStringData(UserStringDataType.SkillData).Split('@');

        string[] skillData = userSkillData[int.Parse(skillId)-1].Split(',');
        CurrentLevel = int.Parse(skillData[0]);    // ��ų ����
        HoldingCount = int.Parse(skillData[1]);    // ���� ����

        transform.Find("Level_Text").GetComponent<TMP_Text>().text = $"LV{CurrentLevel}";

        TargetValue = int.Parse(LevelTemplate[CurrentLevel.ToString()][(int)LevelTemplate_.Skill_Item_RequiredQuantity]);

        transform.Find("CurrentValue_Text").GetComponent<TMP_Text>().text = $"{HoldingCount}";
        transform.Find("TargetValue_Text").GetComponent<TMP_Text>().text = $"/ {TargetValue}";
        transform.Find("Slider").GetComponent<Slider>().value = HoldingCount / (float)TargetValue;

        string grade = SkillTemplate[skillId][(int)SkillTemplate_.Grade];
        string[] icon = SkillTemplate[skillId][(int)SkillTemplate_.Icon].Split('/');

        // ��� ��
        GetComponent<Image>().color = Util.ConvertGradeToColor(grade);

        // ��ų ������
        string spriteName = icon[0];
        string atlasName = icon[1];
        transform.Find("Image").GetComponent<Image>().sprite = CommonFunction.GetSprite_Atlas(spriteName, atlasName);

        // ��ų ȹ�� ���� ���� (���)
        if (CurrentLevel == 1 && HoldingCount == 0)
            State = SkillSlotState.Lock;

        // ���׷��̵� ���� Ȯ��
        if (HoldingCount >= TargetValue)
            State |= SkillSlotState.Upgradeable;

        // ������ Ȯ��
        if (Array.IndexOf(equippedSkillData, skillId) >= 0)
            State |= SkillSlotState.Equipped;

        SetSlot(State);
    }
    public void SetSlot(SkillSlotState State)
    {
        if(State == SkillSlotState.Lock)
        {
            transform.Find("UpperRightImage").GetComponent<Button>().interactable = false;
            transform.Find("UpperRightImage/Equip").GetComponent<Image>().sprite = CommonFunction.GetSprite_Atlas("btn_icon_lock_2", "SkillAtlas");
            transform.Find("Equip_Text").gameObject.SetActive(false);
            
            transform.Find("Image").GetComponent<Image>().color = Util.skillSlot_Lock_Color;
        }
        else if(State.HasFlag(SkillSlotState.Equipped))
        {
            transform.Find("UpperRightImage").GetComponent<Button>().interactable = true;
            transform.Find("UpperRightImage/Equip").GetComponent<Image>().sprite = CommonFunction.GetSprite_Atlas("icon_white_close", "SkillAtlas");
            transform.Find("Equip_Text").gameObject.SetActive(true);
            
            transform.Find("Image").GetComponent<Image>().color = Util.skillSlot_Equip_Color;
        }
        else
        {
            transform.Find("UpperRightImage").GetComponent<Button>().interactable = true;
            transform.Find("UpperRightImage/Equip").GetComponent<Image>().sprite = CommonFunction.GetSprite_Atlas("btn_icon_plus", "SkillAtlas");
            transform.Find("Equip_Text").gameObject.SetActive(false);
            
            transform.Find("Image").GetComponent<Image>().color = Util.skillSlot_Unequip_Color;
        }

        if(State.HasFlag(SkillSlotState.Upgradeable))
        {
            transform.Find("Slider/Fill Area/Fill").GetComponent<Image>().color = Color.yellow;
            transform.Find("LevelUpArrow").gameObject.SetActive(true);
        }
        else
        {
            transform.Find("Slider/Fill Area/Fill").GetComponent<Image>().color = Color.cyan;
            transform.Find("LevelUpArrow").gameObject.SetActive(false);
        }
    }

    public void UpgradeSkill()
    {
        var userSkillData = GlobalManager.Instance.DBManager.GetUserStringData(UserStringDataType.SkillData).Split('@');
        
        CurrentLevel += 1;
        HoldingCount -= TargetValue;

        userSkillData[int.Parse(skillID) - 1] = $"{CurrentLevel},{HoldingCount}";
        GlobalManager.Instance.DBManager.UpdateUserData(UserStringDataType.SkillData, string.Join('@', userSkillData));

        transform.Find("CurrentValue_Text").GetComponent<TMP_Text>().text = $"{HoldingCount}";
        transform.Find("Level_Text").GetComponent<TMP_Text>().text = $"LV{CurrentLevel}";

        // ������ ��ǥ ����
        TargetValue = int.Parse(LevelTemplate[CurrentLevel.ToString()][(int)LevelTemplate_.Skill_Item_RequiredQuantity]);

        transform.Find("TargetValue_Text").GetComponent<TMP_Text>().text = $"/ {TargetValue}";
        transform.Find("Slider").GetComponent<Slider>().value = HoldingCount / (float)TargetValue;

        if(HoldingCount < TargetValue)      // ���������� ������ ���ǰ������� ������
            State &= ~SkillSlotState.Upgradeable;

        SetSlot(State);
    }

    public void MaxUpgradeSkill()
    {
        var userSkillData = GlobalManager.Instance.DBManager.GetUserStringData(UserStringDataType.SkillData).Split('@');

        
        while(HoldingCount >= TargetValue)
        {
            CurrentLevel += 1;
            HoldingCount -= TargetValue;

            TargetValue = int.Parse(LevelTemplate[CurrentLevel.ToString()][(int)LevelTemplate_.Skill_Item_RequiredQuantity]);
        }


        userSkillData[int.Parse(skillID) - 1] = $"{CurrentLevel},{HoldingCount}";
        GlobalManager.Instance.DBManager.UpdateUserData(UserStringDataType.SkillData, string.Join('@', userSkillData));

        transform.Find("CurrentValue_Text").GetComponent<TMP_Text>().text = $"{HoldingCount}";
        transform.Find("Level_Text").GetComponent<TMP_Text>().text = $"LV{CurrentLevel}";

        // ������ ��ǥ ����
        //TargetValue = int.Parse(LevelTemplate[CurrentLevel.ToString()][(int)LevelTemplate_.Skill_Item_RequiredQuantity]);

        transform.Find("TargetValue_Text").GetComponent<TMP_Text>().text = $"/ {TargetValue}";
        transform.Find("Slider").GetComponent<Slider>().value = HoldingCount / (float)TargetValue;

        if (HoldingCount < TargetValue)      // ���������� ������ ���ǰ������� ������
            State &= ~SkillSlotState.Upgradeable;

        SetSlot(State);
    }

    public void UpdateSkillSlot()
    {
        var userSkillData = GlobalManager.Instance.DBManager.GetUserStringData(UserStringDataType.SkillData).Split('@');

        string[] skillData = userSkillData[int.Parse(skillID) - 1].Split(',');
        CurrentLevel = int.Parse(skillData[0]);    // ��ų ����
        HoldingCount = int.Parse(skillData[1]);    // ���� ����

        transform.Find("Level_Text").GetComponent<TMP_Text>().text = $"LV{CurrentLevel}";

        TargetValue = int.Parse(LevelTemplate[CurrentLevel.ToString()][(int)LevelTemplate_.Skill_Item_RequiredQuantity]);

        transform.Find("CurrentValue_Text").GetComponent<TMP_Text>().text = $"{HoldingCount}";
        transform.Find("TargetValue_Text").GetComponent<TMP_Text>().text = $"/ {TargetValue}";
        transform.Find("Slider").GetComponent<Slider>().value = HoldingCount / (float)TargetValue;

        // ��ų ȹ�� ���� ���� (���)
        if (CurrentLevel == 1 && HoldingCount == 0)
            State = SkillSlotState.Lock;
        else
            State &= ~SkillSlotState.Lock;

        // ���׷��̵� ���� Ȯ��
        if (HoldingCount >= TargetValue)
            State |= SkillSlotState.Upgradeable;

        SetSlot(State);
    }

}
