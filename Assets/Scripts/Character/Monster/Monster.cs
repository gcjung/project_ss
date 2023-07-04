using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameDataManager;

public class Monster : MonoBehaviour
{
    //public double MaxHp { get; private set; } = 100;
    //public double CurrentHp { get; private set; } = 100;
    //public double Attack { get; private set; } = 1;
    public double maxHp;
    public double currentHp;
    public double attack;
    public double gold;

    private MonsterController monsterController;
    private BoxCollider2D boxCollider;

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
    public void SetMonsterStat(string monsterName)
    {
        maxHp = double.Parse(MonsterTemplate[monsterName][(int)MonsterTemplate_.Hp]);
        attack = double.Parse(MonsterTemplate[monsterName][(int)MonsterTemplate_.Attack]);
        gold = double.Parse(MonsterTemplate[monsterName][(int)MonsterTemplate_.Gold]);

        currentHp = maxHp;
    }


    public void TakeDamage(double damageAmount)
    {
        currentHp -= damageAmount;
        
        if (currentHp <= 0)
        {
            boxCollider.enabled = false;

            monsterController.SetCurrentMonsterState(MonsterState.Dead);
            MainScene.Instance.GetGoods(gold);
        }
    }
}
