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
    //Unequipped = 1 << 2,  // 미장착중

    Upgradeable = 1 << 2,   // 업그레이드 가능
}

public class SkillSlot : MonoBehaviour
{
    private SkillSlotState slotState;
    public SkillSlotState State
    {
        get { return slotState; }
        set { slotState = value; }
    }
    string skillID;

    int currentLevel = 0;
    int holdingCount = 0;
    int targetValue = 0;
    public int CurrentLevel
    {
        get { return currentLevel; }
    }
    public int HoldingCount
    {
        get { return holdingCount; }
    }

    public void Init(string skillId)
    {
        skillID = skillId;

        RectTransform upArrowMark = transform.Find("LevelUpArrow").GetComponent<RectTransform>();
        upArrowMark.DOLocalMoveY(-10, 0.4f).SetLoops(-1, LoopType.Yoyo);

        var equippedSkillData = GlobalManager.Instance.DBManager.GetUserStringData(UserStringDataType.EquippedSkill).Split('@');
        var userSkillData = GlobalManager.Instance.DBManager.GetUserStringData(UserStringDataType.SkillData).Split('@');

        string[] skillData = userSkillData[int.Parse(skillId)-1].Split(',');
        currentLevel = int.Parse(skillData[0]);    // 스킬 레벨
        holdingCount = int.Parse(skillData[1]);    // 보유 갯수
        
        targetValue = int.Parse(LevelTemplate[currentLevel.ToString()][(int)LevelTemplate_.RequiredQuantity]);

        // 보유갯수
        transform.Find("CurrentValue_Text").GetComponent<TMP_Text>().text = $"{holdingCount}";
        transform.Find("TargetValue_Text").GetComponent<TMP_Text>().text = $"/ {targetValue}";
        transform.Find("Slider").GetComponent<Slider>().value = holdingCount / (float)targetValue;

        // 스킬 획득 못한 상태 (잠금)
        if (currentLevel == 1 && holdingCount == 0)
            State = SkillSlotState.Lock;

        // 업그레이드 가능
        if (holdingCount >= targetValue)
            State |= SkillSlotState.Upgradeable;

        transform.Find("Level_Text").GetComponent<TMP_Text>().text = $"LV {currentLevel}";

        // 장착중 표시
        if (Array.IndexOf(equippedSkillData, skillId) >= 0)
            State |= SkillSlotState.Equipped;

        string grade = SkillTemplate[skillId][(int)SkillTemplate_.Grade];
        string icon = SkillTemplate[skillId][(int)SkillTemplate_.Icon];

        // 등급 색
        GetComponent<Image>().color = Util.ConvertGradeToColor(grade);

        // 스킬 아이콘
        string[] iconDatas = icon.Split('/');
        string spriteName = iconDatas[0];
        string atlasName = iconDatas[1];
        transform.Find("Image").GetComponent<Image>().sprite = CommonFunction.GetSprite_Atlas(spriteName, atlasName);

        SetSlot(State);
    }
    public void SetSlot(SkillSlotState State)
    {
        //Debug.Log($"@@ : {State}, lock : {State.HasFlag(SkillSlotState.Lock)}, equip : {State.HasFlag(SkillSlotState.Equipped)}");
        Button equipButton = transform.Find("UpperRightImage").GetComponent<Button>();

        if(State == SkillSlotState.Lock)
        {
            equipButton.interactable = false;
            transform.Find("UpperRightImage/Equip").GetComponent<Image>().sprite = CommonFunction.GetSprite_Atlas("btn_icon_lock_2", "SkillAtlas");
            transform.Find("Equip_Text").gameObject.SetActive(false);
            Debug.Log("락입니다");
            transform.Find("Image").GetComponent<Image>().color = Util.skillSlot_Lock_Color;
        }
        else if(State.HasFlag(SkillSlotState.Equipped))
        {
            equipButton.interactable = true;
            transform.Find("UpperRightImage/Equip").GetComponent<Image>().sprite = CommonFunction.GetSprite_Atlas("icon_white_close", "SkillAtlas");
            transform.Find("Equip_Text").gameObject.SetActive(true);
            Debug.Log("장착입니다");
            transform.Find("Image").GetComponent<Image>().color = Util.skillSlot_Equip_Color;
        }
        else
        {
            equipButton.interactable = true;
            transform.Find("UpperRightImage/Equip").GetComponent<Image>().sprite = CommonFunction.GetSprite_Atlas("btn_icon_plus", "SkillAtlas");
            transform.Find("Equip_Text").gameObject.SetActive(false);
            Debug.Log("해제입니다");
            transform.Find("Image").GetComponent<Image>().color = Util.skillSlot_Unequip_Color;
        }

        if(State.HasFlag(SkillSlotState.Upgradeable))
        {
            Debug.Log("업글 가능!!!!");
            transform.Find("Slider/Fill Area/Fill").GetComponent<Image>().color = Color.yellow;
            transform.Find("LevelUpArrow").gameObject.SetActive(true);
        }
        else
        {
            Debug.Log("업글 불가@@@");
            transform.Find("Slider/Fill Area/Fill").GetComponent<Image>().color = Color.cyan;
            transform.Find("LevelUpArrow").gameObject.SetActive(false);
        }
    }

    public void UpgradeSkill()
    {
        currentLevel += 1;
        holdingCount -= targetValue;

        transform.Find("CurrentValue_Text").GetComponent<TMP_Text>().text = $"{holdingCount}";
        transform.Find("Level_Text").GetComponent<TMP_Text>().text = $"LV {currentLevel}";

        targetValue = int.Parse(LevelTemplate[currentLevel.ToString()][(int)LevelTemplate_.RequiredQuantity]);

        transform.Find("TargetValue_Text").GetComponent<TMP_Text>().text = $"/ {targetValue}";
        transform.Find("Slider").GetComponent<Slider>().value = holdingCount / (float)targetValue;

        if(holdingCount < targetValue)      // 보유개수가 레벨업 조건개수보다 적으면
            State &= ~SkillSlotState.Upgradeable;

        SetSlot(State);
    }

}
