using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour
{
    public int MaxHp { get; private set; } = 30;
    public int CurrentHp { get; private set; } = 30;
    public int Attack { get; private set; } = 1;

    private MonsterController monsterController;

    private BoxCollider2D boxCollider;

    private void OnEnable()
    {
        CurrentHp = MaxHp;

        if (boxCollider != null)
        {
            boxCollider.enabled = true;
        }

        if (monsterController != null)
        {
            monsterController.SetCurrentMonsterState(MonsterState.Idle);
        }
    }
    private void Awake()
    {
        CurrentHp = MaxHp;

        if(TryGetComponent<MonsterController>(out var controller ))
        {
            monsterController = controller;
        }

        boxCollider = GetComponent<BoxCollider2D>();
    }

    public void TakeDamage(int damageAmount)
    {
        CurrentHp -= damageAmount;

        if (CurrentHp <= 0)
        {
            boxCollider.enabled = false;

            monsterController.SetCurrentMonsterState(MonsterState.Dead);
        }
    }
}
