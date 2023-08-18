using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameDataManager;

public class SkillSlot : MonoBehaviour
{
    string skillID;
    bool isEquipped = false;
    bool isLocked = true;
    bool isUpgradeable = false;
  
    public bool IsEqiupped
    {
        get { return isEquipped; }
    }
    public bool IsLocked
    {
        get { return isLocked; }
    }
    public bool IsUpgradeable
    {
        get { return isUpgradeable; }
    }

    int currentLevel = 0;
    int holdingCount = 0;
    int tartgetValue = 0;
    public int CurrentLevel
    {
        get { return currentLevel; }
    }
    public int HoldingCount
    {
        get { return holdingCount; }
    }
    public int TartgetValue
    {
        get { return tartgetValue; }
    }


    public void Init(string skillId)
    {
        skillID = skillId;
        var equippedSkillData = GlobalManager.Instance.DBManager.GetUserStringData(UserStringDataType.EquippedSkill).Split('@');
        var userSkillData = GlobalManager.Instance.DBManager.GetUserStringData(UserStringDataType.SkillData).Split('@');
        //Debug.Log($"{GlobalManager.Instance.DBManager.GetUserStringData(UserStringDataType.SkillObtainCount)}, skillId : {skillId}");

        string[] skillData = userSkillData[int.Parse(skillId)-1].Split(',');
        currentLevel = int.Parse(skillData[0]);    // 스킬 레벨
        holdingCount = int.Parse(skillData[1]);    // 보유 갯수

        tartgetValue = int.Parse(LevelTemplate[currentLevel.ToString()][(int)LevelTemplate_.RequiredQuantity]);

        // 보유갯수
        transform.Find("CurrentValue_Text").GetComponent<TMP_Text>().text = $"{holdingCount}";
        transform.Find("TargetValue_Text").GetComponent<TMP_Text>().text = $"/ {tartgetValue}";
        transform.Find("Slider").GetComponent<Slider>().value = holdingCount / (float)tartgetValue;
        
        if(holdingCount >= tartgetValue)
        {
            
            PossibleUpgrade(true);
            
        }

        // 레벨 텍스트
        transform.Find("Level_Text").GetComponent<TMP_Text>().text = $"LV {currentLevel}";

        if (currentLevel == 1 && holdingCount == 0)        // 스킬 획득 못한 상태
            SetLock(true);
        else        
            SetLock(false);

        // 장착중 표시
        if (Array.IndexOf(equippedSkillData, skillId) >= 0)
            SetEquip(true);
        else if(!isLocked)
            SetEquip(false);

        string grade = SkillTemplate[skillId][(int)SkillTemplate_.Grade];
        string icon = SkillTemplate[skillId][(int)SkillTemplate_.Icon];

        // 등급 색
        GetComponent<Image>().color = Util.ConvertGradeToColor(grade);

        // 스킬 아이콘
        string[] iconDatas = icon.Split('/');
        string spriteName = iconDatas[0];
        string atlasName = iconDatas[1];
        transform.Find("Image").GetComponent<Image>().sprite = CommonFunction.GetSprite_Atlas(spriteName, atlasName);
         
        
        //new Color(87f/255f, 87f/255f, 87f/255f, 1);
    }
    void PossibleUpgrade(bool isPossible)
    {
        if (isPossible)
        {
            isUpgradeable = true;

            transform.Find("Slider/Fill Area/Fill").GetComponent<Image>().color = Color.yellow;
            RectTransform upArrowMark = transform.Find("LevelUp_Image").GetComponent<RectTransform>();
            // category2_UI.DOAnchorPosY(0, 0.3f).SetEase(Ease.OutExpo);
            //sequence = DOTween.Sequence()
            //.OnStart(() => { t.DOAnchorPosY(-20, 0.4f).SetLoops(-1, LoopType.Yoyo); });
            upArrowMark.DOAnchorPosY(-20, 0.4f).SetLoops(-1, LoopType.Yoyo);
            upArrowMark.gameObject.SetActive(true);
        }
        else 
        {
            isUpgradeable = false;
            transform.Find("Slider/Fill Area/Fill").GetComponent<Image>().color = Color.cyan;
            transform.Find("LevelUp_Image").gameObject.SetActive(false);
        }
    }
    public void UpgradeSkill()
    {
        // 1,2@2,3@1,0 테스트용 
        PossibleUpgrade(false);
        currentLevel += 1;
        holdingCount -= tartgetValue;

        transform.Find("CurrentValue_Text").GetComponent<TMP_Text>().text = $"{holdingCount}";
        transform.Find("Level_Text").GetComponent<TMP_Text>().text = $"LV {currentLevel}";

        tartgetValue = int.Parse(LevelTemplate[currentLevel.ToString()][(int)LevelTemplate_.RequiredQuantity]);

        transform.Find("TargetValue_Text").GetComponent<TMP_Text>().text = $"/ {tartgetValue}";
        transform.Find("Slider").GetComponent<Slider>().value = holdingCount / (float)tartgetValue;
    }
    public void SetLock(bool isLock)
    {
        Button equipButton = transform.Find("UpperRightImage").GetComponent<Button>();
        if (isLock)     // 잠금 (스킬 획득전)
        {
            isLocked = true;
            equipButton.interactable = false;
            transform.Find("Equip_Text").gameObject.SetActive(false);
            transform.Find("UpperRightImage/Lock").gameObject.SetActive(true);
            transform.Find("Image").GetComponent<Image>().color = new Color(100f/255f, 100f/255f, 100f/255f, 100f/255f);
        }
        else
        {
            isLocked = false;
            equipButton.interactable = true;
            transform.Find("Equip_Text").gameObject.SetActive(false);
            transform.Find("UpperRightImage/Lock").gameObject.SetActive(false);
            transform.Find("Image").GetComponent<Image>().color = Color.white;
        }
    }
    public void SetEquip(bool isEquip)
    {
        if (isEquip)
        {
            isEquipped = true;
            transform.Find("Equip_Text").gameObject.SetActive(true);
            transform.Find("Image").GetComponent<Image>().color = new Color(1, 1, 1, 100f / 255f);
        }
        else
        {
            isEquipped = false;
            transform.Find("Equip_Text").gameObject.SetActive(false);
            transform.Find("Image").GetComponent<Image>().color = Color.white;
        }
    }
}
