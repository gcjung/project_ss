using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Rigidbody2D rigid;
    public Rigidbody2D Rigid { get { return rigid; } set { rigid = value; } }
    public float Damage { get; private set; }


    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }
    public void CashingDamage(int damage)
    {
        Damage = damage;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Monster>(out var monster))
        {
            monster.TakeDamage((int)Damage);
            gameObject.SetActive(false);
        }
    }
}
