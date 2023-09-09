using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Rigidbody2D rigid;

    private bool isHit = false;
    public Rigidbody2D Rigid { get { return rigid; } set { rigid = value; } }
    public Vector3 Direction { get; private set; }
    public double Damage { get; private set; }
    public bool IsCritical { get; private set; }
    public float Speed { get; private set; }

    private void OnEnable()
    {
        isHit = false;

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
        if (collision.TryGetComponent<Monster>(out var monster) && !isHit)
        {
            isHit = true;

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
    }

    private IEnumerator Off()
    {
        float delayTime = 4.0f;

        yield return new WaitForSeconds(delayTime);

        if (this.gameObject.activeSelf)
            this.gameObject.SetActive(false);
    }
}
