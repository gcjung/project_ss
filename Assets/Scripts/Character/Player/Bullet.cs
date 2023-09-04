using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Rigidbody2D rigid;
    public Rigidbody2D Rigid { get { return rigid; } set { rigid = value; } }
    public Vector3 Direction { get; private set; }
    public double Damage { get; private set; }
    public bool IsCritical { get; private set; }
    public float Speed { get; private set; }

    private void OnEnable()
    {
        StartCoroutine(Off());
    }
    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();        
    }
    private void Update()
    {
        transform.position += Direction * Speed * Time.deltaTime;
    }
    public void SettingInfo(double damage, bool isCritical, Vector3 direction, float speed)
    {
        Damage = isCritical ? damage * 1.5f : damage;
        IsCritical = isCritical;
        Direction = direction;
        Speed = speed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Monster>(out var monster))
        {
            //Debug.Log($"{monster.Name}에게 {Damage}만큼 피해");

            Vector3 projectilePosition = transform.position;
            float radius = 0.1f;

            Collider2D[] colliers = Physics2D.OverlapCircleAll(projectilePosition, radius);

            {   //데미지 텍스트 생성
                string textName = IsCritical ? "Damage_Text(Critical)" : "Damage_Text";
                GameObject damageText = CommonFunction.GetPrefabInstance(textName, MainScene.Instance.upSidePanel.transform);

                Vector2 contactPoint = collision.ClosestPoint(transform.position);

                if (damageText.TryGetComponent<RectTransform>(out var rect))
                {
                    rect.position = Camera.main.WorldToScreenPoint(contactPoint);
                }

                if (damageText.TryGetComponent<TextMeshProUGUI>(out var tmp))
                {
                    tmp.text = Util.BigNumCalculate(Damage);
                }

                damageText.AddComponent<DamageText>();
            }

            monster.TakeDamage(Damage);
            gameObject.SetActive(false);
        }
        //if (collision.TryGetComponent<Monster>(out var monster))
        //{
        //    // 충돌한 몬스터를 기준으로 주변의 모든 몬스터를 수집
        //    Vector3 projectilePosition = transform.position;
        //    float radius = 0.1f;
        //    Collider2D[] colliders = Physics2D.OverlapCircleAll(projectilePosition, radius);

        //    // 가장 가까운 몬스터 초기화
        //    Monster closestMonster = null;
        //    float closestDistance = Mathf.Infinity;

        //    foreach (Collider2D col in colliders)
        //    {
        //        if (col.TryGetComponent<Monster>(out var otherMonster))
        //        {
        //            // 현재 몬스터와의 거리 계산
        //            float distance = Vector2.Distance(transform.position, col.transform.position);

        //            // 가장 가까운 몬스터 업데이트
        //            if (distance < closestDistance)
        //            {
        //                closestMonster = otherMonster;
        //                closestDistance = distance;
        //            }
        //        }
        //    }

        //    if (closestMonster != null)
        //    {
        //        // 가장 가까운 몬스터를 타겟으로 삼고 데미지를 입힘
        //        closestMonster.TakeDamage(Damage);

        //        // 데미지 텍스트 생성 및 표시 위치 설정
        //        string textName = IsCritical ? "Damage_Text(Critical)" : "Damage_Text";
        //        GameObject damageText = CommonFunction.GetPrefabInstance(textName, MainScene.Instance.upSidePanel.transform);
        //        Vector2 contactPoint = closestMonster.GetComponent<Collider2D>().ClosestPoint(transform.position);

        //        if (damageText.TryGetComponent<RectTransform>(out var rect))
        //        {
        //            rect.position = Camera.main.WorldToScreenPoint(contactPoint);
        //        }

        //        if (damageText.TryGetComponent<TextMeshProUGUI>(out var tmp))
        //        {
        //            tmp.text = Util.BigNumCalculate(Damage);
        //        }

        //        damageText.AddComponent<DamageText>();

        //        // 투사체 비활성화
        //        gameObject.SetActive(false);
        //    }
        //}
    }

    private IEnumerator Off()
    {
        float delayTime = 4.0f;

        yield return new WaitForSeconds(delayTime);

        if (this.gameObject.activeSelf)
            this.gameObject.SetActive(false);
    }
}
