using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameDataManager;

public class Monster : MonoBehaviour
{
    //public double MaxHp { get; private set; } = 100;
    //public double CurrentHp { get; private set; } = 100;
    //public double Attack { get; private set; } = 1;
    [HideInInspector] public double maxHp;
    [HideInInspector] public double currentHp;
    [HideInInspector] public double attack;
    [HideInInspector] public double gold;

    private MonsterController monsterController;
    private BoxCollider2D boxCollider;

    public Action BossMonsterDied;
    private void OnEnable()
    {
        currentHp = maxHp;

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
        boxCollider = GetComponent<BoxCollider2D>();
        if (TryGetComponent<MonsterController>(out var controller))
        {
            monsterController = controller;
        }
    }
    public void SetMonsterStat(int monsterId)
    {
        maxHp = double.Parse(MonsterTemplate[monsterId.ToString()][(int)MonsterTemplate_.Hp]);
        attack = double.Parse(MonsterTemplate[monsterId.ToString()][(int)MonsterTemplate_.Attack]);
        gold = double.Parse(MonsterTemplate[monsterId.ToString()][(int)MonsterTemplate_.Gold]);

        currentHp = maxHp;
    }


    public void TakeDamage(double damageAmount)
    {
        currentHp -= damageAmount;
        
        if (currentHp <= 0)
        {
            MonsterDie();
        }
    }

    private void MonsterDie()
    {
        boxCollider.enabled = false;

        monsterController.SetCurrentMonsterState(MonsterState.Dead);
        MainScene.Instance.GetGoods(gold);

        if (this.gameObject.CompareTag("Boss"))
        {
            BossMonsterDied?.Invoke();
        }
    }
}
