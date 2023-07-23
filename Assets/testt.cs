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

        Invoke(nameof(SpriteTest), 5f);

    }
    void SpriteTest()
    {
        image = UIManager.instance.transform.Find("Image").GetComponent<Image>();
        //image.sprite = CommonFuntion.GetSprite_Atlas("UI_Skill_Icon_Arrow_Barrage", "SkillAtlas");
        Debug.Log("시작시간 : " + Time.time);
        image.sprite = CommonFuntion.GetSprite_Atlas("UI_Skill_Icon_Arrow_Barrage", "SkillAtlas");
        Debug.Log("끝 시간 : " + Time.time);
    }

}
