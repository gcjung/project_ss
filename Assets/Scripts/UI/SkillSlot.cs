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
    //Unequipped = 1 << 2,  // ��������

    Upgradeable = 1 << 2,   // ���׷��̵� ����
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
        currentLevel = int.Parse(skillData[0]);    // ��ų ����
        holdingCount = int.Parse(skillData[1]);    // ���� ����
        
        targetValue = int.Parse(LevelTemplate[currentLevel.ToString()][(int)LevelTemplate_.RequiredQuantity]);

        // ��������
        transform.Find("CurrentValue_Text").GetComponent<TMP_Text>().text = $"{holdingCount}";
        transform.Find("TargetValue_Text").GetComponent<TMP_Text>().text = $"/ {targetValue}";
        transform.Find("Slider").GetComponent<Slider>().value = holdingCount / (float)targetValue;

        // ��ų ȹ�� ���� ���� (���)
        if (currentLevel == 1 && holdingCount == 0)
            State = SkillSlotState.Lock;

        // ���׷��̵� ����
        if (holdingCount >= targetValue)
            State |= SkillSlotState.Upgradeable;

        transform.Find("Level_Text").GetComponent<TMP_Text>().text = $"LV {currentLevel}";

        // ������ ǥ��
        if (Array.IndexOf(equippedSkillData, skillId) >= 0)
            State |= SkillSlotState.Equipped;

        string grade = SkillTemplate[skillId][(int)SkillTemplate_.Grade];
        string icon = SkillTemplate[skillId][(int)SkillTemplate_.Icon];

        // ��� ��
        GetComponent<Image>().color = Util.ConvertGradeToColor(grade);

        // ��ų ������
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
            Debug.Log("���Դϴ�");
            transform.Find("Image").GetComponent<Image>().color = Util.skillSlot_Lock_Color;
        }
        else if(State.HasFlag(SkillSlotState.Equipped))
        {
            equipButton.interactable = true;
            transform.Find("UpperRightImage/Equip").GetComponent<Image>().sprite = CommonFunction.GetSprite_Atlas("icon_white_close", "SkillAtlas");
            transform.Find("Equip_Text").gameObject.SetActive(true);
            Debug.Log("�����Դϴ�");
            transform.Find("Image").GetComponent<Image>().color = Util.skillSlot_Equip_Color;
        }
        else
        {
            equipButton.interactable = true;
            transform.Find("UpperRightImage/Equip").GetComponent<Image>().sprite = CommonFunction.GetSprite_Atlas("btn_icon_plus", "SkillAtlas");
            transform.Find("Equip_Text").gameObject.SetActive(false);
            Debug.Log("�����Դϴ�");
            transform.Find("Image").GetComponent<Image>().color = Util.skillSlot_Unequip_Color;
        }

        if(State.HasFlag(SkillSlotState.Upgradeable))
        {
            Debug.Log("���� ����!!!!");
            transform.Find("Slider/Fill Area/Fill").GetComponent<Image>().color = Color.yellow;
            transform.Find("LevelUpArrow").gameObject.SetActive(true);
        }
        else
        {
            Debug.Log("���� �Ұ�@@@");
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

        if(holdingCount < targetValue)      // ���������� ������ ���ǰ������� ������
            State &= ~SkillSlotState.Upgradeable;

        SetSlot(State);
    }

}
