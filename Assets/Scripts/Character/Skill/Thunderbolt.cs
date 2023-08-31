using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Thunderbolt : SkillEffect
{
    LinkedList<Monster> monsterTargetList;
    public override void Start()
    {
        base.Start();
        monsterTargetList = new LinkedList<Monster>();
    }
    public override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Monster>(out var monster))
        {
            if (monsterTargetList.Find(monster) != null) 
            {
                Debug.Log("이미 목록에 있는 몬스터"); // 공격한 몬스터
                return;
            }

            monsterTargetList.AddLast(monster);

            string textName = CalcCritical(critical) ? "Damage_Text(Critical)" : "Damage_Text";
            GameObject damageText = CommonFunction.GetPrefabInstance(textName, MainScene.Instance.upSidePanel.transform);

            Vector2 contactPoint = collision.ClosestPoint(transform.position);

            if (damageText.TryGetComponent<RectTransform>(out var rect))
            {
                rect.position = Camera.main.WorldToScreenPoint(contactPoint);
            }

            if (damageText.TryGetComponent<TextMeshProUGUI>(out var tmp))
            {
                tmp.text = Util.BigNumCalculate(damage);
            }

            damageText.AddComponent<DamageText>();
            monster.TakeDamage(damage);
        }
    }
    public override void test()
    {
        Debug.Log("난 썬더스톰");
    }
    private void OnDisable()
    {
        if (monsterTargetList != null)
            monsterTargetList.Clear();
    }
    //private void OnParticleSystemStopped()
    //{
    //    gameObject.SetActive(false);
    //}
    
}
