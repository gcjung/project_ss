using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SkillEffect : MonoBehaviour
{
    
    protected double damage;
    protected double critical;
    protected Transform target;
    ParticleSystem ps;

    public virtual void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }
    public virtual void SettingInfo(double damage, double critical, Transform target)
    {
        //Damage = isCritical ? damage * 1.5f : damage;
        this.damage = damage;
        this.critical = critical;
        this.target = target;
        //Speed = speed;
    }
    public bool CalcCritical(double critical)
    {
        bool isCritical;
        double criticalPercent = Random.value;

        if (criticalPercent <= critical)
        {
            isCritical = true;
        }
        else
        {
            isCritical = false;
        }

        return isCritical;
    }
    public virtual void test()
    {
        Debug.Log("난 스킬이펙트");
    }

    public virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Monster>(out var monster))
        {

        }
    }

    //{
        //Debug.Log(collision.gameObject.name + ", 트리거발동");
        //if (collision.TryGetComponent<Monster>(out var monster))
        //{

        //    string textName = CalcCritical(critical) ? "Damage_Text(Critical)" : "Damage_Text";
        //    GameObject damageText = CommonFunction.GetPrefabInstance(textName, MainScene.Instance.upSidePanel.transform);

        //    Vector2 contactPoint = collision.ClosestPoint(transform.position);

        //    if (damageText.TryGetComponent<RectTransform>(out var rect))
        //    {
        //        rect.position = Camera.main.WorldToScreenPoint(contactPoint);
        //    }

        //    if (damageText.TryGetComponent<TextMeshProUGUI>(out var tmp))
        //    {
        //        tmp.text = Util.BigNumCalculate(damage);
        //    }

        //    damageText.AddComponent<DamageText>();


        //    monster.TakeDamage(damage);
        //    gameObject.SetActive(false);
        //}
    //}


    //private void OnParticleTrigger()
    //{
    //    //Debug.Log(other + ", 트리거");
    //}
    //private void OnParticleCollision(GameObject other)
    //{
    //    Debug.Log(other + ", 파티클콜리전");
    //}

    public void OnParticleSystemStopped()
    {
        if(!ps.main.loop)
            TmpObjectPool.Instance.ReturnToPool(gameObject);
    }
}
