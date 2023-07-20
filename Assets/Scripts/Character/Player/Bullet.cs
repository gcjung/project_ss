using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Rigidbody2D rigid;
    public Rigidbody2D Rigid { get { return rigid; } set { rigid = value; } }
    public Vector3 Direction { get; private set; }
    public double Damage { get; private set; }
    public float Speed { get; private set; }

    private void OnEnable()
    {
        Invoke("Off", 4.0f);
    }
    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();        
    }
    private void Update()
    {
        transform.position += Direction * Speed * Time.deltaTime;
    }
    public void CashingInfo(double damage, Vector3 direction, float speed)
    {
        Damage = damage;
        Direction = direction;
        Speed = speed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Monster>(out var monster))
        {
            monster.TakeDamage((int)Damage);
            gameObject.SetActive(false);
        }
    }

    private void Off()
    {
        if (this.gameObject.activeSelf)
            this.gameObject.SetActive(false);
    }
}
