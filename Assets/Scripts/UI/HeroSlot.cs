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

    public string heroId;
    private string heroName;
    private string grade;
    private string atlasName = "HeroIconAtlas";

    private Image frameImage;
    private Image iconImage;
    private GameObject selectedImage;
    private GameObject equipText;

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

        if (transform.Find("Icon_Image").TryGetComponent<Image>(out var _iconImage))
        {
            iconImage = _iconImage;
        }

        if (transform.Find("Selected_Image").TryGetComponent<Image>(out var _selectedImage))
        {
            selectedImage = _selectedImage.gameObject;
        }

        if (transform.Find("Equip_Text").TryGetComponent<Image>(out var _equipText))
        {
            equipText = _equipText.gameObject;
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

    public void SetSelected(bool selected)
    {
        IsSelect = selected;
    }
}
