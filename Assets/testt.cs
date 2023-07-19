using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;

public class testt : MonoBehaviour
{
    public SpriteAtlas atlas;
    public Image testImg;
    public Sprite bindsp;

    private Image image;

    private void Start()
    {
        image = UIManager.instance.transform.Find("Image").GetComponent<Image>();
        image.sprite = CommonFuntion.GetSprite_Atlas("UI_Skill_Icon_Arrow_Barrage","SkillAtlas");
    }
    void test()
    {
        //atlas.GetSprite
    }

}
