using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameDataManager;
public enum SkillSlotState
{
    None = 0,
    Lock = 1 << 0,          // 스킬 미획득 (잠김 상태)
    Equipped = 1 << 1,      // 장착중
    Upgradeable = 1 << 2,   // 업그레이드 가능
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
        CurrentLevel = int.Parse(skillData[0]);    // 스킬 레벨
        HoldingCount = int.Parse(skillData[1]);    // 보유 갯수

        transform.Find("Level_Text").GetComponent<TMP_Text>().text = $"LV{CurrentLevel}";

        TargetValue = int.Parse(LevelTemplate[CurrentLevel.ToString()][(int)LevelTemplate_.Skill_Item_RequiredQuantity]);

        transform.Find("CurrentValue_Text").GetComponent<TMP_Text>().text = $"{HoldingCount}";
        transform.Find("TargetValue_Text").GetComponent<TMP_Text>().text = $"/ {TargetValue}";
        transform.Find("Slider").GetComponent<Slider>().value = HoldingCount / (float)TargetValue;

        string grade = SkillTemplate[skillId][(int)SkillTemplate_.Grade];
        string[] icon = SkillTemplate[skillId][(int)SkillTemplate_.Icon].Split('/');

        // 등급 색
        GetComponent<Image>().color = Util.ConvertGradeToColor(grade);

        // 스킬 아이콘
        string spriteName = icon[0];
        string atlasName = icon[1];
        transform.Find("Image").GetComponent<Image>().sprite = CommonFunction.GetSprite_Atlas(spriteName, atlasName);

        // 스킬 획득 못한 상태 (잠금)
        if (CurrentLevel == 1 && HoldingCount == 0)
            State = SkillSlotState.Lock;

        // 업그레이드 가능 확인
        if (HoldingCount >= TargetValue)
            State |= SkillSlotState.Upgradeable;

        // 장착중 확인
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

        // 레벨업 목표 개수
        TargetValue = int.Parse(LevelTemplate[CurrentLevel.ToString()][(int)LevelTemplate_.Skill_Item_RequiredQuantity]);

        transform.Find("TargetValue_Text").GetComponent<TMP_Text>().text = $"/ {TargetValue}";
        transform.Find("Slider").GetComponent<Slider>().value = HoldingCount / (float)TargetValue;

        if(HoldingCount < TargetValue)      // 보유개수가 레벨업 조건개수보다 적으면
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

        // 레벨업 목표 개수
        //TargetValue = int.Parse(LevelTemplate[CurrentLevel.ToString()][(int)LevelTemplate_.Skill_Item_RequiredQuantity]);

        transform.Find("TargetValue_Text").GetComponent<TMP_Text>().text = $"/ {TargetValue}";
        transform.Find("Slider").GetComponent<Slider>().value = HoldingCount / (float)TargetValue;

        if (HoldingCount < TargetValue)      // 보유개수가 레벨업 조건개수보다 적으면
            State &= ~SkillSlotState.Upgradeable;

        SetSlot(State);
    }

    public void UpdateSkillSlot()
    {
        var userSkillData = GlobalManager.Instance.DBManager.GetUserStringData(UserStringDataType.SkillData).Split('@');

        string[] skillData = userSkillData[int.Parse(skillID) - 1].Split(',');
        CurrentLevel = int.Parse(skillData[0]);    // 스킬 레벨
        HoldingCount = int.Parse(skillData[1]);    // 보유 갯수

        transform.Find("Level_Text").GetComponent<TMP_Text>().text = $"LV{CurrentLevel}";

        TargetValue = int.Parse(LevelTemplate[CurrentLevel.ToString()][(int)LevelTemplate_.Skill_Item_RequiredQuantity]);

        transform.Find("CurrentValue_Text").GetComponent<TMP_Text>().text = $"{HoldingCount}";
        transform.Find("TargetValue_Text").GetComponent<TMP_Text>().text = $"/ {TargetValue}";
        transform.Find("Slider").GetComponent<Slider>().value = HoldingCount / (float)TargetValue;

        // 스킬 획득 못한 상태 (잠금)
        if (CurrentLevel == 1 && HoldingCount == 0)
            State = SkillSlotState.Lock;
        else
            State &= ~SkillSlotState.Lock;

        // 업그레이드 가능 확인
        if (HoldingCount >= TargetValue)
            State |= SkillSlotState.Upgradeable;

        SetSlot(State);
    }

}
