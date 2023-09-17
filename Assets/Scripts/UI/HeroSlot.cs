using System.Collections;
using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameDataManager;
public enum HeroSlotState
{
    None = 0,
    Lock = 1 << 0,          // ½ºÅ³ ¹ÌÈ¹µæ (Àá±è »óÅÂ)
    Equipped = 1 << 1,      // ÀåÂøÁß
    Upgradeable = 1 << 2,   // ¾÷±×·¹ÀÌµå °¡´É
}

public class HeroSlot : MonoBehaviour
{
    public HeroSlotState State { get; set; } = HeroSlotState.None;

    private string heroId;
    private string heroName;
    private string grade;
    private string atlasName = "HeroIconAtlas";

    private Image frameImage;
    private Image iconImage;   

    private void Awake()
    {
        frameImage = GetComponent<Image>();

        if (transform.Find("Icon_Image").TryGetComponent<Image>(out var _iconImage))
        {
            iconImage = _iconImage;
        }
    }
    public void Init(string _heroId)
    {
        heroId = _heroId;
        heroName = HeroTemplate[_heroId][(int)HeroTemplate_.Name];
        grade = HeroTemplate[_heroId][(int)HeroTemplate_.Grade];

        frameImage.color = Util.ConvertGradeToColor(grade);
        iconImage.sprite = CommonFunction.GetSprite_Atlas($"{heroName}_Icon", atlasName);
    }
}
