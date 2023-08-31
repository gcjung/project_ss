using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FireBall : SkillEffect
{
    private int speed = 8;
    private Vector3 moveDir;

    // Update is called once per frame
    void Update()
    {
        transform.position += moveDir * speed * Time.deltaTime;
        LookAtTarget(moveDir);
    }

    IEnumerator CoReturnObject = null;
    private void OnEnable()
    {
        if (CoReturnObject != null)
            StopCoroutine(CoReturnObject);

        CoReturnObject = ReturnObject();
        StartCoroutine(CoReturnObject);
    }
    IEnumerator ReturnObject()
    {
        yield return CommonIEnumerator.WaitForSecond(4f);

        if (gameObject.activeSelf)
        {
            TmpObjectPool.Instance.ReturnToPool(gameObject);
            Debug.Log(gameObject.name + " 시간지나사라짐");
        }
        
    }
    void LookAtTarget(Vector3 dir)             
    {
        transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
    }
    public override void SettingInfo(double damage, double critical, Transform target)
    {
        this.damage = damage;
        this.critical = critical;
        this.target = target;

        if (target != null) 
            moveDir = (target.position - transform.position).normalized;
        else 
            moveDir = Vector3.right;
    }

    public override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Monster>(out var monster))
        {
            if (target == collision.transform || target == null)
            {
                Debug.Log($"파이어볼 target : {target}");
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
                TmpObjectPool.Instance.ReturnToPool(gameObject);
            }
        }
    }

}
